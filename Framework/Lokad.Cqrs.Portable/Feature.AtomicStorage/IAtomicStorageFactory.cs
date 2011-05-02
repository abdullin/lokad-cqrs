namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAtomicStorageFactory 
    {
        IAtomicEntityWriter<TEntity> GetEntityWriter<TEntity>();
        IAtomicEntityReader<TEntity> GetEntityReader<TEntity>();
        IAtomicSingletonReader<TSingleton> GetSingletonReader<TSingleton>();
        IAtomicSingletonWriter<TSingleton> GetSingletonWriter<TSingleton>();

        /// <summary>
        /// Call this once on start-up to initialize folders
        /// </summary>
        void Initialize();
    }
}