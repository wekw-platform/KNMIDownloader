﻿using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace knmidownloader
{
    class Program
    {

        public string Version = "1.0.1";
        public string BuildDate = "Fill-In-Please";
        public string CurrentDir = Directory.GetCurrentDirectory();
        public string WebAddress = "https://cdn.knmi.nl/knmi";
        public string? CurrentDate;
        public string? LatestDownloadDir;
        DiscordBot? Bot;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting KNMIDownloader");
            Program p = new Program();
            await p.Start(args);
        }

        async Task Start(string[] args)
        {
            Console.Title = $"KNMIDownloader {Version}";
            Console.WriteLine($"KNMIDownloader {Version}");
            Console.WriteLine($"Built on {BuildDate}");
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
            await Loop(DownloadAll);
        }

        async Task Loop(Action a)
        {
            while (true)
            {
                _ = Task.Run(a);
                DateTime time = DateTime.Now;
                DateTime next = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 1, 0).AddMinutes(1).AddSeconds(30);
                TimeSpan timeBeforeNext = next - time;
                await Task.Delay(timeBeforeNext);
            }
        }

        async void DownloadAll()
        {
            DateTime time = DateTime.Now;
            string sYear = time.Year.ToString();
            string sMonth = time.Month.ToString();
            string sDayOfMonth = time.Day.ToString();
            string sHourOfDay = time.Hour.ToString();
            string sMinuteOfHour = time.Minute.ToString();
            string sSecondOfMinute = time.Second.ToString();
            if (sYear.Length < 2)
            {
                sYear = $"0{sYear}";
            }
            if (sMonth.Length < 2)
            {
                sMonth = $"0{sMonth}";
            }
            if (sDayOfMonth.Length < 2)
            {
                sDayOfMonth = $"0{sDayOfMonth}";
            }
            if (sHourOfDay.Length < 2)
            {
                sHourOfDay = $"0{sHourOfDay}";
            }
            if (sMinuteOfHour.Length < 2)
            {
                sMinuteOfHour = $"0{sMinuteOfHour}";
            }
            if (sSecondOfMinute.Length < 2)
            {
                sSecondOfMinute = $"0{sSecondOfMinute}";
            }
            CurrentDate = $"{sYear}-{sMonth}-{sDayOfMonth} {sHourOfDay}:{sMinuteOfHour}:{sSecondOfMinute}";
            int totalCompleted = 0;
            try
            {
                if (!Directory.Exists($"{CurrentDir}/downloads"))
                { 
                    Directory.CreateDirectory($"{CurrentDir}/downloads");
                }
                string lastDownload = LatestDownloadDir;
                List<string> filesToPost = new List<string>();
                string folderName = $"weathermaps-{sYear}_{sMonth}_{sDayOfMonth}-{sHourOfDay}{sMinuteOfHour}{sSecondOfMinute}";
                LatestDownloadDir = folderName;
                Directory.CreateDirectory($"{CurrentDir}/downloads/{folderName}");
                for (int i = 0; i < 6; i++)
                {
                    DownloaderClient client = new DownloaderClient(this);
                    switch (i)
                    {
                        case 0:
                            {
                                filesToPost.Add($"0;{CurrentDir}/downloads/{folderName}/weather-map.gif");
                                string fileURL = $"{WebAddress}/map/general/weather-map.gif";
                                await client.Download(fileURL, folderName);
                                ++totalCompleted;
                            }
                            break;
                        case 1:
                            {
                                filesToPost.Add($"1;{CurrentDir}/downloads/{folderName}/WWWRADAR_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADAR_loop.gif";
                                await client.Download(fileURL, folderName);
                                ++totalCompleted;
                            }
                            break;
                        case 2:
                            {
                                filesToPost.Add($"2;{CurrentDir}/downloads/{folderName}/WWWRADARLGT_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARLGT_loop.gif";
                                await client.Download(fileURL, folderName);
                                ++totalCompleted;
                            }
                            break;
                        case 3:
                            {
                                filesToPost.Add($"3;{CurrentDir}/downloads/{folderName}/WWWRADARTMP_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARTMP_loop.gif";
                                await client.Download(fileURL, folderName);
                                ++totalCompleted;
                            }
                            break;
                        case 4:
                            {
                                filesToPost.Add($"4;{CurrentDir}/downloads/{folderName}/WWWRADARWIND_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARWIND_loop.gif";
                                await client.Download(fileURL, folderName);
                                ++totalCompleted;
                            }
                            break;
                        case 5:
                            {
                                filesToPost.Add($"5;{CurrentDir}/downloads/{folderName}/WWWRADARBFT_loop.gif");
                                string fileURL = $"{WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARBFT_loop.gif";
                                await client.Download(fileURL, folderName);
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
                        if (!IsDownloadWorthKeeping(folderName, lastDownload).Result)
                        {
                            LatestDownloadDir = lastDownload;
                            try
                            {
                                Directory.Delete($"{CurrentDir}/downloads/{folderName}", true);
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
                                string msg = filepath.Replace($"{CurrentDir}/downloads/", null);
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

        async Task<bool> IsDownloadWorthKeeping(string newDownload, string oldDownload)
        {
            Console.WriteLine($"\nIs {newDownload} worth keeping? Lets see!");
            if (oldDownload == string.Empty || oldDownload == null)
            {
                Console.WriteLine($"Keeping {newDownload}. There is no valid old download to check.");
                return true;
            }
            bool value = false;
            string filePathNew = $"{CurrentDir}/downloads/{newDownload}";
            string filePathOld = $"{CurrentDir}/downloads/{oldDownload}";
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
                    Debug.WriteLine(ex.Message);
                    Console.WriteLine("Exception thrown: " + ex.Message);
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
            Console.WriteLine($"[{source}] [{GetDate()}] {msg}");
        }

        string GetDate()
        {
            DateTime time = DateTime.Now;
            string sYear = time.Year.ToString();
            string sMonth = time.Month.ToString();
            string sDayOfMonth = time.Day.ToString();
            string sHourOfDay = time.Hour.ToString();
            string sMinuteOfHour = time.Minute.ToString();
            string sSecondOfMinute = time.Second.ToString();
            if (sYear.Length < 2)
            {
                sYear = $"0{sYear}";
            }
            if (sMonth.Length < 2)
            {
                sMonth = $"0{sMonth}";
            }
            if (sDayOfMonth.Length < 2)
            {
                sDayOfMonth = $"0{sDayOfMonth}";
            }
            if (sHourOfDay.Length < 2)
            {
                sHourOfDay = $"0{sHourOfDay}";
            }
            if (sMinuteOfHour.Length < 2)
            {
                sMinuteOfHour = $"0{sMinuteOfHour}";
            }
            if (sSecondOfMinute.Length < 2)
            {
                sSecondOfMinute = $"0{sSecondOfMinute}";
            }
            return $"{sYear}-{sMonth}-{sDayOfMonth} {sHourOfDay}:{sMinuteOfHour}:{sSecondOfMinute}";
        }

        public void EndDiscordBot()
        {
            Bot = null;
        }
    }
}
