using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace P1x3lc0w.DiscordStarboardBot
{
    [JsonConverter(typeof(StarGivingUsersJsonConverter))]
    class StarGivingUsersList
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        internal Dictionary<ulong, StarboardSource> Dictionary { get; private set; }

        public StarGivingUsersList()
        {
            Dictionary = new Dictionary<ulong, StarboardSource>();
        }

        public StarGivingUsersList(Dictionary<ulong, StarboardSource> dictionary)
        {
            Dictionary = dictionary;
        }

        public bool TryAdd(ulong key, StarboardSource value)
        {
            try
            {
                _lock.EnterWriteLock();
                return Dictionary.TryAdd(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool AddOrUpdate(ulong key, StarboardSource addValue, StarboardSource updateValue)
        {
            try
            {
                _lock.EnterWriteLock();
                if(!Dictionary.TryAdd(key, addValue))
                {
                    Dictionary[key] = updateValue;
                    return false;
                }

                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(ulong key, StarboardSource value)
        {
            try
            {
                _lock.EnterUpgradeableReadLock();

                if(Dictionary.ContainsKey(key) && Dictionary[key] == value)
                {
                    try
                    {
                        _lock.EnterReadLock();
                        Dictionary.Remove(key);
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public int Count()
        {
            try
            {
                _lock.EnterReadLock();
                return Dictionary.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            try
            {
                _lock.EnterWriteLock();
                Dictionary.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
