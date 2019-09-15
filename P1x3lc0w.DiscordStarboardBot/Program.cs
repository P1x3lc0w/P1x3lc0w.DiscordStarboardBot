using Discord.WebSocket;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    class Program
    {
        static DiscordShardedClient sc;

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            sc = new DiscordShardedClient();

            sc.ShardReady += Sc_ShardReady;
            sc.LoggedIn += Sc_LoggedIn;
            sc.Log += Sc_Log;
            sc.MessageReceived += Sc_MessageReceived;

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

        private static Task Sc_MessageReceived(SocketMessage arg)
        {

            return Task.CompletedTask;
        }

        private static Task Sc_ShardReady(DiscordSocketClient arg)
        {
            return Task.CompletedTask;
        }

        private static Task Sc_Log(Discord.LogMessage arg)
        {
            switch (arg.Severity)
            {
                case Discord.LogSeverity.Debug:
                case Discord.LogSeverity.Verbose:
                case Discord.LogSeverity.Info:
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

        private static Task Sc_LoggedIn()
        {
            sc.StartAsync();
            return Task.CompletedTask;
        }
    }
}
