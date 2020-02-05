using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal class GuildData
    {
        public Dictionary<ulong, MessageData> messageData;
        public ulong starboardChannel;
        public ulong starboardChannelNSFW;
        public uint requiredStarCount;

        public GuildData()
        {
            messageData = new Dictionary<ulong, MessageData>();
            requiredStarCount = 3;
        }

        public void UpdateMessagesByUser(SocketGuild guild, ulong userId)
        {
            Parallel.ForEach<KeyValuePair<ulong, MessageData>>(messageData, async msgData =>
            {
                if(msgData.Value.userId == userId)
                {
                    await Starboard.UpdateStarboardMessage(this, await guild.GetTextChannel(msgData.Value.channelId).GetMessageAsync(msgData.Key) as IUserMessage, msgData.Value);
                }
            });
        }
    }
}