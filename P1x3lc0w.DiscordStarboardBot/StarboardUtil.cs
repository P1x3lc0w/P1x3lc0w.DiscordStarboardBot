using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal static class StarboardUtil
    {
        internal static async Task<StarboardContext> GetStarboardContextFromStarboardMessage(StarboardContext context)
        {
            IEmbed embed = (await context.GetStarredMessageAsync()).Embeds.FirstOrDefault();

            if (embed != null)
            {
                string[] parts = embed.Author?.Url?.Split('/');

                if (parts != null && parts.Length > 6)
                {
                    ulong channelId = UInt64.Parse(parts[5]);
                    ulong messageId = UInt64.Parse(parts[6]);

                    context.StarredMessageTextChannel = (await context.Guild.GetChannelAsync(channelId)) as ITextChannel;

                    if(context.StarredMessageTextChannel != null)
                    {
                        context.StarredMessage = await context.StarredMessageTextChannel.GetMessageAsync(messageId) as IUserMessage;

                        if (context.StarredMessage == null)
                        {
                            context.Exception = new NullReferenceException("StarredMessage was null (deleted or not found).");
                        }
                    }
                    else
                    {
                        context.Exception = new NullReferenceException("StarredMessageTextChannel was null (deleted or not found).");
                    }

                    return context;
                }
            }

            return null;
        }
    }
}