using System.Diagnostics;
using System.Security.Cryptography;

namespace knmidownloader
{
    internal class DownloaderClient : HttpClient
    {

        Program MainClass;

        public DownloaderClient(Program main)
        {
            MainClass = main;
        }

        public async Task<string> Download(string url, string folderName, string type)
        {
            if (!Directory.Exists($"{MainClass.CurrentDir}/downloads"))
            {
                Directory.CreateDirectory($"{MainClass.CurrentDir}/downloads");
            }
            if (!Directory.Exists($"{MainClass.CurrentDir}/downloads/{type}"))
            {
                Directory.CreateDirectory($"{MainClass.CurrentDir}/downloads/{type}");
            }
            Directory.CreateDirectory($"{MainClass.CurrentDir}/downloads/{type}/{folderName}");
            string name = url.Split('/').Last();
            await using Stream download = GetStreamAsync(url).Result;
            await using FileStream fs = new FileStream($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}", FileMode.Create, FileAccess.Write);
            await download.CopyToAsync(fs);
            await fs.FlushAsync();
            fs.Close();
            return name;
        }

        public async Task DownloadAndCheck(Files file, string folderName, string type, DownloadSummary summary)
        {
            string name = await Download(file.URL, folderName, type);
            if (!await file.IsHashDifferent($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}"))
            {
                try
                {
                    Console.WriteLine($"\nIgnoring {folderName}/{name}. The hash is the same as that of the old file.\n");
                    File.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}");
                    summary.DeletedFiles.Add(name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nException thrown: {ex.Message}\n");
                }
            }
            else
            {
                summary.KeptFiles.Add(name);
                string filepath = $"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}";
                string msg = filepath.Replace($"{MainClass.CurrentDir}/downloads/{type}/", null);
                Console.WriteLine($"\nSending {folderName}/{name}. The hash differs.\n");
                if (MainClass.Bot != null)
                {
                    if (MainClass.Bot.IsReady && File.Exists(filepath))
                    {
                        await MainClass.Bot.PostMessage(file.ID, filepath, msg);
                        await Task.Delay(10000);
                        File.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}");
                    }
                    else
                    {
                        if (MainClass.Bot.IsReady)
                        {
                            await MainClass.Bot.PostSystemMessage(4, $"Bot not ready or file missing\n\n{filepath}");
                        }
                        else
                        {
                            Console.WriteLine("Please end me");
                        }
                    }
                }
                else
                {
                    File.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}");
                    Console.WriteLine("Quitting...");
                    Environment.Exit(0);
                }
            }
            if (file.ID - file.MinID + 1 == summary.Count)
            {
                List<string>[] collections = summary.BuildSummary();
                string msg = "End of summary";
                if (Directory.EnumerateFiles($"{MainClass.CurrentDir}/downloads/{type}/{folderName}").Count() == 0)
                {
                    msg = "Files not saved on disk. Run the regular version of KNMIDownloader to save files.";
                    Directory.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}", true);
                }
                if (MainClass.Bot != null)
                {
                    if (MainClass.Bot.IsReady)
                    {
                        await MainClass.Bot.PostFileSummary(10, $"Summary for {folderName}/{msg}", collections[0], collections[1]);
                    }
                }
            }
        }
    }
}
