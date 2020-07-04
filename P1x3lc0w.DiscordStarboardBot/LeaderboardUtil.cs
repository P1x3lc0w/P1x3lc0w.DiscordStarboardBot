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
        public static async Task<string> GetLeaderboardStringAsync(IGuild guild)
        {
            GuildData guildData = Data.BotData.guildDictionary[guild.Id];

            IOrderedEnumerable<IGrouping<ulong, KeyValuePair<ulong, MessageData>>> userMessageGroups = 
                from msgKv in guildData.messageData
                group msgKv by msgKv.Value.userId into msgGroup
                orderby msgGroup.Sum(mKv => mKv.Value.GetStarCount()) descending
                select msgGroup;

            return await GetLeaderboardString(userMessageGroups, guild);

        }

        private static async Task<string> GetLeaderboardString(IOrderedEnumerable<IGrouping<ulong, KeyValuePair<ulong, MessageData>>> userMessageGroups, IGuild guild)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("✨⭐🌟 🏆 Star Leaderboard 🏆 🌟⭐✨\n\n");

            int index = 1;
            int lastIndex = index;
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
