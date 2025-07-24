using System.Diagnostics;
using System.Security.Cryptography;

namespace knmidownloader
{
    internal class DownloaderClient : HttpClient
    {
        public DownloaderClient(Program main)
        {
            MainClass = main;
        }

        Program MainClass;

        public async Task<string> Download(string url, string folderName, string type)
        {
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
            Console.WriteLine($"\nIs {folderName}/{name} worth keeping? Lets see!");
            if (!await file.IsHashDifferent($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}"))
            {
                try
                {
                    Console.WriteLine($"Deleting {folderName}/{name}. The hash is the same as that of the old file.\n");
                    File.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}");
                    if (MainClass.Bot != null)
                    {
                        await MainClass.Bot.PostSystemMessage(5, $"{folderName}/File {name} has been found useless and has been deleted.");
                    }
                    if (file.ID == file.MaxID && Directory.EnumerateFiles($"{MainClass.CurrentDir}/downloads/{type}/{folderName}").Count() == 0)
                    {
                        Console.WriteLine($"Deleting directory {folderName}. There are no files left in it.\n");
                        Directory.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}", true);
                        if (MainClass.Bot != null)
                        {
                            await MainClass.Bot.PostSystemMessage(5, $"{folderName}/Has been found useless and has been deleted entirely.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown: {ex.Message}");
                }
            }
            else
            {
                string filepath = $"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}";
                string msg = filepath.Replace($"{MainClass.CurrentDir}/downloads/{type}/", null);
                Console.WriteLine($"Keeping {folderName}/{name}. The hash differs.\n");
                if (MainClass.Bot != null)
                {
                    await MainClass.Bot.PostSystemMessage(5, $"{folderName}/File {name} has been posted.");
                    await MainClass.Bot.PostMessage(file.ID, filepath, msg);
                }
            }
        }
    }
}
