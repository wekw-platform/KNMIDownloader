using System.Text.Json;

namespace knmidownloader
{
    internal class JsonFileManager
    {

        static string SystemFileName = "sys/system.json";
        static JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };

        static public async Task<DiscordBotData> Read()
        {
            string content = File.ReadAllText(SystemFileName);
            DiscordBotData data = JsonSerializer.Deserialize<DiscordBotData>(content)!;
            return data;
        }

        public static async Task<ulong[]> ReadChannels(DiscordBotData data)
        {
            ulong[] ids = new ulong[data.GetType().GetProperties().Length - 3];
            for (int i = 0; i < data.GetType().GetProperties().Length; i++)
            {
                if (i > 2)
                {
                    ids[i - 3] = (ulong)data.GetType().GetProperties()[i].GetValue(data)!;
                }
            }
            while (ids.Last() == 0)
            {
                // wait
            }
            return ids;
        }

        public static async Task Write(DiscordBotData data)
        {
            await using FileStream c = File.Create(SystemFileName);
            await JsonSerializer.SerializeAsync(c, data, Options);
        }

        public async Task ConvertFromOld(string workingdir)
        {
            Console.WriteLine("Starting TXT to JSON conversion...");
            DiscordBotData data = new DiscordBotData();
            using (StreamReader reader = new StreamReader($"{workingdir}/sys/discord-token.txt"))
            {
                data.Token = await reader.ReadToEndAsync();
            }
            using (StreamReader reader = new StreamReader($"{workingdir}/sys/ids.txt"))
            {
                string all = await reader.ReadToEndAsync();
                data.SystemServer = Convert.ToUInt64(all.Split('#')[0].Split(':')[0]);
                data.SystemChannel = Convert.ToUInt64(all.Split('#')[0].Split(':')[1]);
                string content = all.Split('#')[1];
                string[] channels = content.Split(':');
                int id = 3;
                for (int i = 0; i < channels.Length; i++)
                {
                    data.GetType().GetProperties()[id].SetValue(data, Convert.ToUInt64(channels[i]));
                    ++id;
                }
            }
            await Write(data);
            Console.WriteLine("TXT to JSON conversion completed.");
        }
    }
}
