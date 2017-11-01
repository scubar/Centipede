using System;
using Centipede.Objects.Interfaces;
using NLog;
using StackExchange.Redis;

namespace Centipede.Objects
{
    /// <summary>
    ///     Redis driver implementing the IDatastore interface.
    /// </summary>
    /// <seealso cref="IDatastore" />
    public class RedisDriver : IDatastore
    {
        private IDatabase Redis { get; } = RedisConnection.Connection.GetDatabase();
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        public long ListLeftPush(string value, string listName)
        {
            try
            {
                return Redis.ListLeftPush(value, listName);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public string ListRightPop(string listName)
        {
            try
            {
                return Redis.ListRightPop(listName);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public bool KeyExists(string key)
        {
            try
            {
                return Redis.KeyExists(key);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public bool StringSet(string key, string value, TimeSpan ttl)
        {
            try
            {
                return Redis.StringSet(key, value, ttl);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public bool Ping()
        {
            try
            {
                Redis.Ping();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                return false;
            }
        }
    }
}