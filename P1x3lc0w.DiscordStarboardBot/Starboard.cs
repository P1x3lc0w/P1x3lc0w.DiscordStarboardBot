using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal static class Starboard
    {
        public static void UpdateStarCount(IUserMessage msg, int delta)
        {
            GuildData guildData = Data.BotData.guildDictionary[(msg.Channel as ITextChannel).Guild.Id];

            MessageData messageData = null;

            lock (guildData)
            {
                if (!guildData.messageData.ContainsKey(msg.Id))
                {
                    messageData = new MessageData()
                    {
                        created = msg.CreatedAt,
                        userId = msg.Author.Id,
                        isNsfw = (msg.Channel as ITextChannel).IsNsfw,
                        channelId = msg.Channel.Id
                    };
                    guildData.messageData.Add(msg.Id, messageData);
                }
                else
                {
                    messageData = guildData.messageData[msg.Id];
                }
            }

            lock (messageData)
            {
                int newStarCount = (int)messageData.stars + delta;

                if (newStarCount < 0)
                    return;

                messageData.stars = (uint)newStarCount;

                if (messageData.starboardMessageId != 0)
                {
                    Task.Run(async () =>
                    {
                        while (messageData.starboardMessageId == 1)
                        {
                            await Task.Delay(100);
                        }

                        await UpdateStarboardMessage(guildData, msg, messageData);
                    });
                }
                else
                {
                    if (messageData.stars >= guildData.requiredStarCount)
                    {
                        //Save that we are currently createing a message
                        messageData.starboardMessageId = 1;

                        Task.Run(async () => messageData.starboardMessageId = await CreateStarboardMessage(guildData, msg, messageData));
                    }
                }
            }
        }

        public static Embed CreateEmbed(IUserMessage msg, MessageData data)
        {
            string avatarUrl = msg.Author.GetAvatarUrl() ?? msg.Author.GetDefaultAvatarUrl();

            string jumpUrl = msg.GetJumpUrl();

            IEnumerator<IAttachment> enumerator = msg.Attachments.GetEnumerator();

            IAttachment attachment = null;
            string imageUrl = null;

            if (enumerator.MoveNext())
            {
                attachment = enumerator.Current;

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
                .WithText($"⭐{data.stars} • {data.created.ToString("yyyy-MM-dd HH:mm:ss zzz")}");

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
                bool isNsfw = (msg.Channel as ITextChannel).IsNsfw;

                IGuild guild = (msg.Channel as ITextChannel).Guild;
                ITextChannel starboardChannel = isNsfw ? await guild.GetTextChannelAsync(guildData.starboardChannelNSFW) : await guild.GetTextChannelAsync(guildData.starboardChannel);

                IUserMessage startboardMsg = await starboardChannel.GetMessageAsync(data.starboardMessageId) as IUserMessage;//SendMessageAsync(embed: CreateEmbed(msg, data));

                await startboardMsg.ModifyAsync((prop) =>
                {
                    prop.Embed = CreateEmbed(msg, data);
                });
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while updating starboard message: {e.GetType().FullName}: {e.Message}");
            }
        }

        internal static async Task<ulong> CreateStarboardMessage(GuildData guildData, IUserMessage msg, MessageData data)
        {
            try
            {
                bool isNsfw = (msg.Channel as ITextChannel).IsNsfw;

                IGuild guild = (msg.Channel as ITextChannel).Guild;
                ITextChannel starboardChannel = isNsfw ? await guild.GetTextChannelAsync(guildData.starboardChannelNSFW) : await guild.GetTextChannelAsync(guildData.starboardChannel);

                IUserMessage startboardMsg = await starboardChannel.SendMessageAsync(embed: CreateEmbed(msg, data));
                return startboardMsg.Id;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while creating starboard message: {e.GetType().FullName}: {e.Message}");
                return 0;
            }
        }
    }
}