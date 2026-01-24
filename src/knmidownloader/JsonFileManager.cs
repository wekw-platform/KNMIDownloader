using System.Text.Json;
using knmidownloader.DataModels;

namespace knmidownloader
{
    internal class JsonFileManager
    {

        static JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };

        public static async Task<DiscordBotData> Read(string systemfile, bool isDocker)
        {
            string content = File.ReadAllText($"sys/{systemfile}");
            DiscordBotData data = JsonSerializer.Deserialize<DiscordBotData>(content)!;
            for (int i = 0; i < data.GetType().GetProperties().Length; i++)
            {
                object StepValue = data.GetType().GetProperties()[i].GetValue(data)!;
                if (StepValue is ulong)
                {
                    ulong v = (ulong)StepValue;
                    if (v == 0)
                    {
                        if (!isDocker)
                        {
                            Console.WriteLine($"Your System file ({systemfile}) does not have a (valid) value for {data.GetType().GetProperties()[i].Name} (unsinged long).\nPlease enter one below:\n\n");
                            ulong parsed;
                            while (!(ulong.TryParse(Console.ReadLine()?.Trim(), out parsed)))
                            {
                                Console.WriteLine("That's not a valid value.");
                            }
                            data.GetType().GetProperties()[i].SetValue(data, parsed);
                            await Write(data, systemfile);
                        }
                        else
                        {
                            Console.WriteLine($"Your System file ({systemfile}) does not have a (valid) value for {data.GetType().GetProperties()[i].Name} (unsigned long).");
                        }
                    }
                }
                if (StepValue is string)
                {
                    string v = (string)StepValue;
                    if (v == string.Empty || v.Length < 17 || v == null) // string length < 17 check to make sure a string value like Token is actually a token and not "YourBotTokenHere" (16 chars)
                    {
                        if (!isDocker)
                        {
                            Console.WriteLine($"Your System file ({systemfile}) does not have a (valid) value for {data.GetType().GetProperties()[i].Name} (string).\nPlease enter one below:\n\n");
                            string value = null;
                            value = Console.ReadLine()?.Trim()!;
                            data.GetType().GetProperties()[i].SetValue(data, value);
                            await Write(data, systemfile);
                        }
                        else
                        {
                            Console.WriteLine($"Your System file ({systemfile}) does not have a (valid) value for {data.GetType().GetProperties()[i].Name} (string).");
                        }
                    }
                }
            }
            return data;
        }

        public static async Task Write(DiscordBotData data, string systemfile)
        {
            await using FileStream fs = File.Create($"sys/{systemfile}");
            await JsonSerializer.SerializeAsync(fs, data, Options);
        }

        public static async Task<ConditionData> ReadConditionData(string conditionfile)
        {
            string content = File.ReadAllText($"sys/condition/{conditionfile}");
            ConditionData data = JsonSerializer.Deserialize<ConditionData>(content)!;
            return data;
        }

        public static async Task<IntervalData> ReadIntervalData(string intervalfile)
        {
            string content = File.ReadAllText($"sys/interval/{intervalfile}");
            IntervalData data = JsonSerializer.Deserialize<IntervalData>(content)!;
            return data;
        }

        public static async Task ConvertFromOld(string workingdir)
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
            await Write(data, "system.json");
            Console.WriteLine("TXT to JSON conversion completed.");
        }
    }
}
