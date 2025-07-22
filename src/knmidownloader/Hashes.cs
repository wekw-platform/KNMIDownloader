using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace knmidownloader
{
    internal class Hashes
    {
        public Hashes(Program main, int id)
        {
            MainClass = main;
            ID = id;
        }

        Program MainClass;
        public string LastHash;
        public int ID;

        public async Task<string> GetHash(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string hash;
                try
                {
                    await using (FileStream fs = File.Open($"{filePath}", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        byte[] bytes = await sha256.ComputeHashAsync(fs);
                        hash = BitConverter.ToString(bytes);
                        fs.Close();
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
                return hash;
            }
        }

        public async Task<bool> IsHashDifferent(string filePath)
        {
            bool value = false;
            string oldHash = LastHash;
            if (string.IsNullOrEmpty(oldHash))
            {
                value = true;
            }
            using (SHA256 sha256 = SHA256.Create())
            {
                try
                {
                    await using (FileStream fs = File.Open($"{filePath}", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        byte[] bytes = await sha256.ComputeHashAsync(fs);
                        string hash = BitConverter.ToString(bytes);
                        fs.Close();
                        if (hash != oldHash || value == true)
                        {
                            LastHash = hash;
                            return true;
                        }
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
                return value;
            }
        }
    }
}
