using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;

namespace knmidownloader.Discord
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
        string SystemFile = "system.json";
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
            if (!MainClass.IsDocker && Directory.EnumerateFiles($"{WorkingDir}/sys/").Count() > 1)
            {
                string[] files = Directory.GetFiles($"{WorkingDir}/sys/");
                Console.WriteLine("\n\nChoose a System file to load:\n\n");
                for (int i = 1; i < files.Count(); i++)
                {
                    Console.WriteLine($"{i}. {files[i - 1].Split('/').Last()}");
                }
                int choice;
                while (!(int.TryParse(Console.ReadLine()?.Trim(), out choice) || (choice >= 1 && choice <= files.Count())))
                {
                    Console.WriteLine("That's not a valid option.");
                }
                SystemFile = files[choice - 1].Split('/').Last();
                Console.WriteLine($"Loading System file {SystemFile}");
            }
            if (!File.Exists($"{WorkingDir}/sys/{SystemFile}") && !MainClass.IsDocker)
            {
                Console.WriteLine($"\n\n\nKNMIDownloader Discord Bot Setup\n\nYou are about to set up the KNMIDownloader Discord Bot.\nThe setup will guide you through all the steps.\nWhile setting up, you need to specify things like your Discord Bot's token and the channels you want KNMIDownloader to post to.\n\nPress any key to begin.\n\n");
                Console.ReadLine();
                DiscordBotData data = new DiscordBotData();
                int stepCount = data.GetType().GetProperties().Length;
                for (int i = 0; i < stepCount; i++)
                {
                    Console.WriteLine($"\n\nStep {i + 1} of {stepCount}\nPlease specify a value for {data.GetType().GetProperties()[i].Name}\n");
                    if (i == 0)
                    {
                        data.GetType().GetProperties()[i].SetValue(data, Console.ReadLine());
                    }
                    else
                    {
                        ulong parsed;
                        while (!ulong.TryParse(Console.ReadLine()?.Trim(), out parsed))
                        {
                            Console.WriteLine("Please try again. That's not a valid ID.");
                        }
                        data.GetType().GetProperties()[i].SetValue(data, parsed);
                    }
                    if (i == stepCount - 1)
                    {
                        Console.WriteLine($"\n\nWriting to file...\n");
                        await JsonFileManager.Write(data);
                        await Main();
                    }
                }
            }
            else if (!File.Exists($"{WorkingDir}/sys/{SystemFile}") && MainClass.IsDocker)
            {
                Console.WriteLine("\n\nNo System file found\n\nKNMIDownloader could not locate a System file.\nThis file is used to allow KNMIDownloader to send files to your Discord bot.\nConfigure system.json in the source and then rebuild and restart your Docker container.\n\nFor more information, see README.\n\n");
                Environment.Exit(0);
            }
            else
            {
                await Main();
            }
        }

        private async Task Main()
        {
            DiscordBotData data = JsonFileManager.Read(SystemFile).Result;
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
                    case 5:
                        embed.WithColor(Color.Orange);
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
                embed.AddField("Files deleted", deletedFilesString);
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
            await PostSystemMessage(0, $"Startup<KNMIDownloader-Bot has started.\n\nKNMIDownloader {MainClass.Version}\n(built {MainClass.BuildDate})\n\nOS: {Environment.OSVersion}\n\n.NET version {Environment.Version}\n\nSystem file: {SystemFile}");
            while (Channels.Count < 6)
            {
                // Wait for Channels to be filled
                // Why exactly 6? I just forgot to update it past 6. Other than that I have no clue.
                // But this thing will crash without this loop.
                // Not updated because it didn't crash.
            }
            IsReady = true;
        }

        void UpdateErrors(int hour)
        {
            if (CurrentHour != hour)
            {
                TotalErrors = 0;
            }
            if (TotalErrors < 4 && TotalErrors! > 3)
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
            IsReady = false;
            Logger.Print("DiscordBot/Error", "Stopping the Discord Bot. The amount of posting errors has exceeded three.");
            try
            {
                if (Restarts >= 3)
                {
                    await Client.LogoutAsync();
                    await Client.StopAsync();
                    if (MainClass.IsDocker)
                    {
                        Console.WriteLine("\n\nYour Discord bot has crashed.\n\nKNMIDownloader has stopped because Discord is unreachable.\n\n");
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("\n\nYour Discord bot has crashed.\n\nKNMIDownloader is continuing without the Discord bot.\n\n");
                    }
                }
                else
                {
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

