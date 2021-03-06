﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal class GuildData
    {
        public ConcurrentDictionary<ulong, MessageData> messageData;
        public ulong starboardChannel;
        public ulong starboardChannelNSFW;
        public uint requiredStarCount;

        public GuildData()
        {
            messageData = new ConcurrentDictionary<ulong, MessageData>();
            requiredStarCount = 3;
        }

        public void UpdateMessagesByUser(SocketGuild guild, ulong userId)
        {
            Parallel.ForEach<KeyValuePair<ulong, MessageData>>(messageData, async msgData =>
            {
                try
                {
                    if (msgData.Value.userId == userId)
                    {
                        await Starboard.UpdateStarboardMessage(new StarboardContext(StarboardContextType.USER_UPDATED, this, msgData.Value));
                    }
                }
                catch (Exception e)
                {
                    Program.Log($"Exception while updating starboard message {msgData.Value?.starboardMessageId}: {e.GetType().FullName}: {e.Message}\n{e.StackTrace}", LogSeverity.Error);
                }
            });
        }
    }
}