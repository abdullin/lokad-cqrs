using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lokad.Cqrs
{
    public sealed class AzureAccessRegistry
    {
        readonly ConcurrentDictionary<string, IAzureAccessConfiguration> _dictionary = new ConcurrentDictionary<string, IAzureAccessConfiguration>();

        public void Register(IAzureAccessConfiguration config)
        {
            // replace
            _dictionary.AddOrUpdate(config.AccountName, config, (s, configuration) => config);
        }

        public bool TryGet(string accountId, out IAzureAccessConfiguration config)
        {
            return _dictionary.TryGetValue(accountId, out config);
        }

        public IAzureAccessConfiguration GetOrThrow(string accountId)
        {
            IAzureAccessConfiguration value;
            if (_dictionary.TryGetValue(accountId,out value))
            {
                return value;
            }
            var message = string.Format("Failed to locate Azure config with id '{0}'. Have you registered it in Azure?", accountId);
            throw new InvalidOperationException(message);
        }

        public ICollection<IAzureAccessConfiguration> GetAll()
        {
            return _dictionary.Values;
        }
        public bool Contains(string accountId)
        {
            return _dictionary.ContainsKey(accountId);
        }
    }
}