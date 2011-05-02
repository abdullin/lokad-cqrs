using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendAtomicEntityWriter
    {
        public static TEntity AddOrUpdate<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, Func<TEntity> addFactory, Action<TEntity> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
        {
            return self.AddOrUpdate(key, addFactory, entity =>
                {
                    update(entity);
                    return entity;
                }, hint);
        }
        public static TEntity AddOrUpdate<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, TEntity newView, Action<TEntity> updateViewFactory, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
        {
            return self.AddOrUpdate(key, () => newView, view =>
                {
                    updateViewFactory(view);
                    return view;
                }, hint); 
        }


        public static TEntity UpdateOrThrow<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, Func<TEntity, TEntity> change)
        {
            return self.AddOrUpdate(key, () =>
                {
                    var txt = String.Format("Failed to load '{0}' with key '{1}'.", typeof (TEntity).Name, key);
                    throw new InvalidOperationException(txt);
                }, change, AddOrUpdateHint.ProbablyExists);
        }
        public static TEntity UpdateOrThrow<TEntity>(this IAtomicEntityWriter<TEntity> self, string key, Action<TEntity> change)
        {
            return self.AddOrUpdate(key, () =>
                {
                    var txt = String.Format("Failed to load '{0}' with key '{1}'.", typeof (TEntity).Name, key);
                    throw new InvalidOperationException(txt);
                }, change, AddOrUpdateHint.ProbablyExists);
        }

        public static TView AddOrUpdate<TView>(this IAtomicEntityWriter<TView> self, string key,
            Action<TView> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
            where TView : new()
        {
            return self.AddOrUpdate(key, () =>
                {
                    var view = new TView();
                    update(view);
                    return view;
                }, view1 =>
                    {
                        update(view1);
                        return view1;
                    }, hint);
        }
    }
}