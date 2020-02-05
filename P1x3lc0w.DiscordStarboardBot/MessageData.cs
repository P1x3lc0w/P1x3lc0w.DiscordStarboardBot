using System;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal class MessageData
    {
        public uint stars;
        public ulong starboardMessageId;
        public ulong userId;
        public ulong channelId;
        public bool isNsfw;
        public DateTimeOffset created;
    }
}