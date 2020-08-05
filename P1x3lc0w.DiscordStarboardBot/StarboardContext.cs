using Discord;
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

        public StarboardContext(IUserMessage starredMessage)
        {
            this._starredMessage = starredMessage;
        }

        public StarboardContext(GuildData guildData, MessageData messageData)
        {
            this._guildData = guildData;
            this._messageData = messageData;
        }

        public StarboardContext(GuildData guildData, MessageData messageData, IUserMessage starredMessage)
        {
            this._guildData = guildData;
            this._messageData = messageData;
            this._starredMessage = starredMessage;
        }

        public StarboardContext(GuildData guildData, IUserMessage starredMessage, ITextChannel starredMessageTextChannel)
        {
            this._guildData = guildData;
            this._starredMessage = starredMessage;
            this._starredMessageTextChannel = starredMessageTextChannel;
        }

        public IGuild Guild
        {
            get
            {
                if (_guild != null)
                {
                    return _guild;
                }

                if (_starredMessageTextChannel != null)
                {
                    _guild = _starredMessageTextChannel.Guild;
                    return _guild;
                }

                if (_starboardTextChannel != null)
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

        public async Task<IUserMessage> GetStarredMessageAsync()
        {
            if (_starredMessage != null)
            {
                return _starredMessage;
            }

            if (Guild != null)
            {
                _starredMessage = await _messageData.GetMessageAsync(Guild);
                return _starredMessage;
            }

            return null;
        }

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
            if (_starredMessage != null)
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

                if (GuildData != null && _starredMessage != null)
                {
                    _messageData = GetOrAddMessageData();
                    return _messageData;
                }

                return null;
            }
            set => _messageData = value;
        }

        private MessageData GetOrAddMessageData()
            => GuildData.messageData.GetOrAdd(
                    _starredMessage.Id,
                    new MessageData(_starredMessage.Id)
                    {
                        created = _starredMessage.CreatedAt,
                        userId = _starredMessage.Author.Id,
                        isNsfw = StarredMessageTextChannel.IsNsfw,
                        channelId = StarredMessageTextChannel.Id,
                        starboardMessageId = _starboardMessage?.Id
                    }
                );
    }
}