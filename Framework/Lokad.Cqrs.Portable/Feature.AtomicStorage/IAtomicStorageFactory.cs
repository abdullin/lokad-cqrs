namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAtomicStorageFactory 
    {
        IAtomicEntityWriter<TKey,TEntity> GetEntityWriter<TKey,TEntity>();
        IAtomicEntityReader<TKey,TEntity> GetEntityReader<TKey,TEntity>();
        IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>();
        IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>();


        /// <summary>
        /// Call this once on start-up to initialize folders
        /// </summary>
        void Initialize();
    }
}