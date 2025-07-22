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

        public async Task<bool> IsDownloadWorthKeepingA(string fileName, string newDownload, string oldDownload, string type)
        {
            Console.WriteLine($"\nIs {newDownload}/{fileName} worth keeping? Lets see!");
            if (oldDownload == string.Empty || oldDownload == null)
            {
                Console.WriteLine($"Keeping {newDownload}/{fileName}. There is no valid old file to check.");
                return true;
            }
            bool value = false;
            string filePathNew = $"{MainClass.CurrentDir}/downloads/{type}/{newDownload}/{fileName}";
            string filePathOld = $"{MainClass.CurrentDir}/downloads/{type}/{oldDownload}/{fileName}";
            if (!File.Exists($"{MainClass.CurrentDir}/downloads/{type}/{oldDownload}/{fileName}"))
            {
                Console.WriteLine($"Keeping {newDownload}/{fileName}. An old file with that name doesn't exist.");
                if (MainClass.Bot != null)
                {
                    await MainClass.Bot.PostSystemMessage(5, $"{newDownload}/File {fileName} has been saved and posted.");
                }
                return true;
            }
            using (SHA256 sha256 = SHA256.Create())
            {
                string newHash;
                string oldHash;
                try
                {
                    await using (FileStream fs = File.Open($"{MainClass.CurrentDir}/downloads/{type}/{newDownload}/{fileName}", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        byte[] bytes = await sha256.ComputeHashAsync(fs);
                        newHash = BitConverter.ToString(bytes);
                        fs.Close();
                    }
                    await using (FileStream fs = File.Open($"{MainClass.CurrentDir}/downloads/{type}/{oldDownload}/{fileName}", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        byte[] bytes = await sha256.ComputeHashAsync(fs);
                        oldHash = BitConverter.ToString(bytes);
                        fs.Close();
                    }
                    if (newHash != oldHash)
                    {
                        if (MainClass.Bot != null)
                        {
                            await MainClass.Bot.PostSystemMessage(5, $"{newDownload}/File {fileName} has been saved and posted.");
                        }
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown: {ex.Message}");
                    if (MainClass.Bot != null)
                    {
                        await MainClass.Bot.PostSystemMessage(4, $"Please restart KNMIDownloader/KNMIDownloader has run into an error that it cannot recover from.\nLeaving the current instance running may result in faulty downloads or system instability.");
                    }
                    throw new Exception("\n\nKNMIDownloader cannot continue due to an error.\nPlease restart KNMIDownloader or try updating it.\n\n");
                }
            }
            Console.WriteLine($"Deleting {newDownload}/{fileName}. The hash is the same as that of the previous download.");
            return value;
        }
    }
}
