using System;

namespace Centipede.Objects.Interfaces
{
    public interface IDatastore
    {
        long ListLeftPush(string value, string listName);
        string ListRightPop(string listName);
        bool KeyExists(string key);
        bool StringSet(string key, string value, TimeSpan ttl);
        bool Ping();
    }
}