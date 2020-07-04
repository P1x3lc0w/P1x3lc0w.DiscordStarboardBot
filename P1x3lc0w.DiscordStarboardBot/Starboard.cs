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
        public static async Task RescanMessage(GuildData guildData, IUserMessage message, IUserMessage starboardMessage = null)
        {
            if (message.Author.Id == Program.sc.CurrentUser.Id)
            {
                if (guildData.messageData.ContainsKey(message.Id))
                {
                    guildData.messageData.TryRemove(message.Id, out _);
                }
                return;
            }

            await UpdateStarsGivenAsync(message, message.GetReactionUsersAsync(StarboardEmote, 5000), starboardMessage);
        }

        public static async Task UpdateStarsGivenAsync(IUserMessage starredMessage, IAsyncEnumerable<IReadOnlyCollection<IUser>> starGivingUsers, IUserMessage starboardMessage = null)
        {
            GuildData guildData = Data.BotData.guildDictionary[(starredMessage.Channel as ITextChannel).Guild.Id];
            MessageData messageData = GetOrAddMessageData(guildData, starredMessage, starboardMessage);

            messageData.StarGivingUsers.Clear();

            await starGivingUsers.ForEachAsync(users =>
            {
                foreach (IUser user in users)
                {
                    if (user.Id != starredMessage.Author.Id)
                    {
                        messageData.StarGivingUsers.Add(user.Id);
                    }
                    else
                    {
                        starredMessage.RemoveReactionAsync(StarboardEmote, user);
                    }
                }
            });
            
            await CreateOrUpdateStarboardMessage(guildData, starredMessage, messageData);
        }

        public static async Task UpdateStarsGivenAsync(IUserMessage starredMessage, IEnumerable<IUser> starGivingUsers, IUserMessage starboardMessage = null)
        {
            GuildData guildData = Data.BotData.guildDictionary[(starredMessage.Channel as ITextChannel).Guild.Id];
            MessageData messageData = GetOrAddMessageData(guildData, starredMessage, starboardMessage);

            messageData.StarGivingUsers.Clear();

            foreach (IUser user in starGivingUsers)
            {
                if (user.Id != starredMessage.Author.Id)
                {
                    messageData.StarGivingUsers.Add(user.Id);
                }
                else
                {
                    await starredMessage.RemoveReactionAsync(StarboardEmote, user);
                }
            }

            await CreateOrUpdateStarboardMessage(guildData, starredMessage, messageData);
        }

        public static async Task UpdateStarGivenAsync(IUserMessage starredMessage, IUser starGivingUser, bool starGiven, IUserMessage starboardMessage = null)
        {
            GuildData guildData = Data.BotData.guildDictionary[(starredMessage.Channel as ITextChannel).Guild.Id];
            MessageData messageData = GetOrAddMessageData(guildData, starredMessage, starboardMessage);

            if (starGiven)
            {
                messageData.StarGivingUsers.Add(starGivingUser.Id);
            }
            else
            {
                messageData.StarGivingUsers.Remove(starGivingUser.Id);
            }

            await CreateOrUpdateStarboardMessage(guildData, starredMessage, messageData);
        }

        private static async Task CreateOrUpdateStarboardMessage(GuildData guildData, IUserMessage msg, MessageData messageData)
        {
            if (messageData.starboardMessageStatus == StarboardMessageStatus.CREATED)
            {
                while (messageData.starboardMessageId == 1)
                {
                    await Task.Delay(100);
                }

                await UpdateStarboardMessage(guildData, msg, messageData);
            }
            else if (messageData.starboardMessageStatus == StarboardMessageStatus.NONE && messageData.GetStarCount() >= guildData.requiredStarCount)
            {
                EnqueueStarboardMessageCreation(guildData, msg, messageData);
            }
        }

        public static MessageData GetOrAddMessageData(GuildData guildData, IUserMessage starredMessage, IUserMessage starboardMessage = null)
            => guildData.messageData.GetOrAdd(
                    starredMessage.Id,
                    new MessageData()
                    {
                        created = starredMessage.CreatedAt,
                        userId = starredMessage.Author.Id,
                        isNsfw = (starredMessage.Channel as ITextChannel).IsNsfw,
                        channelId = starredMessage.Channel.Id,
                        starboardMessageId = starboardMessage?.Id
                    }
                );

        public static Embed CreateStarboardEmbed(IUserMessage msg, MessageData messageData)
        {
            string avatarUrl = msg.Author.GetAvatarUrl() ?? msg.Author.GetDefaultAvatarUrl();

            string jumpUrl = msg.GetJumpUrl();

            IEnumerator<IAttachment> enumerator = msg.Attachments.GetEnumerator();
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
                .WithName(msg.Author.Username + "#" + msg.Author.Discriminator)
                .WithIconUrl(avatarUrl)
                .WithUrl(jumpUrl);

            EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder()
                .WithText($"⭐{messageData.GetStarCount()} • {messageData.created:yyyy-MM-dd HH:mm:ss zzz}");

            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor(authorBuilder)
                .WithColor(Color.LightOrange)
                .WithImageUrl(imageUrl)
                .WithUrl(jumpUrl)
                .WithFooter(footerBuilder);

            if (!string.IsNullOrWhiteSpace(msg.Content))
            {
                embed.AddField("Content", msg.Content);
            }

            return embed.Build();
        }

        internal static async Task UpdateStarboardMessage(GuildData guildData, IUserMessage msg, MessageData data)
        {
            try
            {
                if (data.starboardMessageId != null)
                {
                    bool isNsfw = (msg.Channel as ITextChannel).IsNsfw;

                    IGuild guild = (msg.Channel as ITextChannel).Guild;
                    ITextChannel starboardChannel = isNsfw ? await guild.GetTextChannelAsync(guildData.starboardChannelNSFW) : await guild.GetTextChannelAsync(guildData.starboardChannel);

                    IUserMessage startboardMsg = await starboardChannel.GetMessageAsync(data.starboardMessageId.Value) as IUserMessage;

                    await startboardMsg.ModifyAsync((prop) =>
                    {
                        prop.Embed = CreateStarboardEmbed(msg, data);
                    });
                }
                else
                {
                    EnqueueStarboardMessageCreation(guildData, msg, data);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while updating starboard message: {e.GetType().FullName}: {e.Message}");
            }
        }

        private static void EnqueueStarboardMessageCreation(GuildData guildData, IUserMessage msg, MessageData messageData)
        {
            lock (messageData)
            {
                if (messageData.starboardMessageStatus != StarboardMessageStatus.CREATING && messageData.GetStarCount() >= guildData.requiredStarCount)
                {
                    messageData.starboardMessageStatus = StarboardMessageStatus.CREATING;

                    _ = CreateStarboardMessage(guildData, msg, messageData);
                }
            }
        }

        internal static async Task<ulong?> CreateStarboardMessage(GuildData guildData, IUserMessage msg, MessageData data)
        {
            ulong? createdStarboardMsgId = null;

            try
            {
                bool isNsfw = (msg.Channel as ITextChannel).IsNsfw;

                IGuild guild = (msg.Channel as ITextChannel).Guild;
                ITextChannel starboardChannel = isNsfw ? await guild.GetTextChannelAsync(guildData.starboardChannelNSFW) : await guild.GetTextChannelAsync(guildData.starboardChannel);

                IUserMessage starboardMsg = await starboardChannel.SendMessageAsync(embed: CreateStarboardEmbed(msg, data));
                createdStarboardMsgId = starboardMsg.Id;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while creating starboard message: {e.GetType().FullName}: {e.Message}");
                createdStarboardMsgId = null;
            }

            data.starboardMessageId = createdStarboardMsgId;

            if (createdStarboardMsgId != null)
            {
                data.starboardMessageStatus = StarboardMessageStatus.CREATED;
            }

            return createdStarboardMsgId;
        }
    }
}