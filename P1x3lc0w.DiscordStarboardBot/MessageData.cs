using Discord;
using P1x3lc0w.Common;
using System;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal class MessageData
    {
        public ConcurrentHashSet<ulong> StarGivingUsers { get; private set; }
        public ulong? starboardMessageId;
        public ulong userId;
        public ulong channelId;
        public bool isNsfw;
        public DateTimeOffset created;
        public StarboardMessageStatus starboardMessageStatus;

        public int GetStarCount()
            => StarGivingUsers.Count;

        public MessageData()
        {
            StarGivingUsers = new ConcurrentHashSet<ulong>();
        }

        public async Task<IUserMessage> GetStarboardMessageAsync(IUserMessage msg, GuildData guildData = null)
        {
            IGuild guild = (msg.Channel as ITextChannel).Guild;

            if (guildData == null)
                guildData = Data.BotData.guildDictionary[guild.Id];

            if (starboardMessageStatus != StarboardMessageStatus.CREATED || starboardMessageId == null)
            {
                return null;
            }

            bool isNsfw = (msg.Channel as ITextChannel).IsNsfw;

            
            ITextChannel starboardChannel = isNsfw ? await guild.GetTextChannelAsync(guildData.starboardChannelNSFW) : await guild.GetTextChannelAsync(guildData.starboardChannel);

            return await starboardChannel.GetMessageAsync(starboardMessageId.Value) as IUserMessage;
        }
    }
}