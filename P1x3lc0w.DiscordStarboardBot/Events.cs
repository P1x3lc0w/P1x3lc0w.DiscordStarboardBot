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
            Program.Log(arg.Message, arg.Severity);

            return Task.CompletedTask;
        }

        internal static Task Sc_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            try
            {
                if (arg1.Username != arg2.Username ||
                    arg1.Discriminator != arg2.Discriminator ||
                    arg1.AvatarId != arg2.AvatarId)
                {
                    Data.BotData.guildDictionary[arg2.Guild.Id].UpdateMessagesByUser(arg2.Guild, arg2.Id);
                }

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Program.Log($"Exception in Sc_GuildMemberUpdated: {e.GetType().FullName}: {e.Message}\n{e.StackTrace}", LogSeverity.Error);
                return Task.CompletedTask;
            }
        }

        internal static Task Sc_Ready() => Task.CompletedTask;

        internal static async Task Sc_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            try
            {
                IUserMessage msg = arg1.Value ?? await arg1.DownloadAsync();

                if (arg3.Emote.Name.Equals("⭐", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (msg.Author.Id != arg3.User.Value.Id)
                    {
                        await Starboard.UpdateStarGivenAsync(new StarboardContext(StarboardContextType.REACTION_REMOVED, msg), arg3.User.Value, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Log($"Exception in Sc_ReactionRemoved {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}", LogSeverity.Error);
            }
        }

        internal static async Task Sc_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            try
            {
                IUserMessage msg = arg1.Value ?? await arg1.DownloadAsync();

                if (arg3.Emote.Name.Equals("⭐", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (msg.Author.Id == arg3.User.Value.Id)
                    {
                        await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                    }
                    else
                    {
                        await Starboard.UpdateStarGivenAsync(new StarboardContext(StarboardContextType.REACTION_ADDED, msg), arg3.User.Value, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Log($"Exception in Sc_ReactionAdded {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}", LogSeverity.Error);
            }
        }

        internal static Task Sc_GuildAvailable(SocketGuild arg)
        {
            try
            {
                Program.Log($"Guild Available: {arg.Name} ({arg.Id})");

                if (!Data.BotData.guildDictionary.ContainsKey(arg.Id))
                {
                    Program.Log($"Createing Guild Data for: {arg.Name} ({arg.Id})");

                    Data.BotData.guildDictionary.TryAdd(arg.Id, new GuildData());
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Program.Log($"Exception in Sc_GuildAvailable {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}", LogSeverity.Error);
                throw;
            }
        }

        internal static Task Sc_LoggedIn()
        {
            try
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
            catch (Exception ex)
            {
                Program.Log($"Exception in Sc_LoggedIn {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}", LogSeverity.Error);
                throw;
            }
        }
    }
}