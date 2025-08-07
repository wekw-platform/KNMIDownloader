using System.Security.Cryptography;

namespace knmidownloader
{
    internal class Files
    {

        Program MainClass;
        public string? LastHash;
        public string? URL;
        public string? Type;
        public int ID;
        public int MinID;
        public int MaxID;

        public Files(Program main, int id)
        {
            MainClass = main;
            ID = id;
            SetURLByID(ID);
            SetTypeByID(ID);
        }

        public async Task<string> GetHash(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                try
                {
                    await using (FileStream fs = File.Open($"{filePath}", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        byte[] bytes = await sha256.ComputeHashAsync(fs);
                        string hash = BitConverter.ToString(bytes);
                        fs.Close();
                        return hash;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown: {ex.Message}");
                    if (MainClass.Bot != null)
                    {
                        if (MainClass.Bot.IsReady)
                        {
                            await MainClass.Bot.PostSystemMessage(4, $"Please restart KNMIDownloader/KNMIDownloader has run into an error that it cannot recover from.\nLeaving the current instance running may result in faulty downloads or system instability.");
                        }
                    }
                    throw new Exception("\n\nKNMIDownloader cannot continue due to an error.\nPlease restart KNMIDownloader or try updating it.\n\n");
                }
            }
        }

        public async Task<bool> IsHashDifferent(string filePath)
        {
            bool value = false;
            if (string.IsNullOrEmpty(LastHash))
            {
                value = true;
            }
            string hash = await GetHash(filePath);
            if (hash != LastHash || value == true)
            {
                LastHash = hash;
                return true;
            }
            return value;
        }

        void SetURLByID(int id)
        {
            switch (id)
            {
                case 0:
                    URL = $"{MainClass.WebAddress}/map/general/weather-map.gif";
                    break;
                case 1:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADAR_loop.gif";
                    break;
                case 2:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARLGT_loop.gif";
                    break;
                case 3:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARTMP_loop.gif";
                    break;
                case 4:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARWIND_loop.gif";
                    break;
                case 5:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/neerslagradar/WWWRADARBFT_loop.gif";
                    break;
                case 6:
                    URL = $"{MainClass.WebAddress}/map/current/weather/warning/waarschuwing_land_0_new.gif";
                    break;
                case 7:
                    URL = $"{MainClass.WebAddress}/map/current/weather/warning/waarschuwing_land_1_new.gif";
                    break;
                case 8:
                    URL = $"{MainClass.WebAddress}/map/current/weather/warning/waarschuwing_land_2_new.gif";
                    break;
                case 9:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/temperatuur.png";
                    break;
                case 10:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/windsnelheid.png";
                    break;
                case 11:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/windkracht.png";
                    break;
                case 12:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/maxwindkm.png";
                    break;
                case 13:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/zicht.png";
                    break;
                case 14:
                    URL = $"{MainClass.WebAddress}/map/page/weer/actueel-weer/relvocht.png";
                    break;
                case 15:
                    URL = $"{MainClass.WebAddress}/map/current/weather/forecast/kaart_verwachtingen_Vandaag_nacht.gif";
                    break;
                case 16:
                    URL = $"{MainClass.WebAddress}/map/current/weather/forecast/kaart_verwachtingen_Vandaag_dag.gif";
                    break;
                case 17:
                    URL = $"{MainClass.WebAddress}/map/current/weather/forecast/kaart_verwachtingen_Morgen_nacht.gif";
                    break;
                case 18:
                    URL = $"{MainClass.WebAddress}/map/current/weather/forecast/kaart_verwachtingen_Morgen_dag.gif";
                    break;
            }
        }

        void SetTypeByID(int id)
        {
            switch (id)
            {
                case > 14:
                    Type = "forecastmaps";
                    MinID = 15;
                    MaxID = 18;
                    break;
                case > 8:
                    Type = "currentmaps";
                    MinID = 9;
                    MaxID = 14;
                    break;
                case > 5:
                    Type = "warningmaps";
                    MinID = 6;
                    MaxID = 8;
                    break;
                case > -1:
                    Type = "weathermaps";
                    MinID = 0;
                    MaxID = 5;
                    break;
            }
        }

        public int GetTypeFileCount()
        {
            return MaxID - MinID;
        }
    }
}
