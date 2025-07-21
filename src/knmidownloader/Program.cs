using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace knmidownloader
{
    class Program
    {

        public string Version = "1.1.0-rc4";
        public string BuildDate = "Fill-In-Please";
        public string CurrentDir = Directory.GetCurrentDirectory();
        public string WebAddress = "https://cdn.knmi.nl/knmi";
        public string ProcessArch;
        public string? LatestWeatherMaps;
        public string? LatestWarningMaps;
        public string? LatestCurrentMaps;
        public int BotRestarts;
        DiscordBot? Bot;

        enum Stamps
        {
            WarningMapsStart = 6,
            CurrentMapsStart = 9
            //    0 -
            //     v
        }

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
            Time time = new Time();
            string[] timeStrings = time.GetArray();
            int totalCompleted = 0;
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
                List<string> filesToPost = new List<string>();
                string folderName = $"weathermaps-{timeStrings[0]}_{timeStrings[1]}_{timeStrings[2]}-{timeStrings[3]}{timeStrings[4]}{timeStrings[5]}";
                LatestWeatherMaps = folderName;
                Directory.CreateDirectory($"{CurrentDir}/downloads/weathermaps/{folderName}");
                for (int i = 0; i < 6; i++)
                {
                    DownloaderClient client = new DownloaderClient(this);
                    switch (i)
                    {
                        case 0:
                            {
                                filesToPost.Add($"0;{CurrentDir}/downloads/weathermaps/{folderName}/weather-map.gif");
                                string fileURL = $"{WebAddress}/map/general/weather-map.gif";
                                await client.Download(fileURL, folderName, "weathermaps");
                                ++totalCompleted;
                            }
                            break;
                        case 1:
                            {
                                filesToPost.Add($"1;{CurrentDir}/downloads/weathermaps/{folderName}/WWWRADAR_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADAR_loop.gif";
                                await client.Download(fileURL, folderName, "weathermaps");
                                ++totalCompleted;
                            }
                            break;
                        case 2:
                            {
                                filesToPost.Add($"2;{CurrentDir}/downloads/weathermaps/{folderName}/WWWRADARLGT_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARLGT_loop.gif";
                                await client.Download(fileURL, folderName, "weathermaps");
                                ++totalCompleted;
                            }
                            break;
                        case 3:
                            {
                                filesToPost.Add($"3;{CurrentDir}/downloads/weathermaps/{folderName}/WWWRADARTMP_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARTMP_loop.gif";
                                await client.Download(fileURL, folderName, "weathermaps");
                                ++totalCompleted;
                            }
                            break;
                        case 4:
                            {
                                filesToPost.Add($"4;{CurrentDir}/downloads/weathermaps/{folderName}/WWWRADARWIND_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARWIND_loop.gif";
                                await client.Download(fileURL, folderName, "weathermaps");
                                ++totalCompleted;
                            }
                            break;
                        case 5:
                            {
                                filesToPost.Add($"5;{CurrentDir}/downloads/weathermaps/{folderName}/WWWRADARBFT_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARBFT_loop.gif";
                                await client.Download(fileURL, folderName, "weathermaps");
                                ++totalCompleted;
                            }
                            break;
                        default:
                            client.Dispose();
                            Console.WriteLine("");
                            Console.WriteLine($"No code to run for i = {i} :(");
                            Console.WriteLine("");
                            if (Bot != null)
                            {
                                await Bot.PostSystemMessage(4, $"No code to run for i = {i} :(/yeah");
                            }
                            break;
                    }
                    if (totalCompleted == 6)
                    {
                        if (!IsDownloadWorthKeeping(folderName, lastDownload, 0).Result)
                        {
                            LatestWeatherMaps = lastDownload;
                            try
                            {
                                Directory.Delete($"{CurrentDir}/downloads/weathermaps/{folderName}", true);
                                if (Bot != null)
                                {
                                    await Bot.PostSystemMessage(5, $"Download information/Download {folderName} has been found useless and has been deleted.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            foreach(string path in filesToPost)
                            {
                                string[] content = path.Split(';');
                                int id = Convert.ToInt32(content[0]);
                                string filepath = content[1];
                                string msg = filepath.Replace($"{CurrentDir}/downloads/weathermaps/", null);
                                if (Bot != null)
                                {
                                    await Bot.PostMessage(id, filepath, msg);
                                }
                            }
                        }
                    }
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
            Time time = new Time();
            string[] timeStrings = time.GetArray();
            int totalCompleted = 0;
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
                List<string> filesToPost = new List<string>();
                string folderName = $"warningmaps-{timeStrings[0]}_{timeStrings[1]}_{timeStrings[2]}-{timeStrings[3]}{timeStrings[4]}{timeStrings[5]}";
                LatestWarningMaps = folderName;
                Directory.CreateDirectory($"{CurrentDir}/downloads/warningmaps/{folderName}");
                int downloadID = (int)Stamps.WarningMapsStart;
                for (int i = 0; i < 3; i++)
                {
                    DownloaderClient client = new DownloaderClient(this);
                    filesToPost.Add($"{downloadID};{CurrentDir}/downloads/warningmaps/{folderName}/waarschuwing_land_{i}_new.gif");
                    string fileURL = $"{WebAddress}/map/current/weather/warning/waarschuwing_land_{i}_new.gif";
                    await client.Download(fileURL, folderName, "warningmaps");
                    ++downloadID;
                    ++totalCompleted;
                    if (totalCompleted == 3)
                    {
                        if (!IsDownloadWorthKeeping(folderName, lastDownload, 1).Result)
                        {
                            LatestWarningMaps = lastDownload;
                            try
                            {
                                Directory.Delete($"{CurrentDir}/downloads/warningmaps/{folderName}", true);
                                if (Bot != null)
                                {
                                    await Bot.PostSystemMessage(5, $"Download information/Download {folderName} has been found useless and has been deleted.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            foreach (string path in filesToPost)
                            {
                                string[] content = path.Split(';');
                                int id = Convert.ToInt32(content[0]);
                                string filepath = content[1];
                                string msg = filepath.Replace($"{CurrentDir}/downloads/warningmaps/", null);
                                if (Bot != null)
                                {
                                    await Bot.PostMessage(id, filepath, msg);
                                }
                            }
                        }
                    }
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
            Time time = new Time();
            string[] timeStrings = time.GetArray();
            int totalCompleted = 0;
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
                List<string> filesToPost = new List<string>();
                string folderName = $"currentmaps-{timeStrings[0]}_{timeStrings[1]}_{timeStrings[2]}-{timeStrings[3]}{timeStrings[4]}{timeStrings[5]}";
                LatestCurrentMaps = folderName;
                Directory.CreateDirectory($"{CurrentDir}/downloads/currentmaps/{folderName}");
                int downloadID = (int)Stamps.CurrentMapsStart;
                for (int i = 0; i < 6; i++)
                {
                    DownloaderClient client = new DownloaderClient(this);
                    switch (i)
                    {
                        case 0:
                            {
                                filesToPost.Add($"{downloadID};{CurrentDir}/downloads/currentmaps/{folderName}/temperatuur.png");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/temperatuur.png";
                                await client.Download(fileURL, folderName, "currentmaps");
                                ++downloadID;
                            }
                            break;
                        case 1:
                            {
                                filesToPost.Add($"{downloadID};{CurrentDir}/downloads/currentmaps/{folderName}/windsnelheid.png");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/windsnelheid.png";
                                await client.Download(fileURL, folderName, "currentmaps");
                                ++downloadID;
                            }
                            break;
                        case 2:
                            {
                                filesToPost.Add($"{downloadID};{CurrentDir}/downloads/currentmaps/{folderName}/windkracht.png");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/windkracht.png";
                                await client.Download(fileURL, folderName, "currentmaps");
                                ++downloadID;
                            }
                            break;
                        case 3:
                            {
                                filesToPost.Add($"{downloadID};{CurrentDir}/downloads/currentmaps/{folderName}/maxwindkm.png");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/maxwindkm.png";
                                await client.Download(fileURL, folderName, "currentmaps");
                                ++downloadID;
                            }
                            break;
                        case 4:
                            {
                                filesToPost.Add($"{downloadID};{CurrentDir}/downloads/currentmaps/{folderName}/zicht.png");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/zicht.png";
                                await client.Download(fileURL, folderName, "currentmaps");
                                ++downloadID;
                            }
                            break;
                        case 5:
                            {
                                filesToPost.Add($"{downloadID};{CurrentDir}/downloads/currentmaps/{folderName}/relvocht.png");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/relvocht.png";
                                await client.Download(fileURL, folderName, "currentmaps");
                                ++downloadID;
                            }
                            break;
                    }
                    ++totalCompleted;
                    if (totalCompleted == 6)
                    {
                        if (!IsDownloadWorthKeeping(folderName, lastDownload, 2).Result)
                        {
                            LatestCurrentMaps = lastDownload;
                            try
                            {
                                Directory.Delete($"{CurrentDir}/downloads/currentmaps/{folderName}", true);
                                if (Bot != null)
                                {
                                    await Bot.PostSystemMessage(5, $"Download information/Download {folderName} has been found useless and has been deleted.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            foreach (string path in filesToPost)
                            {
                                string[] content = path.Split(';');
                                int id = Convert.ToInt32(content[0]);
                                string filepath = content[1];
                                string msg = filepath.Replace($"{CurrentDir}/downloads/currentmaps/", null);
                                if (Bot != null)
                                {
                                    await Bot.PostMessage(id, filepath, msg);
                                }
                            }
                        }
                    }
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

        async Task<bool> IsDownloadWorthKeeping(string newDownload, string oldDownload, int type)
        {
            Console.WriteLine($"\nIs {newDownload} worth keeping? Lets see!");
            if (oldDownload == string.Empty || oldDownload == null)
            {
                Console.WriteLine($"Keeping {newDownload}. There is no valid old download to check.");
                return true;
            }
            bool value = false;
            string filePathNew = null;
            string filePathOld = null;
            switch (type)
            {
                case 0:
                    filePathNew = $"{CurrentDir}/downloads/weathermaps/{newDownload}";
                    filePathOld = $"{CurrentDir}/downloads/weathermaps/{oldDownload}";
                    break;
                case 1:
                    filePathNew = $"{CurrentDir}/downloads/warningmaps/{newDownload}";
                    filePathOld = $"{CurrentDir}/downloads/warningmaps/{oldDownload}";
                    break;
                case 2:
                    filePathNew = $"{CurrentDir}/downloads/currentmaps/{newDownload}";
                    filePathOld = $"{CurrentDir}/downloads/currentmaps/{oldDownload}";
                    break;
            }
            var filesInNew = Directory.EnumerateFiles(filePathNew).ToArray();
            var filesInOld = Directory.EnumerateFiles(filePathOld).ToArray();
            int newCount = filesInNew.Length;
            int oldCount = filesInOld.Length;
            if (newCount != oldCount)
            {
                Console.WriteLine($"Keeping {newDownload}. The file count is different.");
                if (Bot != null)
                {
                    await Bot.PostSystemMessage(5, $"Download information/Download {newDownload} has been saved and posted.");
                }
                return true;
            }
            List<string>[] hashes = await checkFiles(filesInNew, filesInOld);
            List<string> newHashes = hashes[0];
            List<string> oldHashes = hashes[1];
            for (int i = 0; i < newHashes.Count; i++)
            {
                if (newHashes[i] != oldHashes[i])
                {
                    Console.WriteLine($"Keeping {newDownload}. The hashes are different.");
                    if (Bot != null)
                    {
                        await Bot.PostSystemMessage(5, $"Download information/Download {newDownload} has been saved and posted.");
                    }
                    return true;
                }
            }
            Console.WriteLine($"Deleting {newDownload}. The hashes are the same as those of the previous download.");
            return value;
        }

        async Task<List<string>[]> checkFiles(string[] filesInNew, string[] filesInOld)
        {
            List<string>[] hashes = new List<string>[2];
            using (SHA256 sha256 = SHA256.Create())
            {
                try
                {
                    List<string> newh = new List<string>();
                    List<string> oldh = new List<string>();
                    foreach (var file in filesInNew)
                    {
                        using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            byte[] bytes = await sha256.ComputeHashAsync(fs);
                            string hash = BitConverter.ToString(bytes);
                            fs.Close();
                            newh.Add(hash);
                        }
                    }
                    foreach (var file in filesInOld)
                    {
                        using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            byte[] bytes = await sha256.ComputeHashAsync(fs);
                            string hash = BitConverter.ToString(bytes);
                            fs.Close();
                            oldh.Add(hash);
                        }
                    }
                    hashes[0] = newh;
                    hashes[1] = oldh;
                    return hashes;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Exception thrown: {ex.Message}");
                    if (Bot != null)
                    {
                        await Bot.PostSystemMessage(4, $"Please restart KNMIDownloader/KNMIDownloader has run into an error that it cannot recover from.\nLeaving the current instance running may result in faulty downloads or system instability.");
                    }
                    throw new Exception("\n\nKNMIDownloader cannot continue due to an error.\nPlease restart KNMIDownloader or try updating it.\n\n");
                }
            }
        }

        public void Print(string source, string msg)
        {
            Time time = new Time();
            Console.WriteLine($"[{source}] [{time.GetDateTime()}] {msg}");
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
