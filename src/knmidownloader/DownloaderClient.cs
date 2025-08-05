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

        public async Task DownloadAndCheck(Files file, string folderName, string type)
        {
            string name = await Download(file.URL, folderName, type);
            if (!await file.IsHashDifferent($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}"))
            {
                try
                {
                    Console.WriteLine($"\nDeleting {folderName}/{name}. The hash is the same as that of the old file.\n");
                    File.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}");
                    if (MainClass.Bot != null)
                    {
                        if (MainClass.Bot.IsReady)
                        {
                            await MainClass.Bot.PostSystemMessage(5, $"{folderName}/File {name} has been found useless and has been deleted.");
                        }
                    }
                    if (file.ID == file.MaxID && Directory.EnumerateFiles($"{MainClass.CurrentDir}/downloads/{type}/{folderName}").Count() == 0)
                    {
                        Console.WriteLine($"\nDeleting directory {folderName}. There are no files left in it.\n");
                        Directory.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}", true);
                        if (MainClass.Bot != null)
                        {
                            if (MainClass.Bot.IsReady)
                            {
                                await MainClass.Bot.PostSystemMessage(5, $"{folderName}/Has been found useless and has been deleted entirely.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nException thrown: {ex.Message}\n");
                }
            }
            else
            {
                string filepath = $"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}";
                string msg = filepath.Replace($"{MainClass.CurrentDir}/downloads/{type}/", null);
                Console.WriteLine($"\nKeeping {folderName}/{name}. The hash differs.\n");
                if (MainClass.Bot != null)
                {
                    if (MainClass.Bot.IsReady)
                    {
                        await MainClass.Bot.PostSystemMessage(5, $"{folderName}/File {name} has been posted.");
                        await MainClass.Bot.PostMessage(file.ID, filepath, msg);
                    }
                }
            }
        }
    }
}
