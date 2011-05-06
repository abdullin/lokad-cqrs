using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendAtomicEntityWriter
    {
        public static TEntity AddOrUpdate<TKey,TEntity>(this IAtomicEntityWriter<TKey, TEntity> self, TKey key, Func<TEntity> addFactory, Action<TEntity> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
        {
            return self.AddOrUpdate(key, addFactory, entity =>
                {
                    update(entity);
                    return entity;
                }, hint);
        }
        public static TEntity AddOrUpdate<TKey,TEntity>(this IAtomicEntityWriter<TKey,TEntity> self, TKey key, TEntity newView, Action<TEntity> updateViewFactory, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
        {
            return self.AddOrUpdate(key, () => newView, view =>
                {
                    updateViewFactory(view);
                    return view;
                }, hint); 
        }


        public static TEntity UpdateOrThrow<TKey,TEntity>(this IAtomicEntityWriter<TKey,TEntity> self, TKey key, Func<TEntity, TEntity> change)
        {
            return self.AddOrUpdate(key, () =>
                {
                    var txt = String.Format("Failed to load '{0}' with key '{1}'.", typeof (TEntity).Name, key);
                    throw new InvalidOperationException(txt);
                }, change, AddOrUpdateHint.ProbablyExists);
        }
        public static TEntity UpdateOrThrow<TKey,TEntity>(this IAtomicEntityWriter<TKey,TEntity> self, TKey key, Action<TEntity> change)
        {
            return self.AddOrUpdate(key, () =>
                {
                    var txt = String.Format("Failed to load '{0}' with key '{1}'.", typeof (TEntity).Name, key);
                    throw new InvalidOperationException(txt);
                }, change, AddOrUpdateHint.ProbablyExists);
        }

        public static TView UpdateEnforcingNew<TKey,TView>(this IAtomicEntityWriter<TKey,TView> self, TKey key,
            Action<TView> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
            where TView : new()
        {
            return self.AddOrUpdate(key, () =>
                {
                    var view = new TView();
                    update(view);
                    return view;
                }, v =>
                    {
                        update(v);
                        return v;
                    }, hint);
        }
        
    }
}