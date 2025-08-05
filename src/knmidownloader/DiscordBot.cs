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
            Logger.Print("DiscordBot", "Attempting login...");
            Client = new DiscordSocketClient();
            if (!Directory.Exists($"{workingdir}/sys"))
            {
                Directory.CreateDirectory($"{workingdir}/sys");
            }
            if (!File.Exists($"{workingdir}/sys/system.json"))
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
                        data.GetType().GetProperties()[i].SetValue(data, Convert.ToUInt64(Console.ReadLine()));
                    }
                    if (i == stepCount - 1)
                    {
                        Console.WriteLine($"\n\nWriting to file...\n");
                        await JsonFileManager.Write(data);
                        WorkingDir = workingdir;
                        await Main();
                    }
                }
            }
            else
            {
                WorkingDir = workingdir;
                await Main();
            }
        }

        async Task Main()
        {
            DiscordBotData data = JsonFileManager.Read().Result;
            await Client.LoginAsync(TokenType.Bot, data.Token);
            await Client.StartAsync();
            Client.Ready += OnReady;
            SystemServerID = data.SystemServer;
            SystemChannelID = data.SystemChannel;
            ulong[] channels = JsonFileManager.ReadChannels(data).Result;
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
                string[] s = msg.Split('/');
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
            await PostSystemMessage(0, $"Startup/KNMIDownloader-Bot has started.\n\nKNMIDownloader {MainClass.Version} (built {MainClass.BuildDate})\n\nOS: {Environment.OSVersion}\n\n.NET version {Environment.Version}");
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
            if (TotalErrors > 3)
            {
                TryRestart();
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
                    await PostSystemMessage(4, "KNMIDownloader has run into an error./Too many recovery attempts have been made. The Discord Bot will be stopped so that KNMIDownloader can continue saving.");
                    await Client.LogoutAsync();
                    await Client.StopAsync();
                    MainClass.EndDiscordBot();
                }
                else
                {
                    await PostSystemMessage(4, "KNMIDownloader has run into an error./It will now attempt to recover the Discord Bot.");
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
    }
}
