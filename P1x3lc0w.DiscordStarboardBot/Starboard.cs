using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal static class Starboard
    {
        public static IEmote StarboardEmote { get; } = new Emoji("⭐");

        public static async Task RescanMessage(StarboardContext context)
        {
            await context.GetStarredMessageAsync();

            if (context.StarredMessage.Author.Id == Program.sc.CurrentUser.Id)
            {
                if (context.GuildData.messageData.ContainsKey(context.StarredMessage.Id))
                {
                    context.GuildData.messageData.TryRemove(context.StarredMessage.Id, out _);
                }
                return;
            }

            await UpdateStarsGivenAsync(context, context.StarredMessage.GetReactionUsersAsync(StarboardEmote, 5000));
        }

        public static async Task UpdateStarsGivenAsync(StarboardContext context, IAsyncEnumerable<IReadOnlyCollection<IUser>> starGivingUsers)
        {
            await context.GetStarredMessageAsync();
            context.GetOrAddMessageData();

            context.MessageData.StarGivingUsers.Clear();

            await starGivingUsers.ForEachAsync(users =>
            {
                foreach (IUser user in users)
                {
                    if (user.Id != context.StarredMessage.Author.Id)
                    {
                        context.MessageData.StarGivingUsers.Add(user.Id);
                    }
                    else
                    {
                        context.StarredMessage.RemoveReactionAsync(StarboardEmote, user);
                    }
                }
            });

            await CreateOrUpdateStarboardMessage(context);
        }

        public static async Task UpdateStarsGivenAsync(StarboardContext context, IEnumerable<IUser> starGivingUsers)
        {
            await context.GetStarredMessageAsync();
            context.GetOrAddMessageData();

            context.MessageData.StarGivingUsers.Clear();

            foreach (IUser user in starGivingUsers)
            {
                if (user.Id != context.StarredMessage.Author.Id)
                {
                    context.MessageData.StarGivingUsers.Add(user.Id);
                }
                else
                {
                    await context.StarredMessage.RemoveReactionAsync(StarboardEmote, user);
                }
            }

            await CreateOrUpdateStarboardMessage(context);
        }

        public static async Task UpdateStarGivenAsync(StarboardContext context, IUser starGivingUser, bool starGiven)
        {
            //If the starred message was sent by the current bot user...
            if ((await context.GetStarredMessageAsync()).Author.Id == Program.sc.CurrentUser.Id)
            {
                //... check if it is a starboard message
                StarboardContext starboardContext = await StarboardUtil.GetStarboardContextFromStarboardMessage(context);

                if(starboardContext != null)
                {
                    if(context.Exception != null)
                    {
                        //We should probably delete the broken starboard message, ignore for now.
                        return;
                    }

                    await UpdateStarGivenInternalAsync(starboardContext, starGivingUser, starGiven);
                    return;
                }
            }

            await UpdateStarGivenInternalAsync(context, starGivingUser, starGiven);
        }

        private static async Task UpdateStarGivenInternalAsync(StarboardContext context, IUser starGivingUser, bool starGiven)
        {
            context.GetOrAddMessageData();

            if (starGiven)
            {
                context.MessageData.StarGivingUsers.Add(starGivingUser.Id);
            }
            else
            {
                context.MessageData.StarGivingUsers.Remove(starGivingUser.Id);
            }

            await CreateOrUpdateStarboardMessage(context);
        }

        private static async Task CreateOrUpdateStarboardMessage(StarboardContext context)
        {
            context.GetOrAddMessageData();

            if (context.MessageData.starboardMessageStatus == StarboardMessageStatus.CREATED)
            {
                while (context.MessageData.starboardMessageId == 1)
                {
                    await Task.Delay(100);
                }

                await UpdateStarboardMessage(context);
            }
            else if (context.MessageData.starboardMessageStatus == StarboardMessageStatus.NONE && context.MessageData.GetStarCount() >= context.GuildData.requiredStarCount)
            {
                EnqueueStarboardMessageCreation(context);
            }
        }

        

        public static Embed CreateStarboardEmbed(StarboardContext context)
        {
            string avatarUrl = 
                context.StarredMessage.Author.GetAvatarUrl() ??
                context.StarredMessage.Author.GetDefaultAvatarUrl();

            string jumpUrl = context.StarredMessage.GetJumpUrl();

            IEnumerator<IAttachment> enumerator = context.StarredMessage.Attachments.GetEnumerator();
            string imageUrl = null;

            if (enumerator.MoveNext())
            {
                IAttachment attachment = enumerator.Current;

                if (!attachment.IsSpoiler())
                {
                    if (attachment.Height != null)
                    {
                        imageUrl = attachment.Url;
                    }
                }
            }

            EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder()
                .WithName(context.StarredMessage.Author.Username + "#" + context.StarredMessage.Author.Discriminator)
                .WithIconUrl(avatarUrl)
                .WithUrl(jumpUrl);

            EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder()
                .WithText($"⭐{context.MessageData.GetStarCount()} • #{context.StarredMessage.Channel.Name} • 📅{context.MessageData.created:yyyy-MM-dd HH:mm:ss zzz}");

            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor(authorBuilder)
                .WithColor(Color.LightOrange)
                .WithImageUrl(imageUrl)
                .WithUrl(jumpUrl)
                .WithFooter(footerBuilder);

            if (!String.IsNullOrWhiteSpace(context.StarredMessage.Content))
            {
                embed.AddField("Content", context.StarredMessage.Content);
            }

            return embed.Build();
        }

        internal static async Task UpdateStarboardMessage(StarboardContext context)
        {
            try
            {
                if (context.MessageData.starboardMessageId != null)
                {
                    IUserMessage starboardMessage = await context.GetStarboardMessageAsync();

                    if (starboardMessage != null)
                    {
                        await starboardMessage.ModifyAsync((prop) =>
                        {
                            prop.Embed = CreateStarboardEmbed(context);
                        });
                    }
                    else
                    {
                        EnqueueStarboardMessageCreation(context);
                    }
                }
                else
                {
                    EnqueueStarboardMessageCreation(context);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while updating starboard message: {e.GetType().FullName}: {e.Message}");
            }
        }

        private static void EnqueueStarboardMessageCreation(StarboardContext context)
        {
            lock (context.MessageData)
            {
                if (context.MessageData.starboardMessageStatus != StarboardMessageStatus.CREATING && context.MessageData.GetStarCount() >= context.GuildData.requiredStarCount)
                {
                    context.MessageData.starboardMessageStatus = StarboardMessageStatus.CREATING;

                    _ = CreateStarboardMessage(context);
                }
            }
        }

        internal static async Task<ulong?> CreateStarboardMessage(StarboardContext context)
        {
            ulong? createdStarboardMsgId;

            try
            {
                bool isNsfw = context.StarredMessageTextChannel.IsNsfw;

                IGuild guild = context.StarredMessageTextChannel.Guild;
                ITextChannel starboardChannel = isNsfw ? 
                    await guild.GetTextChannelAsync(context.GuildData.starboardChannelNSFW) : 
                    await guild.GetTextChannelAsync(context.GuildData.starboardChannel);

                IUserMessage starboardMsg = await starboardChannel.SendMessageAsync(embed: CreateStarboardEmbed(context));
                createdStarboardMsgId = starboardMsg.Id;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while creating starboard message: {e.GetType().FullName}: {e.Message}");
                createdStarboardMsgId = null;
            }

            context.MessageData.starboardMessageId = createdStarboardMsgId;

            if (createdStarboardMsgId != null)
            {
                context.MessageData.starboardMessageStatus = StarboardMessageStatus.CREATED;
            }

            return createdStarboardMsgId;
        }
    }
}