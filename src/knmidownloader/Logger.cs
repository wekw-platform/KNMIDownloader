namespace knmidownloader
{
    internal class Logger
    {

        public static void Print(object source, string msg, int type)
        {
            string infoType = string.Empty;
            switch (type)
            {
                case 0:
                    infoType = "INFO";
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case 1:
                    infoType = "WARN";
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case 2:
                    infoType = "ERROR";
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            string ns = source.GetType().Namespace.Split('.').Last();
            Console.WriteLine($"\n[{ns}/{source.GetType().Name}] [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [{infoType}] {msg}\n");
            Console.ResetColor();
        }

        public static void PrintError(object source, string msg)
        {
            Console.WriteLine("==============================================================");
            Console.WriteLine("KNMIDownloader needs to restart");
            Console.WriteLine("");
            Console.WriteLine("KNMIDownloader ran into a problem that it cannot recover from.");
            Console.WriteLine("If this error appears again, try updating KNMIDownloader.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"What failed: {source.GetType().Name} in {source.GetType().Namespace.Split('.').Last()}");
            Console.WriteLine("");
            switch (source.GetType().Name)
            {
                case "MapFile":
                    {
                        MapFile file = (MapFile)source;
                        Console.WriteLine($"Object data: {file.Id} {file.URL.Split('/').Last()}");
                        break;
                    }
            }
            Console.WriteLine("");
            Console.WriteLine($"Error info: {msg}");
            Console.WriteLine("");
            Console.WriteLine("==============================================================");
            Environment.Exit(-1);
        }
    }
}
