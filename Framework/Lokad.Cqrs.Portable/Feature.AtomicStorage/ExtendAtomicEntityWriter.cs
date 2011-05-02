using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendAtomicEntityWriter
    {
        public static TEntity AddOrUpdate<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, Func<TEntity> addFactory, Action<TEntity> update)
        {
            return self.AddOrUpdate(key, addFactory, entity =>
                {
                    update(entity);
                    return entity;
                });
        }
        public static TEntity AddOrUpdate<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, TEntity newView, Action<TEntity> updateViewFactory)
        {
            return self.AddOrUpdate(key, () => newView, view =>
                {
                    updateViewFactory(view);
                    return view;
                }); 
        }

        public static TEntity UpdateOrAdd<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, Action<TEntity> update, Func<TEntity> ifNone)
        {
            return self.UpdateOrAdd(key, entity =>
            {
                update(entity);
                return entity;
            }, ifNone);
        }

        public static TEntity UpdateOrThrow<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, Func<TEntity, TEntity> change)
        {
            return self.UpdateOrAdd(key, change, () =>
                {
                    var txt = string.Format("Failed to load '{0}' with key '{1}'.", typeof (TEntity).Name, key);
                    throw new InvalidOperationException(txt);
                });
        }
        public static TEntity UpdateOrThrow<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, Action<TEntity> change)
        {
            return self.UpdateOrAdd(key, change, () =>
                {
                    var txt = string.Format("Failed to load '{0}' with key '{1}'.", typeof (TEntity).Name, key);
                    throw new InvalidOperationException(txt);
                });
        }

    }
}