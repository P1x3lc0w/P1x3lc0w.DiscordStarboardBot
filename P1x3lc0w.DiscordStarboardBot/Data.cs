using P1x3lc0w.DiscordStarboardBot.Datafixing;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal class Data
    {
        public static Data BotData { get; private set; }

        public ConcurrentDictionary<ulong, GuildData> guildDictionary;

        public Data()
        {
            guildDictionary = new ConcurrentDictionary<ulong, GuildData>();
        }

        public static async Task LoadDataAsync()
        {
            if (Saving.SaveDataExists)
            {
                Data data = await Saving.LoadDataAsync();
                Datafixer.FixData(data);
                BotData = data;
            }
            else
            {
                BotData = new Data();
            }
        }
    }
}