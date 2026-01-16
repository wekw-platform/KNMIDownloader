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
    }
}
