using System;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace knmidownloader
{
    class Program
    {

        public string Version = "1.3.0-jsonformatting1";
        public string BuildDate = "Fill-In-Please";
        public string CurrentDir = Directory.GetCurrentDirectory();
        public string WebAddress = "https://cdn.knmi.nl/knmi";
        public string? ProcessArch;
        public DiscordBot? Bot;
        public List<Files> FileList = new();
        public Logger Logger = new Logger();

        public const int WarningMapsStart = 6;
        public const int CurrentMapsStart = 9;
        public const int ForecastMapsStart = 15;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting KNMIDownloader");
            Program p = new();
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
                if (File.Exists($"{CurrentDir}/sys/discord-token.txt") || File.Exists($"{CurrentDir}/sys/ids.txt"))
                {
                    Console.WriteLine($"\n\n\nOld KNMIDownloader System files (pre 1.3) have been found.\n\nPlease convert to JSON to start the Discord Bot.\n\n1. Convert to JSON and start the Discord Bot\n2. Skip conversion and start KNMIDownloader without the Discord Bot\n\n\n");
                    switch (Convert.ToInt32(Console.ReadLine()))
                    {
                        case 1:
                            JsonFileManager jsonFileManager = new JsonFileManager();
                            await jsonFileManager.ConvertFromOld(CurrentDir);
                            Logger.Print("KNMIDownloader", "Starting Discord Bot...");
                            Bot = new DiscordBot();
                            await Bot.Start(this, CurrentDir, Logger);
                            break;
                        case 2:
                            Logger.Print("KNMIDownloader", "Conversion skipped. Continuing without Discord Bot.");
                            break;
                    }
                }
                else
                {
                    Logger.Print("KNMIDownloader", "Starting Discord Bot...");
                    Bot = new DiscordBot();
                    await Bot.Start(this, CurrentDir, Logger);
                }
            }
            for (int i = 0; i < 19; i++)
            {
                FileList.Add(new Files(this, i));
            }
            List<Task> tasks = new List<Task>();
            tasks.Add(LoopMapsTimer(DownloadWeatherMaps, 0));
            tasks.Add(LoopMapsTimer(DownloadWarningMaps, 1));
            tasks.Add(LoopMapsTimer(DownloadCurrentMaps, 2));
            tasks.Add(LoopMapsTimer(DownloadForecastMaps, 3));
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
                case 3:
                    while (true)
                    {
                        _ = Task.Run(a);
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0).AddHours(2);
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
            }
        }

        async void DownloadWeatherMaps()
        {
            try
            {
                string folderName = $"weathermaps-{DateTime.Now.ToString("yyyy_MM_dd-HHmmss")}";
                DownloaderClient client = new DownloaderClient(this);
                for (int i = 0; i < 6; i++)
                {
                    await client.DownloadAndCheck(FileList[i], folderName, "weathermaps");
                }
            }
            catch (Exception exception)
            {
                if (Bot != null || Bot.IsReady)
                {
                    if (Bot.IsReady)
                    {
                        await Bot.PostSystemMessage(4, $"Download error/The download system has failed.\n{exception.Message}");
                    }
                }
            }
        }

        async void DownloadWarningMaps()
        {
            try
            {
                string folderName = $"warningmaps-{DateTime.Now.ToString("yyyy_MM_dd-HHmmss")}";
                int downloadID = WarningMapsStart;
                DownloaderClient client = new DownloaderClient(this);
                for (int i = 0; i < 3; i++)
                {
                    await client.DownloadAndCheck(FileList[downloadID], folderName, "warningmaps");
                    ++downloadID;
                }
            }
            catch (Exception exception)
            {
                if (Bot != null)
                {
                    if (Bot.IsReady)
                    {
                        await Bot.PostSystemMessage(4, $"Download error/The download system has failed.\n{exception.Message}");
                    }
                }
            }
        }

        async void DownloadCurrentMaps()
        {
            try
            {
                string folderName = $"currentmaps-{DateTime.Now.ToString("yyyy_MM_dd-HHmmss")}";
                int downloadID = CurrentMapsStart;
                DownloaderClient client = new DownloaderClient(this);
                for (int i = 0; i < 6; i++)
                {
                    await client.DownloadAndCheck(FileList[downloadID], folderName, "currentmaps");
                    ++downloadID;
                }
            }
            catch (Exception exception)
            {
                if (Bot != null)
                {
                    if (Bot.IsReady)
                    {
                        await Bot.PostSystemMessage(4, $"Download error/The download system has failed.\n{exception.Message}");
                    }    
                }
            }
        }

        async void DownloadForecastMaps()
        {
            try
            {
                string folderName = $"forecastmaps-{DateTime.Now.ToString("yyyy_MM_dd-HHmmss")}";
                int downloadID = ForecastMapsStart;
                DownloaderClient client = new DownloaderClient(this);
                for (int i = 0; i < 4; i++)
                {
                    await client.DownloadAndCheck(FileList[downloadID], folderName, "forecastmaps");
                    ++downloadID;
                }
            }
            catch (Exception exception)
            {
                if (Bot != null)
                {
                    if (Bot.IsReady)
                    {
                        await Bot.PostSystemMessage(4, $"Download error/The download system has failed.\n{exception.Message}");
                    }
                }
            }
        }

        public void EndDiscordBot()
        {
            Bot = null;
        }
    }
}
