using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    public class Program
    {
        public static DiscordSocketClient sc;
        public static CommandHandler handler;
        static void Main(string[] args)
        {
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

            ServiceCollection services = new ServiceCollection();

            services.AddSingleton(sc)
            .AddSingleton<CommandHandler>()
            .AddSingleton(new CommandService(new CommandServiceConfig
            {                                       // Add the command service to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
            }));

            var provider = services.BuildServiceProvider();
            _ = provider.GetRequiredService<CommandHandler>().InstallCommandsAsync();


            string token = "";

            {
#if DEBUG
                Console.WriteLine("DEBUG");
                byte[] token_enc = Convert.FromBase64String(@"wBVgBhGVlpmboAWxh6WODPFiSDyOLxPskAHmb8Ruk40CjeOBsifAu5JLqNIZUJ3g0q8qiYzyCQmqsAY58lp1Yg==");
                byte[] iv = Convert.FromBase64String(@"fClcUe7OWzqU8dIOt1u60g==");
#else
                Console.WriteLine("RELEASE");
                byte[] token_enc = Convert.FromBase64String(@"CBFtbeHilk2H1EDrcMst17Fu/07yIZXrmwv2w6Lowu1X9u0pz3UrtbSOQIkFv3eJ+vlOAIb4cug6VoljLvYFlQ==");
                byte[] iv = Convert.FromBase64String(@"w8Ky3A53CBv/wFLkATG4Lw==");
#endif
                byte[] aes_key = Convert.FromBase64String(File.ReadAllText(".key"));

                token = Crypto.DecryptStringFromBytes_Aes(token_enc, aes_key, iv);
            }


            sc.LoginAsync(Discord.TokenType.Bot, token);


            Thread.Sleep(Timeout.Infinite);
        }


        internal static void Exit()
        {
            Console.WriteLine("Shutting Down....");
            Task.Run(ExitAsync);
        }
        private static async Task ExitAsync()
        {
            await sc.LogoutAsync();
            await sc.StopAsync();

            Console.WriteLine("Goodbye!");
            Environment.Exit(0);
        }
    }
}
