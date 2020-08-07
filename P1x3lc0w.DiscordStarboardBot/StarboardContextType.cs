using System;
using System.Collections.Generic;
using System.Text;

namespace P1x3lc0w.DiscordStarboardBot
{
    public enum StarboardContextType
    {
        REACTION_ADDED,
        REACTION_REMOVED,
        COMMAND_RESCANALL,
        COMMAND_SCAN,
        COMMANMD_REDISCOVER,
        USER_UPDATED
    }
}
