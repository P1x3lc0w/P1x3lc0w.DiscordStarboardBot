using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("channel")]
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


        [Command("minStars")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task SetMinStars(uint minStars)
        {
            GuildData guildData = Data.BotData.guildDictionary[Context.Guild.Id];
            guildData.requiredStarCount = minStars;

            await ReplyAsync($":star: Set min star count to `{minStars}`");
        }

        [Command("save")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Save()
        {
            await File.WriteAllTextAsync("savedata.json", JsonConvert.SerializeObject(Data.BotData));

            await ReplyAsync($":white_check_mark: Saved!");
        }
    }
}
