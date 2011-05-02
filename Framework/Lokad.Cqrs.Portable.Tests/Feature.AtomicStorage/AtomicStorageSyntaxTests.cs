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
            storage.Save(1, new Entity());

            storage.Get<Entity>(1);
            storage.Get<Entity>(1, new Entity());

            storage.Update<Entity>(1, e => e.Do());
            storage.TryDelete<Entity>(1);
            
            storage.SaveSingleton(new Entity());
            storage.GetSingleton<Entity>();
            storage.UpdateSingleton<Entity>(e => e.Do());
            storage.TryDeleteSingleton<Entity>();
        }
    }
}