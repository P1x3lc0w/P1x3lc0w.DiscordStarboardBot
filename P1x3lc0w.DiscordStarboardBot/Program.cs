using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using P1x3lc0w.Common;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    public class Program
    {
        public static DiscordSocketClient sc;
        private static BotConfig botConfig;
        public static CommandHandler handler;

        private static void Main(string[] args)
        {
            Task.Run(Data.LoadDataAsync);

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            sc = new DiscordSocketClient();

            sc.Ready += Events.Sc_Ready;
            sc.LoggedIn += Events.Sc_LoggedIn;
            sc.Log += Events.Sc_Log;
            sc.MessageReceived += Events.Sc_MessageReceived;
            sc.ReactionAdded += Events.Sc_ReactionAdded;
            sc.GuildAvailable += Events.Sc_GuildAvailable;
            sc.ReactionRemoved += Events.Sc_ReactionRemoved;
            sc.GuildMemberUpdated += Events.Sc_GuildMemberUpdated;

            ServiceCollection services = new ServiceCollection();

            services.AddSingleton(sc)
            .AddSingleton<CommandHandler>()
            .AddSingleton(new CommandService(new CommandServiceConfig
            {                                       // Add the command service to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
            }));

            ServiceProvider provider = services.BuildServiceProvider();
            _ = provider.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            botConfig = BotConfig.ReadConfigFromFile("botconfig.json");

            sc.LoginAsync(Discord.TokenType.Bot, botConfig.token);

            Thread.Sleep(Timeout.Infinite);
        }

        internal static void Exit()
        {
            Log("Shutting Down....");
            Task.Run(ExitAsync);
        }

        private static async Task ExitAsync()
        {
            await sc.LogoutAsync();
            await sc.StopAsync();

            Log("Goodbye!");
            Environment.Exit(0);
        }

        private static object _logLock = new object();

        public static void Log(string message, LogSeverity severity = LogSeverity.Info)
        {
            lock (_logLock)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write($"[{DateTimeOffset.Now}]");

                switch (severity)
                {
                    case Discord.LogSeverity.Debug:
                    case Discord.LogSeverity.Verbose:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;


                    case Discord.LogSeverity.Info:
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        break;

                    case Discord.LogSeverity.Warning:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;

                    case Discord.LogSeverity.Error:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;

                    case Discord.LogSeverity.Critical:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                Console.Write($"[{severity}]");

                switch (severity)
                {
                    case Discord.LogSeverity.Debug:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;

                    case Discord.LogSeverity.Verbose:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;


                    case Discord.LogSeverity.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case Discord.LogSeverity.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case Discord.LogSeverity.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case Discord.LogSeverity.Critical:
                        Console.BackgroundColor = ConsoleColor.Red;
                        break;
                }

                Console.Write(' ');

                Console.WriteLine(message);
            }
        }
    }
}