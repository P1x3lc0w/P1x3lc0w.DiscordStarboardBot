using System;
using System.Collections.Generic;
using System.Text;

namespace P1x3lc0w.DiscordStarboardBot
{
    class Data
    {
        public static Data BotData { get; private set; } = new Data();

        public Dictionary<ulong, GuildData> guildDictionary;

        public Data()
        {
            guildDictionary = new Dictionary<ulong, GuildData>();
        }
    }
}
