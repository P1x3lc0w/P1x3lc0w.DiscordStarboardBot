using Discord;
using P1x3lc0w.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    static class LeaderboardUtil
    {
        const uint ENTRIES_PER_PAGE = 10;

        public static async Task<string> GetLeaderboardStringAsync(IGuild guild, uint page)
        {
            if (page == 0)
                page = 1;

            GuildData guildData = Data.BotData.guildDictionary[guild.Id];

            IOrderedEnumerable<IGrouping<ulong, KeyValuePair<ulong, MessageData>>> userMessageGroups = 
                from msgKv in guildData.messageData
                group msgKv by msgKv.Value.userId into msgGroup
                orderby msgGroup.Sum(mKv => mKv.Value.GetStarCount()) descending
                select msgGroup;

            uint pageCount = (uint)Math.Ceiling((float)userMessageGroups.Count() / ENTRIES_PER_PAGE);

            if (page > pageCount)
                return $"❌ Page {page} does not exist. There are {pageCount} pages";

            return await GetLeaderboardStringAsync(userMessageGroups.Skip((int)((page-1) * ENTRIES_PER_PAGE)).Take((int)ENTRIES_PER_PAGE), guild, page, pageCount);

        }

        private static async Task<string> GetLeaderboardStringAsync(IEnumerable<IGrouping<ulong, KeyValuePair<ulong, MessageData>>> userMessageGroups, IGuild guild, uint page, uint pageCount)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("✨⭐🌟 🏆 Star Leaderboard 🏆 🌟⭐✨\n\n");

            uint index = 1 + ((page - 1) * ENTRIES_PER_PAGE);
            uint lastIndex = index;
            long lastCount = Int64.MaxValue;

            foreach(IGrouping<ulong, KeyValuePair<ulong, MessageData>> userMsgGroup in userMessageGroups)
            {
                long sum = userMsgGroup.Sum(mKv => mKv.Value.GetStarCount());

                if(sum < lastCount)
                {
                    lastIndex = index;
                    lastCount = sum;
                }

                AddUserLeaderboardRow(
                        await guild.GetUserAsync(userMsgGroup.Key),
                        sum,
                        sb,
                        lastIndex == 1 ? "**🥇" :
                        lastIndex == 2 ? "🥈" :
                        lastIndex == 3 ? "🥉" :
                        lastIndex.ToString() + "th",
                        lastIndex == 1 ? "**" : String.Empty
                    );

                if (lastIndex <= 3)
                    sb.Append('\n');

                index++;
            }

            sb.Append("*Page ");
            sb.Append(page);
            sb.Append(" of ");
            sb.Append(pageCount);
            sb.Append("*");
            return sb.ToString();
        }

        private static void AddUserLeaderboardRow(IGuildUser user, long starCount, StringBuilder sb, string prefix, string suffix)
        {
            sb.Append(prefix);
            sb.Append(" ");

            
            if (user != null)
            {
                sb.Append('`');
                sb.Append((user.Nickname ?? user.Username).StripDiscordCodeBlockChars());
                sb.Append('`');
                sb.Append("*#");
                sb.Append(user.Discriminator);
                sb.Append('*');
            }
            else{
                sb.Append("*<Unknown User>*");
            }

            sb.Append('\t');
            sb.Append(starCount);
            sb.Append('⭐');
            sb.Append(suffix);
            sb.Append('\n');
        }
    }
}
