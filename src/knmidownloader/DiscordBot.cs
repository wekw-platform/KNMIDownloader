using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;

namespace knmidownloader
{
    internal class DiscordBot
    {
        public DiscordSocketClient? Client;
        Program? MainClass;
        string? WorkingDir;
        public ulong SystemChannelID;
        public ulong SystemServerID;
        List<ulong> Channels = new List<ulong>();
        public bool IsReady;
        public int TotalErrors;
        public int Restarts;
        public int CurrentHour;
        Logger Logger;

        public async Task Start(Program main, string workingdir, Logger logger)
        {
            MainClass = main;
            Logger = logger;
            WorkingDir = workingdir;
            if (IsConversionNeeded().Result)
            {
                Console.WriteLine($"\n\n\nOld KNMIDownloader System files (pre 1.3) have been found.\n\nPlease convert to JSON or set up again to start the Discord Bot.\n\n1. Convert to JSON and start the Discord Bot\n2. Restart Discord Bot Setup\n\n\n");
                int parsed;
                while (!(int.TryParse(Console.ReadLine()?.Trim(), out parsed) || (parsed >= 1 && parsed <= 2)))
                {
                    Console.WriteLine("That's not a valid option.");
                }
                switch (parsed)
                {
                    case 1:
                        await JsonFileManager.ConvertFromOld(WorkingDir);
                        break;
                    case 2:
                        Logger.Print("KNMIDownloader", "Conversion skipped.");
                        break;
                }
            }
            Logger.Print("DiscordBot", "Attempting login...");
            Client = new DiscordSocketClient();
            if (!Directory.Exists($"{WorkingDir}/sys"))
            {
                Directory.CreateDirectory($"{WorkingDir}/sys");
            }
            if (!File.Exists($"{WorkingDir}/sys/system.json"))
            {
                Console.WriteLine($"\n\n\nConfiguration incorrect: system.json does not exist in folder 'sys'.\nSet this file up and restart KNMIDownloader.\n\n");
                Console.ReadLine();
                Environment.Exit(0);
            }
            else
            {
                await Main();
            }
        }

        private async Task Main()
        {
            DiscordBotData data = JsonFileManager.Read().Result;
            await Client.LoginAsync(TokenType.Bot, data.Token);
            await Client.StartAsync();
            Client.Ready += OnReady;
            SystemServerID = data.SystemServer;
            SystemChannelID = data.SystemChannel;
            ulong[] channels = data.ReadChannels();
            for (int i = 0; i < channels.Length; i++)
            {
                Channels.Add(Convert.ToUInt64(channels[i]));
            }
        }

        public async Task PostSystemMessage(int type, string msg)
        {
            try
            {
                SocketGuild guild = Client.GetGuild(SystemServerID);
                var channel = guild.GetChannel(SystemChannelID) as IMessageChannel;
                EmbedBuilder embed = new EmbedBuilder();
                string[] s = msg.Split('<');
                embed.WithTitle(s[0]);
                embed.AddField(s[1], $"Code: {type}");
                switch (type)
                {
                    case 0:
                        embed.WithColor(Color.Green);
                        break;
                    case 4:
                        embed.WithColor(Color.Red);
                        break;
                    default:
                        embed.WithColor(Color.MaxDecimalValue);
                        break;
                }
                await channel.SendMessageAsync(null, false, embed.Build(), null, null, null, null);
            }
            catch (Exception ex)
            {
                ++TotalErrors;
                Console.WriteLine($"\nFailed to post system message.\n{ex.Message}\nRan into {TotalErrors} errors in total this hour.\n");
                UpdateErrors(DateTime.Now.Hour);
            }
            CurrentHour = DateTime.Now.Hour;
        }

        public async Task PostFileSummary(int type, string msg, List<string> kept, List<string> deleted)
        {
            try
            {
                SocketGuild guild = Client.GetGuild(SystemServerID);
                var channel = guild.GetChannel(SystemChannelID) as IMessageChannel;
                EmbedBuilder embed = new EmbedBuilder();
                string[] s = msg.Split('/');
                embed.WithTitle(s[0]);
                string keptFilesString = string.Empty;
                string deletedFilesString = string.Empty;
                foreach (string file in kept)
                {
                    keptFilesString += $"{file}\n";
                }
                foreach (string file in deleted)
                {
                    deletedFilesString += $"{file}\n";
                }
                if (string.IsNullOrEmpty(keptFilesString))
                {
                    keptFilesString += "None";
                }
                if (string.IsNullOrEmpty(deletedFilesString))
                {
                    deletedFilesString += "None";
                }
                embed.AddField("Files kept", keptFilesString);
                embed.AddField("Files ignored", deletedFilesString);
                embed.AddField(s[1], $"Code: {type}");
                embed.WithColor(Color.MaxDecimalValue);
                await channel.SendMessageAsync(null, false, embed.Build(), null, null, null, null);
            }
            catch (Exception ex)
            {
                ++TotalErrors;
                Console.WriteLine($"\nFailed to post system message.\n{ex.Message}\nRan into {TotalErrors} errors in total this hour.\n");
                UpdateErrors(DateTime.Now.Hour);
            }
            CurrentHour = DateTime.Now.Hour;
        }

        public async Task PostMessage(int type, string path, string msg)
        {
            try
            {
                ulong cID;
                cID = Channels[type];
                SocketGuild guild = Client.GetGuild(SystemServerID);
                var channel = guild.GetChannel(cID) as IMessageChannel;
                await channel.SendFileAsync(path, msg);
            }
            catch (Exception ex)
            {
                ++TotalErrors;
                Console.WriteLine($"\nFailed to post message.\n{ex.Message}\nRan into {TotalErrors} errors in total this hour.\n");
                UpdateErrors(DateTime.Now.Hour);
            }
            CurrentHour = DateTime.Now.Hour;
        }

        private async Task OnReady()
        {
            Console.Title = $"KNMIDownloader {MainClass.Version} - {Client.GetGuild(SystemServerID).Name}";
            Logger.Print("DiscordBot", "Discord Bot has started and is ready.");
            await PostSystemMessage(0, $"Startup<KNMIDownloader-Bot has started.\n\nKNMIDownloader {MainClass.Version} (built {MainClass.BuildDate})\n\nOS: {Environment.OSVersion}\n\n.NET version {Environment.Version}");
            while (Channels.Count < 6)
            {
                
            }
            IsReady = true;
        }

        void UpdateErrors(int hour)
        {
            if (CurrentHour != hour)
            {
                TotalErrors = 0;
            }
            if (TotalErrors < 4 && TotalErrors !> 3)
            {
                TryRestart();
            }
            if (TotalErrors > 3)
            {
                Console.WriteLine("Stopping...");
                Environment.Exit(0);
            }
        }

        async void TryRestart()
        {
            // This trash will be reworked dw
            IsReady = false;
            Logger.Print("DiscordBot/Error", "Stopping the Discord Bot. The amount of posting errors has exceeded three.");
            try
            {
                if (Restarts >= 3)
                {
                    await PostSystemMessage(4, "KNMIDownloader has run into an error.<Too many recovery attempts have been made. The Discord Bot will be stopped so that KNMIDownloader can continue saving.");
                    await Client.LogoutAsync();
                    await Client.StopAsync();
                    MainClass.EndDiscordBot();
                }
                else
                {
                    await PostSystemMessage(4, "KNMIDownloader has run into an error.<It will now attempt to recover the Discord Bot.");
                    await Client.LogoutAsync();
                    await Client.StopAsync();
                    try
                    {
                        if (Restarts < 4)
                        {
                            Logger.Print("DiscordBot", "Restarting Discord Bot...");
                            await Main();
                            ++Restarts;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Print("DiscordBot/Error", $"The Discord Bot could not be recovered.\n{ex.Message}");
                        MainClass.EndDiscordBot();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Did an oopsie: {ex.Message}");
            }
        }

        public async Task<bool> IsConversionNeeded()
        {
            if (File.Exists($"{WorkingDir}/sys/discord-token.txt") || File.Exists($"{WorkingDir}/sys/ids.txt"))
            {
                if (File.Exists($"{WorkingDir}/sys/system.json"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
