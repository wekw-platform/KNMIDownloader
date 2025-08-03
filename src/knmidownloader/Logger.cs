namespace knmidownloader
{
    internal class Logger
    {

        public int Calls;
        public void Print(string source, string msg)
        {
            ++Calls;
            Console.WriteLine($"[{source}] [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {msg}");
        }
    }
}
