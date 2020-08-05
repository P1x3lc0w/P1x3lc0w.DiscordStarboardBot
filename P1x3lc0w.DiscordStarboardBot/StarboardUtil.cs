using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal static class StarboardUtil
    {
        internal static MessageData GetStarboardMessageData(GuildData guildData, IUserMessage starredMessage)
        {
            IEmbed embed = starredMessage.Embeds.FirstOrDefault();

            if (embed != null)
            {
                string[] parts = embed.Author?.Url?.Split('/');

                if (parts != null && parts.Length > 6)
                {
                    ulong msgId = UInt64.Parse(parts[6]);

                    if (guildData.messageData.ContainsKey(msgId))
                    {
                        return guildData.messageData[msgId];
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Message data missing for message {msgId}");
                    }
                }
            }

            return null;
        }
    }
}
