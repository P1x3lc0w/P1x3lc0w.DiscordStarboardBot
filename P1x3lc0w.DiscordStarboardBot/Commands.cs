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
        [Command("config channel")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task SetStarboardChannel(SocketTextChannel channel)
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

        [Command("config minStars")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task SetMinStars(uint minStars)
        {
            GuildData guildData = Data.BotData.guildDictionary[Context.Guild.Id];
            guildData.requiredStarCount = minStars;

            await ReplyAsync($":star: Set min star count to `{minStars}`");
        }

        [Command("admin save")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Save()
        {
            await Saving.SaveDataAsync(Data.BotData);

            await ReplyAsync($":white_check_mark: Saved!");
        }

        [Command("admin scan")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Scan(SocketTextChannel channel, string messageId)
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
                uint currentStarCount = 0;
                if (guildData.messageData.ContainsKey(message.Id))
                {
                    currentStarCount = guildData.messageData[message.Id].stars;
                }

                foreach (KeyValuePair<IEmote, ReactionMetadata> keyValue in message.Reactions)
                {
                    if (keyValue.Key.Name.Equals("⭐", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int delta = keyValue.Value.ReactionCount - (int)currentStarCount;
                        if (await message.GetReactionUsersAsync(new Emoji("⭐"), Math.Min((int)guildData.requiredStarCount * 10, 100)).Any(users => users.Any(user => user.Id == message.Author.Id)))
                        {
                            await message.RemoveReactionAsync(new Emoji("⭐"), message.Author).ConfigureAwait(false);
                            delta -= 1;
                        }

                        Starboard.UpdateStarCount(message, delta);
                    }
                }
                await ReplyAsync(":white_check_mark: Message Was Scanned!");
            }
        }

        [Command("leaderboard")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Leaderboard()
        {
            await ReplyAsync(await LeaderboardUtil.GetLeaderboardStringAsync(Context.Guild));
        }
    }
}