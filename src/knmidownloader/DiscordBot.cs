using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Threading.Channels;

namespace knmidownloader
{
    internal class DiscordBot
    {
        public DiscordSocketClient Client;
        Program MainClass;
        string WorkingDir;
        public ulong SystemChannelID;
        public ulong SystemServerID;
        List<ulong> Channels = new List<ulong>();
        public bool IsReady = false;
        public int TotalErrors;
        public int CurrentHour;

        public async Task Start(Program main, string workingdir)
        {
            this.MainClass = main;
            main.Print("DiscordBot", "Logging in...");
            Client = new DiscordSocketClient();
            if (!Directory.Exists($"{workingdir}/sys"))
            {
                Directory.CreateDirectory($"{workingdir}/sys");
            }
            if (!File.Exists($"{workingdir}/sys/ids.txt"))
            {
                File.CreateText($"{workingdir}/sys/ids.txt");
            }
            if (!File.Exists($"{workingdir}/sys/discord-token.txt"))
            {
                File.CreateText($"{workingdir}/sys/discord-token.txt");
                string path = $@"{workingdir}\sys\discord-token.txt";
                Console.WriteLine($"The KNMIDownloader-Bot system has been set up and can now be used.\n\nPlace your Discord bot's token in the following file and restart the program:\n\n{path}\n\n");
                Console.WriteLine("Press any key to quit");
                Console.ReadLine();
                Environment.Exit(0);
            }
            else
            {
                this.WorkingDir = workingdir;
                await this.Main();
            }
        }

        async Task Main()
        {
            using (StreamReader reader = new StreamReader($"{WorkingDir}/sys/discord-token.txt"))
            {
                var token = reader.ReadToEnd();
                await this.Client.LoginAsync(TokenType.Bot, token);
                await this.Client.StartAsync();
                this.Client.Ready += OnReady;
            }
            using (StreamReader reader = new StreamReader($"{WorkingDir}/sys/ids.txt"))
            {
                string all = reader.ReadToEnd();
                this.SystemServerID = Convert.ToUInt64(all.Split('#')[0].Split(':')[0]);
                this.SystemChannelID = Convert.ToUInt64(all.Split('#')[0].Split(':')[1]);
                string content = all.Split('#')[1];
                string[] channels = content.Split(':');
                for (int i = 0; i < channels.Length; i++)
                {
                    this.Channels.Add(Convert.ToUInt64(channels[i]));
                }
            }
        }

        public async Task PostSystemMessage(int type, string msg)
        {
            try
            {
                SocketGuild guild = Client.GetGuild(this.SystemServerID);
                var channel = guild.GetChannel(this.SystemChannelID) as IMessageChannel;
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
            this.CurrentHour = DateTime.Now.Hour;
        }

        public async Task PostMessage(int type, string path, string msg)
        {
            try
            {
                ulong cID;
                cID = Channels[type];
                SocketGuild guild = Client.GetGuild(this.SystemServerID);
                var channel = guild.GetChannel(cID) as IMessageChannel;
                await channel.SendFileAsync(path, msg);
            }
            catch (Exception ex)
            {
                ++TotalErrors;
                Console.WriteLine($"\nFailed to post system message.\n{ex.Message}\nRan into {TotalErrors} errors in total this hour.\n");
                UpdateErrors(DateTime.Now.Hour);
            }
            this.CurrentHour = DateTime.Now.Hour;
        }

        private async Task OnReady()
        {
            MainClass.Print("DiscordBot", "Discord bot has started and is ready.");
            await PostSystemMessage(0, $"Startup/KNMIDownloader-Bot has started.\n\nKNMIDownloader {this.MainClass.Version} (built {this.MainClass.BuildDate})\n\nOS: {Environment.OSVersion}\n\n.NET version {Environment.Version}");
            while (Channels.Count < 6)
            {
                
            }
            this.IsReady = true;
        }

        void UpdateErrors(int hour)
        {
            if (CurrentHour != hour)
            {
                TotalErrors = 0;
            }
            if (TotalErrors > 3)
            {
                Stop();
            }
        }

        void Stop()
        {
            MainClass.Print("DiscordBot/Error", "Stopping the Discord Bot. The amount of posting errors has exceeded three.");
            try
            {
                Client.LogoutAsync();
                Client.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Did an oopsie: {ex.Message}");
            }
            MainClass.EndDiscordBot();
        }
    }
}
