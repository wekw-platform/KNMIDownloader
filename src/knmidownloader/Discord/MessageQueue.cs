using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace knmidownloader.Discord
{
    internal class MessageQueue : List<Message>
    {
        public async Task SendAll()
        {
            foreach(Message message in this)
            {
                if (message.Bot.IsReady)
                {
                    await message.Send();
                    if (message.HasFile && File.Exists(message.FilePath) && message.Bot.MainClass.IsDocker)
                    {
                        File.Delete(message.FilePath);
                        // Moved the File deletion from DownloaderClient to here to make sure the file is not deleted before MessageQueue gets the chance to send it.
                    }
                    Remove(message);
                    break;
                }
            }
        }
    }
}
