using Discord;
using System;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal class StarboardContext
    {
        private IGuild _guild;
        private ITextChannel _starredMessageTextChannel;
        private ITextChannel _starboardTextChannel;
        private IUserMessage _starredMessage;
        private IUserMessage _starboardMessage;

        private GuildData _guildData;
        private MessageData _messageData;

        public StarboardContext(StarboardContextType contextType, IUserMessage starredMessage)
        {
            ContextType = contextType;
            this._starredMessage = starredMessage;
        }

        public StarboardContext(StarboardContextType contextType, GuildData guildData, MessageData messageData)
        {
            ContextType = contextType;
            this._guildData = guildData;
            this._messageData = messageData;
        }

        public StarboardContext(StarboardContextType contextType, GuildData guildData, IUserMessage starredMessage, ITextChannel starredMessageTextChannel, IUserMessage starboardMessage, ITextChannel starboardTextChannel)
        {
            ContextType = contextType;
            this._guildData = guildData;
            this._starredMessage = starredMessage;
            this._starredMessageTextChannel = starredMessageTextChannel;
            this._starboardMessage = starboardMessage;
            this._starboardTextChannel = starboardTextChannel;
        }

        public StarboardContext(StarboardContextType contextType, GuildData guildData, MessageData messageData, IUserMessage starredMessage)
        {
            ContextType = contextType;
            this._guildData = guildData;
            this._messageData = messageData;
            this._starredMessage = starredMessage;
        }

        public StarboardContext(StarboardContextType contextType, GuildData guildData, IUserMessage starredMessage, ITextChannel starredMessageTextChannel)
        {
            ContextType = contextType;
            this._guildData = guildData;
            this._starredMessage = starredMessage;
            this._starredMessageTextChannel = starredMessageTextChannel;
        }

        public StarboardContext(StarboardContextType contextType, GuildData guildData, IUserMessage starredMessage)
        {
            ContextType = contextType;
            this._guildData = guildData;
            this._starredMessage = starredMessage;
        }

        public StarboardContextType ContextType { get; private set; }
        public StarboardSource Source { get; set; }

        public IGuild Guild
        {
            get
            {
                if (_guild != null)
                {
                    return _guild;
                }

                if (StarredMessageTextChannel != null)
                {
                    _guild = _starredMessageTextChannel.Guild;
                    return _guild;
                }

                if (StarboardTextChannel != null)
                {
                    _guild = _starboardTextChannel.Guild;
                    return _guild;
                }

                return null;
            }
        }

        public ITextChannel StarredMessageTextChannel
        {
            get
            {
                if (_starredMessageTextChannel != null)
                {
                    return _starredMessageTextChannel;
                }

                if (_starredMessage != null)
                {
                    _starredMessageTextChannel = _starredMessage.Channel as ITextChannel;
                    return _starredMessageTextChannel;
                }

                return null;
            }

            set => _starredMessageTextChannel = value;
        }

        public ITextChannel StarboardTextChannel
        {
            get
            {
                if (_starboardTextChannel != null)
                {
                    return _starboardTextChannel;
                }

                if (_starboardMessage != null)
                {
                    _starboardTextChannel = _starboardMessage.Channel as ITextChannel;
                    return _starboardTextChannel;
                }

                return null;
            }

            set => _starboardTextChannel = value;
        }

        public IUserMessage StarredMessage
        {
            get => _starredMessage;
            set => _starredMessage = value;
        }

        public IUserMessage StarboardMessage
        {
            get => _starboardMessage;
            set => _starboardMessage = value;
        }

        public GuildData GuildData
        {
            get
            {
                if (_guildData != null)
                {
                    return _guildData;
                }

                if (Guild != null)
                {
                    _guildData = Data.BotData.guildDictionary[Guild.Id];
                    return _guildData;
                }

                return null;
            }
        }

        public MessageData MessageData
        {
            get
            {
                if (_messageData != null)
                {
                    return _messageData;
                }

                if (GuildData != null && _starredMessage != null && GuildData.messageData.ContainsKey(_starredMessage.Id))
                {
                    _messageData = GuildData.messageData[_starredMessage.Id];
                    return _messageData;
                }

                return null;
            }
            set => _messageData = value;
        }

        public Exception Exception { get; set; }

        public async Task<IUserMessage> GetStarredMessageAsync()
        {
            if (_starredMessage != null)
            {
                return _starredMessage;
            }

            if (Guild != null && _messageData != null)
            {
                _starredMessage = await _messageData.GetMessageAsync(Guild);
                return _starredMessage;
            }

            return null;
        }

        public async Task RemoveReactionAsync(IUser user, StarboardSource source = StarboardSource.UNKNOWN)
        {
            if (source == StarboardSource.UNKNOWN)
                source = Source;

            switch(source)
            {
                case StarboardSource.STARBOARD_MESSAGE:
                    await (await GetStarboardMessageAsync()).RemoveReactionAsync(Starboard.StarboardEmote, user);
                    break;

                case StarboardSource.STARRED_MESSAGE:
                    await (await GetStarredMessageAsync()).RemoveReactionAsync(Starboard.StarboardEmote, user);
                    break;

                default:
                    throw new NotSupportedException();
            };
        }

        public async Task RemoveReactionFromStarboardMessageAsync(IUser user)
            => await (await GetStarboardMessageAsync()).RemoveReactionAsync(Starboard.StarboardEmote, user);

        public void ResetStarredMessage()
        {
            _starredMessage = null;
        }

        public void ResetStarboardMessage()
        {
            _starboardMessage = null;
        }

        public async Task<IUserMessage> GetStarboardMessageAsync()
        {
            if (_starboardMessage != null)
            {
                return _starboardMessage;
            }

            if (Guild != null)
            {
                _starboardMessage = await _messageData.GetStarboardMessageAsync(await GetStarredMessageAsync(), GuildData);
                return _starboardMessage;
            }

            return null;
        }

        public MessageData GetOrAddMessageData()
        {
            _messageData = GuildData.messageData.GetOrAdd(
                    _starredMessage.Id,
                    new MessageData(_starredMessage.Id)
                    {
                        created = _starredMessage.CreatedAt,
                        userId = _starredMessage.Author.Id,
                        isNsfw = StarredMessageTextChannel.IsNsfw,
                        channelId = StarredMessageTextChannel.Id,
                        starboardMessageId = _starboardMessage?.Id,
                        starboardMessageStatus = _starboardMessage?.Id != null ? StarboardMessageStatus.CREATED : StarboardMessageStatus.NONE
                    }
                );

            return _messageData;
        }
    }
}