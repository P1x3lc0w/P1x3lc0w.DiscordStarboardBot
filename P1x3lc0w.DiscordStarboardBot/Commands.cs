using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private async Task HandleException(Exception e)
        {
            Console.WriteLine($"Exception while handling command: {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
            await ReplyAsync($":x: An exception occured while handling a command: `{e.GetType().FullName}`");
        }

        [Command("config channel")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task SetStarboardChannel(SocketTextChannel channel)
        {
            try
            {
                GuildData guildData = Data.BotData.guildDictionary[Context.Guild.Id];
                if (channel.IsNsfw)
                {
                    guildData.starboardChannelNSFW = channel.Id;
                    await ReplyAsync($":star: Set NSFW starboard channel to {channel.Mention}");
                }
                else
                {
                    guildData.starboardChannel = channel.Id;
                    await ReplyAsync($":star: Set starboard channel to {channel.Mention}");
                }
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
        }

        [Command("config minStars")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task SetMinStars(uint minStars)
        {
            try
            {
                GuildData guildData = Data.BotData.guildDictionary[Context.Guild.Id];
                guildData.requiredStarCount = minStars;

                await ReplyAsync($":star: Set min star count to `{minStars}`");
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
        }

        [Command("admin save")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Save()
        {
            try
            {
                await Saving.SaveDataAsync(Data.BotData);

                await ReplyAsync($":white_check_mark: Saved!");
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
        }

        [Command("admin rescanall")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Scan()
        {
            try
            {
                GuildData guildData = Data.BotData.guildDictionary[Context.Guild.Id];

                foreach(KeyValuePair<ulong, MessageData> messageDataKV in guildData.messageData)
                {
                    IUserMessage message = await messageDataKV.Value.GetMessageAsync(Context.Guild);
                    await Starboard.RescanMessage(guildData, message, await messageDataKV.Value.GetStarboardMessageAsync(message, guildData));
                }

                await ReplyAsync("✅ Rescanned all saved messages.");
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
        }

        [Command("admin scan")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Scan(SocketTextChannel channel, string messageId)
        {
            try
            {
                GuildData guildData = Data.BotData.guildDictionary[Context.Guild.Id];

                if (!UInt64.TryParse(messageId, out ulong msgId))
                {
                    await ReplyAsync(":x: Invalid message id");
                    return;
                }

                if (!(await channel.GetMessageAsync(msgId) is IUserMessage message))
                {
                    await ReplyAsync(":x: Message Not Found!");
                }
                else
                {
                    await Starboard.RescanMessage(guildData, message);
                    await ReplyAsync(":white_check_mark: Message Was Scanned!");
                }
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
        }

        [Command("admin rediscover")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Rediscover(ITextChannel channel)
        {
            try
            {
                DateTimeOffset rediscoverStart = DateTimeOffset.Now;
                GuildData guildData = Data.BotData.guildDictionary[Context.Guild.Id];
                await ReplyAsync($"▶ Starting rediscover for channel {channel.Mention}");

                IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages = channel.GetMessagesAsync(200);

                await messages.ForEachAsync(async batch =>
                {
                    try
                    {
                        int rescanCount = 0;
                        foreach (IMessage msg in batch)
                        {
                            if (msg.Timestamp > rediscoverStart)
                                continue;

                            if (msg is IUserMessage usrMsg)
                            {
                                if (!usrMsg.Author.IsBot)
                                {
                                    await Starboard.RescanMessage(guildData, usrMsg);
                                    rescanCount++;
                                }
                                else if (usrMsg.Author.Id == Context.Client.CurrentUser.Id)
                                {
                                    IEmbed embed = usrMsg.Embeds.FirstOrDefault();

                                    if (embed != null)
                                    {
                                        string[] parts = embed.Author?.Url?.Split('/');

                                        if (parts != null)
                                        {
                                            ulong channelId = UInt64.Parse(parts[5]);
                                            ulong msgId = UInt64.Parse(parts[6]);

                                            await ReplyAsync($":information_source: Found starboard message {msgId}");

                                            ITextChannel textChannel = Context.Guild.GetTextChannel(channelId);

                                            if (channel != null)
                                            {
                                                IUserMessage message = (await textChannel.GetMessageAsync(msgId)) as IUserMessage;

                                                if (message != null)
                                                {
                                                    await Starboard.RescanMessage(guildData, message, usrMsg);
                                                    await ReplyAsync($":white_check_mark: Message {message.Id} (for starboard message {usrMsg.Id}) was scanned!");
                                                    rescanCount++;
                                                }
                                                else
                                                {
                                                    await ReplyAsync($":x: Could not find message {msgId}");
                                                }
                                            }
                                            else
                                            {
                                                await ReplyAsync($":x: Could not find channel {channelId}");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        await ReplyAsync($":white_check_mark: Rescanned {rescanCount} messages.");
                    }
                    catch (Exception e)
                    {
                        await HandleException(e);
                    }
                });
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
        }

        [Command("leaderboard")]
        public async Task Leaderboard(uint page = 1)
        {
            try
            {
                await ReplyAsync(await LeaderboardUtil.GetLeaderboardStringAsync(Context.Guild, page));
            }
            catch(Exception e)

            {
                await HandleException(e);
            }
        }
    }
}