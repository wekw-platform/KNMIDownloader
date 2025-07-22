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

        public async Task Download(string url, string folderName, string type)
        {
            string name = url.Split('/').Last();
            await using Stream download = GetStreamAsync(url).Result;
            await using FileStream fs = new FileStream($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}", FileMode.Create, FileAccess.Write);
            await download.CopyToAsync(fs);
            await fs.FlushAsync();
            fs.Close();
        }

        public async Task DownloadAndCheck(string url, string folderName, string type, Hashes hash)
        {
            string name = url.Split('/').Last();
            await using Stream download = GetStreamAsync(url).Result;
            await using FileStream fs = new FileStream($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}", FileMode.Create, FileAccess.Write);
            await download.CopyToAsync(fs);
            await fs.FlushAsync();
            fs.Close();
            Console.WriteLine($"\nIs {folderName}/{name} worth keeping? Lets see!");
            if (!await hash.IsHashDifferent($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}"))
            {
                try
                {
                    File.Delete($"{MainClass.CurrentDir}/downloads/{type}/{folderName}/{name}");
                    Console.WriteLine($"Deleting {folderName}/{name}. The hash is the same as that of the old file.\n");
                    if (MainClass.Bot != null)
                    {
                        await MainClass.Bot.PostSystemMessage(5, $"{folderName}/File {name} has been found useless and has been deleted.");
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
                    await MainClass.Bot.PostMessage(hash.ID, filepath, msg);
                }
            }
        }
    }
}
