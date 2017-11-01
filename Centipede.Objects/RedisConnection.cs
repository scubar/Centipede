using System;
using NLog;
using StackExchange.Redis;

namespace Centipede.Objects
{
    public class RedisConnection
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var connectionMultiplexer =
                ConnectionMultiplexer.Connect(
                    "172.16.1.200:6379,172.16.1.201:6379,password=V01P@dmin,connectRetry=3,connectTimeout=5000");
            Logger.Debug("Connected to redis");
            connectionMultiplexer.PreserveAsyncOrder = false;
            return connectionMultiplexer;
        });

        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        internal static ConnectionMultiplexer Connection => LazyConnection.Value;
    }
}