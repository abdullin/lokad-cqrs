using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class NuclearStorageExtensions
    {
        public static TSingleton UpdateSingleton<TSingleton>(this NuclearStorage storage, Action<TSingleton> update)
            where TSingleton : new()
        {
            return storage.Factory.GetSingletonWriter<TSingleton>().UpdateEnforcingNew(update);
        }
    }
}