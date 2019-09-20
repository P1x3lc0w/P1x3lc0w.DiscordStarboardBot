using System;
using System.Collections.Generic;
using System.Text;

namespace P1x3lc0w.DiscordStarboardBot
{
    class GuildData
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
    }
}
