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

        public IAzureStorageConfiguration GetOrThrow(string accountId)
        {
            IAzureStorageConfiguration value;
            if (_dictionary.TryGetValue(accountId,out value))
            {
                return value;
            }
            var message = string.Format("Failed to locate Azure config with id '{0}'. Have you registered it in Azure?", accountId);
            throw new InvalidOperationException(message);
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