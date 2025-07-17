using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace knmidownloader
{
    internal class DownloaderClient : HttpClient
    {
        public DownloaderClient(Program main)
        {
            this.MainClass = main;
        }

        Program MainClass;

        public async Task Download(string url, string folderName)
        {
            string name = url.Split('/').Last();
            await using Stream download = this.GetStreamAsync(url).Result;
            await using FileStream fs = new FileStream($"{MainClass.CurrentDir}/downloads/{folderName}/{name}", FileMode.Create, FileAccess.Write);
            await download.CopyToAsync(fs);
            await fs.FlushAsync();
            fs.Close();
        }
    }
}
