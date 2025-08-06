namespace knmidownloader
{
    internal class DownloadSummary
    {
        public DownloadSummary(int files, string workingdir)
        {
            Count = files + 1;
            Files = new string[Count];
            WorkingDir = workingdir;
        }

        private string WorkingDir;

        public string Name { get; set; }

        public string[] Files { get; set; }

        public List<string> KeptFiles = new();

        public List<string> DeletedFiles = new();

        public int Count;

        public List<string>[] BuildSummary()
        {
            List<string>[] collections = new List<string>[2];
            string type = Name.Split('-')[0];
            for (int i = 0; i < Count; i++)
            {
                if (File.Exists($"{WorkingDir}/downloads/{type}/{Name}/{Files[i]}"))
                {
                    KeptFiles.Add(Files[i]);
                }
                else
                {
                    DeletedFiles.Add(Files[i]);
                }
            }
            collections[0] = KeptFiles;
            collections[1] = DeletedFiles;
            return collections;
        }
    }
}
