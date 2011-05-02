using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class AtomicStorageSyntaxTests
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable RedundantTypeArgumentsOfMethod

        sealed class Entity
        {
            public void Do()
            {}
        }

        void VerifyNonAtomic(NuclearStorage storage)
        {
            storage.AddOrUpdateEntity(1, new Entity());

            storage.GetEntity<Entity>(1);
            storage.GetEntity<Entity>(1, new Entity());

            //storage.UpdateOrAddEntity<Entity>(1, e => e.Do());
            //storage.TryDelete<Entity>(1);
            
            //storage.SaveSingleton(new Entity());
            //storage.GetSingleton<Entity>();
            //storage.UpdateSingleton<Entity>(e => e.Do());
            //storage.TryDeleteSingleton<Entity>();
        }
    }
}