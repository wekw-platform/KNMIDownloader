using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using knmidownloader.Discord;
using knmidownloader.DataModels;

namespace knmidownloader
{
    class KNMIDownloader
    {

        public readonly string Version = "1.4.0-rc3";
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
            KNMIDownloader p = new();
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
                Logger.Print(this, "Starting Discord Bot...", 0);
                await Bot.Start(this, CurrentDir, Logger);
            }
            for (int i = 0; i < 20; i++)
            {
                FileList.Add(new MapFile(this, i));
            }
            List<Task> tasks = new List<Task>();
            tasks.Add(Loop(null, -100));
            tasks.Add(Loop(DownloadWeatherMaps, 0));
            tasks.Add(Loop(DownloadWarningMaps, 1));
            tasks.Add(Loop(DownloadCurrentMaps, 2));
            tasks.Add(Loop(DownloadForecastMaps, 3));
            Task.WaitAll(tasks.ToArray());
        }

        async Task Loop(Action a, int i)
        {
            IntervalData data = null;
            string filetype = string.Empty;
            switch (i)
            {
                case 0:
                    filetype = "weathermaps";
                    break;
                case 1:
                    filetype = "warningmaps";
                    break;
                case 2:
                    filetype = "currentmaps";
                    break;
                case 3:
                    filetype = "forecastmaps";
                    break;
            }
            if (filetype.Length > 0)
            {
                if (File.Exists($"sys/interval/interval-{filetype}.json"))
                {
                    Logger.Print(this, $"Loading interval data for {filetype} from interval-{filetype}.json...", 0);
                    data = JsonFileManager.ReadIntervalData($"interval-{filetype}.json").Result;
                }
            }
            if (data != null && data.Milliseconds < 10000)
            {
                Console.WriteLine($"Interval data for {filetype} has been loaded, but the data is not valid:\nValue {data.GetType().GetProperties()[0].Name} totals less than 10 seconds ({data.Milliseconds / 1000} seconds). Using default values...");
            }
            switch (i)
            {
                case -100:
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
                        if (data == null || data.Milliseconds < 10000)
                        {
                            await Task.Delay(10000);
                        }
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 1, 0).AddMinutes(1).AddSeconds(30);
                        if (data != null)
                        {
                            if (data.Milliseconds >= 10000)
                            {
                                next = time.AddMilliseconds(data.Milliseconds);
                            }
                        }
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
                case 1:
                    while (true)
                    {
                        _ = Task.Run(a);
                        if (data == null || data.Milliseconds < 10000)
                        {
                            await Task.Delay(10000);
                        }
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0).AddHours(1);
                        if (data != null)
                        {
                            if (data.Milliseconds >= 10000)
                            {
                                next = time.AddMilliseconds(data.Milliseconds);
                            }
                        }
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
                case 2:
                    while (true)
                    {
                        _ = Task.Run(a);
                        if (data == null || data.Milliseconds < 10000)
                        {
                            await Task.Delay(10000);
                        }
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 1, 0).AddMinutes(1).AddSeconds(30);
                        if (data != null)
                        {
                            if (data.Milliseconds >= 10000)
                            {
                                next = time.AddMilliseconds(data.Milliseconds);
                            }
                        }
                        TimeSpan timeBeforeNext = next - time;
                        await Task.Delay(timeBeforeNext);
                    }
                case 3:
                    while (true)
                    {
                        _ = Task.Run(a);
                        if (data == null || data.Milliseconds < 10000)
                        {
                            await Task.Delay(10000);
                        }
                        DateTime time = DateTime.Now;
                        DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0).AddHours(2);
                        if (data != null)
                        {
                            if (data.Milliseconds >= 10000)
                            {
                                next = time.AddMilliseconds(data.Milliseconds);
                            }
                        }
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
