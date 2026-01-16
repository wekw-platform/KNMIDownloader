using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;

namespace knmidownloader.Discord
{
    internal class Message
    {

        public DiscordBot Bot;
        Embed Embed;
        public string Msg;
        public string FilePath;
        public bool HasFile = false;
        public ulong ChannelId;

        public Message(DiscordBot bot, string msg, Embed embed, bool hasFile, string? filePath, ulong channelId)
        {
            Bot = bot;
            Msg = msg;
            Embed = embed;
            HasFile = hasFile;
            FilePath = filePath;
            ChannelId = channelId;
        }

        public async Task Send()
        {
            try
            {
                SocketGuild guild = Bot.Client.GetGuild(Bot.SystemServerID);
                var channel = guild.GetChannel(ChannelId) as IMessageChannel;
                if (HasFile)
                {
                    await channel.SendFileAsync(FilePath, Msg);
                }
                else if (Embed != null)
                {
                    await channel.SendMessageAsync(null, false, Embed, null, null, null, null);
                }
            }
            catch (HttpException ex)
            {
                Logger.Print(this, $"Could not post message:\n{ex.StackTrace}", 2);
                Console.WriteLine($"\nFailed to post message.\n{ex.Message}\n");
                Console.WriteLine($"\nThe server responded with code \n{ex.HttpCode}\n");
            }
            catch (Exception ex)
            {
                Logger.Print(this, $"Could not post message:\n{ex.GetType()}\n{ex.StackTrace}", 2);
                ++Bot.TotalErrors;
                Console.WriteLine($"\nFailed to post message.\n{ex.Message}\nRan into {Bot.TotalErrors} errors in total this hour.\n");
                Bot.UpdateErrors(DateTime.Now.Hour);
            }
        }
    }
}
