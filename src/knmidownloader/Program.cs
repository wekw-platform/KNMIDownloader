using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using knmidownloader.Discord;

namespace knmidownloader
{
    class Program
    {

        public readonly string Version = "1.4.0-rc1";
        public readonly string BuildDate = "YYYY-MM-DD";
        public readonly string? ProcessArch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().ToLower();
        public string CurrentDir = Directory.GetCurrentDirectory();
        public string WebAddress = "https://cdn.knmi.nl/knmi";
        public DiscordBot? Bot;
        public List<MapFile> FileList = new();
        public Logger Logger = new Logger();

        public const int WarningMapsStart = 6;
        public const int CurrentMapsStart = 9;
        public const int ForecastMapsStart = 16;

        public bool DoUTCOffset = true;
        public bool DoDebugNames;
        public bool IsDocker;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting KNMIDownloader");
            Program p = new();
            await p.Start(args);
        }

        async Task Start(string[] args)
        {
            Console.Title = $"KNMIDownloader {Version}";
            Console.WriteLine($"KNMIDownloader {Version} ({ProcessArch})");
            Console.WriteLine($"{BuildDate}");
            Console.WriteLine($"(c) 2026 wekw.nl");
            bool shouldStartDiscordBot = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "dodiscord":
                        {
                            shouldStartDiscordBot = true;
                            break;
                        }
                    case "docker":
                        {
                            IsDocker = true;
                            break;
                        }
                    case "disableutc":
                        {
                            DoUTCOffset = false;
                            break;
                        }
                    case "debugnames":
                        {
                            DoDebugNames = true;
                            break;
                        }
                    case "options":
                        {
                            Console.WriteLine($"\n\nKNMIDownloader options\n\n\n1. Start with Discord Bot\n2. Start with Discord Bot in Docker mode\n3. Start without Discord Bot\n4. Disable UTC Offset in folder names\n5. Enable Guid in folder names (Debug)\n6. Exit\n\n\n");
                            int parsed;
                            while (!(int.TryParse(Console.ReadLine()?.Trim(), out parsed) && (parsed >= 1 && parsed <= 5)))
                            {
                                Console.WriteLine("That's not a valid option.");
                            }
                            switch (parsed)
                            {
                                case 1:
                                    shouldStartDiscordBot = true;
                                    break;
                                case 2:
                                    shouldStartDiscordBot = true;
                                    IsDocker = true;
                                    break;
                                case 3:
                                    shouldStartDiscordBot = false;
                                    break;
                                case 4:
                                    DoUTCOffset = false;
                                    break;
                                case 5:
                                    DoDebugNames = true;
                                    break;
                                case 6:
                                    Environment.Exit(0);
                                    break;
                            }
                            break;
                        }
                }
            }
            if (shouldStartDiscordBot)
            {
                Bot = new DiscordBot();
                Logger.Print("KNMIDownloader", "Starting Discord Bot...");
                await Bot.Start(this, CurrentDir, Logger);
            }
            for (int i = 0; i < 20; i++)
            {
                FileList.Add(new MapFile(this, i));
            }
            List<Task> tasks = new List<Task>();
            tasks.Add(LoopMapsTimer(null, -1));
            tasks.Add(LoopMapsTimer(DownloadWeatherMaps, 0));
            tasks.Add(LoopMapsTimer(DownloadWarningMaps, 1));
            tasks.Add(LoopMapsTimer(DownloadCurrentMaps, 2));
            tasks.Add(LoopMapsTimer(DownloadForecastMaps, 3));
            Task.WaitAll(tasks.ToArray());
        }

        async Task LoopMapsTimer(Action a, int i)
        {
            switch (i)
            {
                case -1:
                    while (true)
                    {
                        if (Bot != null)
                        {
                            await Task.Run(Bot.MessageQueue.SendAll);
                        }
                        await Task.Delay(1000);
                    }
                case 0:
                    while (true)
                    {
                        _ = Task.Run(a);
                        await Task.Delay(10000);
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 1, 0).AddMinutes(1).AddSeconds(30);
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
                case 1:
                    while (true)
                    {
                        _ = Task.Run(a);
                        await Task.Delay(10000);
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0).AddHours(1);
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
                case 2:
                    while (true)
                    {
                        _ = Task.Run(a);
                        await Task.Delay(10000);
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 1, 0).AddMinutes(1).AddSeconds(30);
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
                case 3:
                    while (true)
                    {
                        _ = Task.Run(a);
                        await Task.Delay(10000);
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
                DateTimeOffset offset = DateTimeOffset.Now;
                DateTime now = DateTime.Now;
                string folderName = $"weathermaps-{now.ToString("yyyy_MM_dd-HHmmss")}{GetUTCOffset()}{GenerateNewGuid()}";
                DownloaderClient client = new DownloaderClient(this);
                DownloadSummary summary = new DownloadSummary(FileList[0].GetTypeFileCount(), CurrentDir);
                summary.Name = folderName;
                for (int i = 0; i < 6; i++)
                {
                    if (FileList[i].ShouldDownload(now))
                    {
                        await client.DownloadAndCheck(FileList[i], folderName, "weathermaps", summary);
                    }
                    else
                    {
                        Console.WriteLine($"Skipped downloading {FileList[i].URL.Split('/').Last()} because conditions don't match.");
                        if (Bot != null)
                        {
                            if (Bot.IsReady)
                            {
                                await Bot.PostSystemMessage(6, $"File skipped<The file {FileList[i].Id} {FileList[i].URL.Split('/').Last()} was skipped because conditions don't match.");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\n\nThere was a problem\n{exception.Message}\n{exception.StackTrace}\n\n");
                if (Bot != null || Bot.IsReady)
                {
                    if (Bot.IsReady)
                    {
                        await Bot.PostSystemMessage(4, $"Download error<The download system has failed.\n{exception.Message}");
                    }
                }
            }
        }

        async void DownloadWarningMaps()
        {
            try
            {
                DateTimeOffset offset = DateTimeOffset.Now;
                DateTime now = DateTime.Now;
                string folderName = $"warningmaps-{now.ToString("yyyy_MM_dd-HHmmss")}{GetUTCOffset()}{GenerateNewGuid()}";
                int downloadID = WarningMapsStart;
                DownloaderClient client = new DownloaderClient(this);
                DownloadSummary summary = new DownloadSummary(FileList[downloadID].GetTypeFileCount(), CurrentDir);
                summary.Name = folderName;
                for (int i = 0; i < 3; i++)
                {
                    if (FileList[downloadID].ShouldDownload(now))
                    {
                        await client.DownloadAndCheck(FileList[downloadID], folderName, "warningmaps", summary);
                        ++downloadID;
                    }
                    else
                    {
                        Console.WriteLine($"\nSkipped downloading {FileList[downloadID].URL.Split('/').Last()} because conditions don't match.\n");
                        if (Bot != null)
                        {
                            if (Bot.IsReady)
                            {
                                await Bot.PostSystemMessage(6, $"File skipped<The file {FileList[downloadID].Id} {FileList[downloadID].URL.Split('/').Last()} was skipped because conditions don't match.");
                            }
                        }
                        ++downloadID;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\n\nThere was a problem\n{exception.Message}\n{exception.StackTrace}\n\n");
                if (Bot != null)
                {
                    if (Bot.IsReady)
                    {
                        await Bot.PostSystemMessage(4, $"Download error<The download system has failed.\n{exception.Message}");
                    }
                }
            }
        }

        async void DownloadCurrentMaps()
        {
            try
            {
                DateTimeOffset offset = DateTimeOffset.Now;
                DateTime now = DateTime.Now;
                string folderName = $"currentmaps-{now.ToString("yyyy_MM_dd-HHmmss")}{GetUTCOffset()}{GenerateNewGuid()}";
                int downloadID = CurrentMapsStart;
                DownloaderClient client = new DownloaderClient(this);
                DownloadSummary summary = new DownloadSummary(FileList[downloadID].GetTypeFileCount(), CurrentDir);
                summary.Name = folderName;
                for (int i = 0; i < 7; i++)
                {
                    if (FileList[downloadID].ShouldDownload(now))
                    {
                        await client.DownloadAndCheck(FileList[downloadID], folderName, "currentmaps", summary);
                        ++downloadID;
                    }
                    else
                    {
                        Console.WriteLine($"Skipped downloading {FileList[downloadID].URL.Split('/').Last()} because conditions don't match.");
                        if (Bot != null)
                        {
                            if (Bot.IsReady)
                            {
                                await Bot.PostSystemMessage(6, $"File skipped<The file {FileList[downloadID].Id} {FileList[downloadID].URL.Split('/').Last()} was skipped because conditions don't match.");
                            }
                        }
                        ++downloadID;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\n\nThere was a problem\n{exception.Message}\n{exception.StackTrace}\n\n");
                if (Bot != null)
                {
                    if (Bot.IsReady)
                    {
                        await Bot.PostSystemMessage(4, $"Download error<The download system has failed.\n{exception.Message}");
                    }    
                }
            }
        }

        async void DownloadForecastMaps()
        {
            try
            {
                DateTimeOffset offset = DateTimeOffset.Now;
                DateTime now = DateTime.Now;
                string folderName = $"forecastmaps-{now.ToString("yyyy_MM_dd-HHmmss")}{GetUTCOffset()}{GenerateNewGuid()}";
                int downloadID = ForecastMapsStart;
                DownloaderClient client = new DownloaderClient(this);
                DownloadSummary summary = new DownloadSummary(FileList[downloadID].GetTypeFileCount(), CurrentDir);
                summary.Name = folderName;
                for (int i = 0; i < 4; i++)
                {
                    if (FileList[downloadID].ShouldDownload(now))
                    {
                        await client.DownloadAndCheck(FileList[downloadID], folderName, "forecastmaps", summary);
                        ++downloadID;
                    }
                    else
                    {
                        Console.WriteLine($"Skipped downloading {FileList[downloadID].URL.Split('/').Last()} because conditions don't match.");
                        ++downloadID;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\n\nThere was a problem\n{exception.Message}\n{exception.StackTrace}\n\n");
                if (Bot != null)
                {
                    if (Bot.IsReady)
                    {
                        await Bot.PostSystemMessage(4, $"Download error<The download system has failed.\n{exception.Message}");
                    }
                }
            }
        }

        string GetUTCOffset()
        {
            if (!DoUTCOffset)
            {
                return string.Empty;
            }
            string s = string.Empty;
            DateTimeOffset dtoffset = DateTimeOffset.Now;
            double offset = dtoffset.Offset.TotalHours;
            switch (offset)
            {
                case < 0:
                    s = $"-utc{offset}";
                    break;
                case > -1:
                    s = $"-utc+{offset}";
                    break;
            }
            return s;
        }

        string GenerateNewGuid()
        {
            if (DoDebugNames)
            {
                Guid guid = Guid.NewGuid();
                return $";{guid.ToString()}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
