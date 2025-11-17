namespace knmidownloader.Discord
{
    internal class DownloadSummary
    {

        private string WorkingDir;

        public string Name { get; set; }

        public List<string> KeptFiles = new();

        public List<string> DeletedFiles = new();

        public int Count;

        public DownloadSummary(int files, string workingdir)
        {
            Count = files + 1;
            WorkingDir = workingdir;
        }

        public List<string>[] BuildSummary()
        {
            List<string>[] collections = new List<string>[2];
            string type = Name.Split('-')[0];
            collections[0] = KeptFiles;
            collections[1] = DeletedFiles;
            return collections;
        }
    }
}
