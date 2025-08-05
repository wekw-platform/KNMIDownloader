namespace knmidownloader
{
    internal class Logger
    {

        public void Print(string source, string msg)
        {
            Console.WriteLine($"\n[{source}] [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {msg}\n");
        }
    }
}
