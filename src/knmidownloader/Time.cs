namespace knmidownloader
{
    internal class Time
    {
        public string GetDateTime()
        {
            DateTime time = DateTime.Now;
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string[] GetArray()
        {
            string[] s = new string[6];
            DateTime time = DateTime.Now;
            s[0] = time.Year.ToString("D2");
            s[1] = time.Month.ToString("D2");
            s[2] = time.Day.ToString("D2");
            s[3] = time.Hour.ToString("D2");
            s[4] = time.Minute.ToString("D2");
            s[5] = time.Second.ToString("D2");
            return s;
        }
    }
}
