using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lokad.Cqrs
{
    public sealed class AzureStorageDictionary
    {
        readonly ConcurrentDictionary<string, IAzureStorageConfiguration> _dictionary = new ConcurrentDictionary<string, IAzureStorageConfiguration>();

        public void Register(IAzureStorageConfiguration config)
        {
            // replace
            _dictionary.AddOrUpdate(config.AccountName, config, (s, configuration) => config);
        }

        public bool TryGet(string accountId, out IAzureStorageConfiguration config)
        {
            return _dictionary.TryGetValue(accountId, out config);
        }

        public ICollection<IAzureStorageConfiguration> GetAll()
        {
            return _dictionary.Values;
        }
        public bool Contains(string accountId)
        {
            return _dictionary.ContainsKey(accountId);
        }
    }
}