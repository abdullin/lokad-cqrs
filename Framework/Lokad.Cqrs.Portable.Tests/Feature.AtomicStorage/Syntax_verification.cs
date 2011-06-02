#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture, Explicit]
    public sealed class Syntax_verification
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable RedundantTypeArgumentsOfMethod

        sealed class Entity
        {
            public void Do() {}
        }

        void VerifyNonAtomic(NuclearStorage storage)
        {
            storage.AddOrUpdateEntity(1, new Entity());
            storage.GetEntity<Entity>(1);
            storage.UpdateEntity<Entity>(1, e => e.Do());
            storage.TryDeleteEntity<Entity>(1);


            storage.AddOrUpdateSingleton(() => new Entity(), e => e.Do());
            storage.UpdateSingletonEnforcingNew<Entity>(e => e.Do());
            storage.GetSingleton<Entity>();
            storage.TryDeleteSingleton<Entity>();
            storage.UpdateSingleton<Entity>(e => e.Do());


            //storage.UpdateOrAddEntity<Entity>(1, e => e.Do());
            //storage.TryDelete<Entity>(1);

            //storage.SaveSingleton(new Entity());
            //storage.GetSingleton<Entity>();
            //storage.UpdateSingleton<Entity>(e => e.Do());
            //storage.TryDeleteSingleton<Entity>();
        }

        [DataContract]
        public sealed class Customer : Define.AtomicEntity
        {
            [DataMember(Order = 1)] public string Name;
            [DataMember(Order = 2)] public string Address;
        }


        [Test]
        public void Sample()
        {
            NuclearStorage storage = null;
            var customer = new Customer()
                {
                    Name = "My first customer",
                    Address = "The address"
                };
            storage.AddEntity("cust-123", customer);
            storage.UpdateEntity<Customer>("cust-123", c =>
                {
                    c.Address = "Customer Moved";
                });
            
            var result = storage.GetEntity<Customer>("cust-123").Value;
            Console.WriteLine("{0}: {1}", result.Name, result.Address);
        }
    }
}