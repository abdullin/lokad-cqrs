using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendAtomicEntityReader
    {
        public static Maybe<TEntity> Get<TKey,TEntity>(this IAtomicEntityReader<TKey,TEntity> self, TKey key)
        {
            TEntity entity;
            if (self.TryGet(key, out entity))
            {
                return entity;
            }
            return Maybe<TEntity>.Empty;
        }

        public static TEntity Load<TKey,TEntity>(this IAtomicEntityReader<TKey,TEntity> self, TKey key)
        {
            TEntity entity;
            if (self.TryGet(key, out entity))
            {
                return entity;
            }
            var txt = string.Format("Failed to load '{0}' with key '{1}'.", typeof(TEntity).Name, key);
            throw new InvalidOperationException(txt);
        }
    }
}