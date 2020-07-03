using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal class Events
    {
        internal static Task Sc_MessageReceived(SocketMessage arg)
        {
            return Task.CompletedTask;
        }

        internal static Task Sc_Log(Discord.LogMessage arg)
        {
            switch (arg.Severity)
            {
                case Discord.LogSeverity.Debug:
                case Discord.LogSeverity.Verbose:
                case Discord.LogSeverity.Info:
                case Discord.LogSeverity.Warning:
                    Console.WriteLine("[" + arg.Severity.ToString() + "]" + arg.Message);
                    break;

                case Discord.LogSeverity.Error:
                    Console.Error.WriteLine("[ERROR] " + arg.Message);
                    break;

                case Discord.LogSeverity.Critical:
                    Console.Error.WriteLine("[CRITICAL] " + arg.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        internal static Task Sc_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            try
            {
                Data.BotData.guildDictionary[arg2.Guild.Id].UpdateMessagesByUser(arg2.Guild, arg2.Id);
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while updating starboard message: {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
                return Task.CompletedTask;
            }

        }

        internal static Task Sc_Ready() => Task.CompletedTask;

        internal static async Task Sc_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            IUserMessage msg = arg1.Value ?? await arg1.DownloadAsync();

            if (arg3.Emote.Name.Equals("⭐", StringComparison.InvariantCultureIgnoreCase))
            {
                if (msg.Author.Id != arg3.User.Value.Id)
                {
                    await Starboard.UpdateStarCount(msg, -1);
                }
            }
        }

        internal static async Task Sc_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            IUserMessage msg = arg1.Value ?? await arg1.DownloadAsync();

            if(msg.Author.Id == Program.sc.CurrentUser.Id)
            {
                return;
            }

            if (arg3.Emote.Name.Equals("⭐", StringComparison.InvariantCultureIgnoreCase))
            {
                if (msg.Author.Id == arg3.User.Value.Id)
                {
                    await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }
                else
                {
                    await Starboard.UpdateStarCount(msg, 1);
                }
            }
        }

        internal static Task Sc_GuildAvailable(SocketGuild arg)
        {
            Console.WriteLine($"Guild Available: {arg.Name} ({arg.Id})");

            if (!Data.BotData.guildDictionary.ContainsKey(arg.Id))
            {
                Console.WriteLine($"Createing Guild Data for: {arg.Name} ({arg.Id})");

                Data.BotData.guildDictionary.TryAdd(arg.Id, new GuildData());
            }

            return Task.CompletedTask;
        }

        internal static Task Sc_LoggedIn()
        {
            Program.sc.StartAsync();

            Task.Run(async () =>
            {
                while (true)
                {
                    await Saving.SaveDataAsync(Data.BotData);
                    await Task.Delay(new TimeSpan(0, 2, 0, 0, 0));
                }
            });

            return Task.CompletedTask;
        }
    }
}