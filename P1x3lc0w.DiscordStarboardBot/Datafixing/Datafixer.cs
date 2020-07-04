using System;
using System.Collections.Generic;
using System.Text;

namespace P1x3lc0w.DiscordStarboardBot.Datafixing
{
    static class Datafixer
    {
        public static void FixData(Data dataToFix)
        {
            foreach(KeyValuePair<ulong, GuildData> guildDataKV in dataToFix.guildDictionary)
            {
                foreach(KeyValuePair<ulong, MessageData> messageDataKV in guildDataKV.Value.messageData)
                {
                    if(messageDataKV.Value.starboardMessageId < 10 || messageDataKV.Value.starboardMessageStatus == StarboardMessageStatus.CREATING)
                    {
                        messageDataKV.Value.starboardMessageStatus = StarboardMessageStatus.NONE;
                        messageDataKV.Value.starboardMessageId = null;
                    }
                    else if(messageDataKV.Value.starboardMessageId > 10 && messageDataKV.Value.starboardMessageStatus == StarboardMessageStatus.NONE)
                    {
                        messageDataKV.Value.starboardMessageStatus = StarboardMessageStatus.CREATED;
                    }

                    messageDataKV.Value.messageId = messageDataKV.Key;

                    if(messageDataKV.Value.userId == 622703582801559563)
                    {
                        guildDataKV.Value.messageData.Remove(messageDataKV.Key, out _);
                    }
                }
            }
        }
    }
}
