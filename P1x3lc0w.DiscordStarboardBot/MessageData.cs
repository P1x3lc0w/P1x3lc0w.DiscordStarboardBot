using P1x3lc0w.Common;
using System;

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
    }
}