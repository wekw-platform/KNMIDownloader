namespace knmidownloader
{
    internal class Logger
    {
        public void Print(string source, string msg)
        {
            Console.WriteLine($"[{source}] [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {msg}");
        }
    }
}
