using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace P1x3lc0w.DiscordStarboardBot
{
    class BotConfig
    {
        public readonly string token;

        public BotConfig(string token)
        {
            this.token = token;
        }

        public static BotConfig ReadConfigFromFile(string filePath)
            => JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(filePath)); 
    }
}
