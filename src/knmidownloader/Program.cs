using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace knmidownloader
{
    class Program
    {

        public string Version = "1.2.0-rc1";
        //public string BuildDate = "Fill-In-Please";
        public string BuildDate = "2025-07-24";
        public string CurrentDir = Directory.GetCurrentDirectory();
        public string WebAddress = "https://cdn.knmi.nl/knmi";
        public string ProcessArch;
        public string? LatestWeatherMaps;
        public string? LatestWarningMaps;
        public string? LatestCurrentMaps;
        public int BotRestarts;
        public DiscordBot? Bot;
        public List<Files> FileList = new();

        public const int WarningMapsStart = 6;
        public const int CurrentMapsStart = 9;
        //    0 -
        //     v

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting KNMIDownloader");
            Program p = new Program();
            await p.Start(args);
        }

        async Task Start(string[] args)
        {
            ProcessArch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower();
            Console.Title = $"KNMIDownloader {Version}";
            Console.WriteLine($"KNMIDownloader {Version}");
            Console.WriteLine($"{BuildDate}");
            Console.WriteLine($"(c) 2025 wekw.nl");
            bool shouldStartDiscordBot = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "dodiscord")
                {
                    shouldStartDiscordBot = true;
                }
                if (args[i] == "options")
                {
                    Console.WriteLine($"\n\nKNMIDownloader options\n\n\n1. Start with Discord Bot\n2. Start without Discord Bot\n3. Exit\n\n\n");
                    int option = Convert.ToInt32(Console.ReadLine());
                    switch (option)
                    {
                        case 1:
                            shouldStartDiscordBot = true;
                            break;
                        case 2:
                            shouldStartDiscordBot = false;
                            break;
                        case 3:
                            Environment.Exit(0);
                            break;
                    }
                }
            }
            if (shouldStartDiscordBot)
            {
                Print("KNMIDownloader", "Starting Discord Bot...");
                Bot = new DiscordBot();
                await Bot.Start(this, CurrentDir);
                while(!Bot.IsReady)
                {
                    // halt and wait until the bot has started
                }
                Console.Title = $"KNMIDownloader {Version} - {Bot.Client.GetGuild(Bot.SystemServerID).Name}";
            }
            for (int i = 0; i < 15; i++)
            {
                FileList.Add(new Files(this, i));
            }
            List<Task> tasks = new List<Task>();
            tasks.Add(LoopMapsTimer(DownloadWeatherMaps, 0));
            tasks.Add(LoopMapsTimer(DownloadWarningMaps, 1));
            tasks.Add(LoopMapsTimer(DownloadCurrentMaps, 2));
            Task.WaitAll(tasks.ToArray());
        }

        async Task LoopMapsTimer(Action a, int i)
        {
            switch(i)
            {
                case 0:
                    while (true)
                    {
                        _ = Task.Run(a);
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 1, 0).AddMinutes(1).AddSeconds(30);
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
                case 1:
                    while (true)
                    {
                        _ = Task.Run(a);
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0).AddHours(1);
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
                case 2:
                    while (true)
                    {
                        _ = Task.Run(a);
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 1, 0).AddMinutes(1).AddSeconds(30);
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
            }
        }

        async void DownloadWeatherMaps()
        {
            try
            {
                if (!Directory.Exists($"{CurrentDir}/downloads"))
                { 
                    Directory.CreateDirectory($"{CurrentDir}/downloads");
                }
                if (!Directory.Exists($"{CurrentDir}/downloads/weathermaps"))
                {
                    Directory.CreateDirectory($"{CurrentDir}/downloads/weathermaps");
                }
                string lastDownload = LatestWeatherMaps;
                string folderName = $"weathermaps-{DateTime.Now.ToString("yyyy_MM_dd-HHmmss")}";
                LatestWeatherMaps = folderName;
                Directory.CreateDirectory($"{CurrentDir}/downloads/weathermaps/{folderName}");
                for (int i = 0; i < 6; i++)
                {
                    DownloaderClient client = new DownloaderClient(this);
                    await client.DownloadAndCheck(FileList[i], folderName, "weathermaps");
                }
            }
            catch (Exception exception)
            {
                if (Bot != null)
                {
                    await Bot.PostSystemMessage(4, $"Download error/The download system has failed.\n{exception.Message}");
                }
            }
        }

        async void DownloadWarningMaps()
        {
            try
            {
                if (!Directory.Exists($"{CurrentDir}/downloads"))
                {
                    Directory.CreateDirectory($"{CurrentDir}/downloads");
                }
                if (!Directory.Exists($"{CurrentDir}/downloads/warningmaps"))
                {
                    Directory.CreateDirectory($"{CurrentDir}/downloads/warningmaps");
                }
                string lastDownload = LatestWarningMaps;
                string folderName = $"warningmaps-{DateTime.Now.ToString("yyyy_MM_dd-HHmmss")}";
                LatestWarningMaps = folderName;
                Directory.CreateDirectory($"{CurrentDir}/downloads/warningmaps/{folderName}");
                int downloadID = WarningMapsStart;
                for (int i = 0; i < 3; i++)
                {
                    DownloaderClient client = new DownloaderClient(this);
                    await client.DownloadAndCheck(FileList[downloadID], folderName, "warningmaps");
                    ++downloadID;
                }
            }
            catch (Exception exception)
            {
                if (Bot != null)
                {
                    await Bot.PostSystemMessage(4, $"Download error/The download system has failed.\n{exception.Message}");
                }
            }
        }

        async void DownloadCurrentMaps()
        {
            try
            {
                if (!Directory.Exists($"{CurrentDir}/downloads"))
                {
                    Directory.CreateDirectory($"{CurrentDir}/downloads");
                }
                if (!Directory.Exists($"{CurrentDir}/downloads/currentmaps"))
                {
                    Directory.CreateDirectory($"{CurrentDir}/downloads/currentmaps");
                }
                string lastDownload = LatestCurrentMaps;
                string folderName = $"currentmaps-{DateTime.Now.ToString("yyyy_MM_dd-HHmmss")}";
                LatestCurrentMaps = folderName;
                string fileURL = string.Empty;
                Directory.CreateDirectory($"{CurrentDir}/downloads/currentmaps/{folderName}");
                int downloadID = CurrentMapsStart;
                for (int i = 0; i < 6; i++)
                {
                    DownloaderClient client = new DownloaderClient(this);
                    await client.DownloadAndCheck(FileList[downloadID], folderName, "currentmaps");
                    ++downloadID;
                }
            }
            catch (Exception exception)
            {
                if (Bot != null)
                {
                    await Bot.PostSystemMessage(4, $"Download error/The download system has failed.\n{exception.Message}");
                }
            }
        }

        public void Print(string source, string msg)
        {
            Console.WriteLine($"[{source}] [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {msg}");
        }

        public async Task StopDiscordBot()
        {
            Bot = null;
            if (BotRestarts < 4)
            {
                Print("KNMIDownloader", "Starting Discord Bot...");
                Bot = new DiscordBot();
                await Bot.Start(this, CurrentDir);
                while (!Bot.IsReady)
                {
                    // halt and wait until the bot has started
                }
                Console.Title = $"KNMIDownloader {Version} - {Bot.Client.GetGuild(Bot.SystemServerID).Name}";
            }
            ++BotRestarts;
        }
    }
}
