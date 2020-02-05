using System.Collections.Generic;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal class Data
    {
        public static Data BotData { get; private set; }

        public Dictionary<ulong, GuildData> guildDictionary;

        public Data()
        {
            guildDictionary = new Dictionary<ulong, GuildData>();
        }

        public static async Task LoadDataAsync()
        {
            if (Saving.SaveDataExists)
            {
                BotData = await Saving.LoadDataAsync();
            }
            else
            {
                BotData = new Data();
            }
        }
    }
}