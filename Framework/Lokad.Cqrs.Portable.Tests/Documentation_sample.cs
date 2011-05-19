#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Runtime.Serialization;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Feature.AtomicStorage;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class Documentation_sample
    {
        [DataContract]
        public sealed class CreateCustomer : Define.Command
        {
            [DataMember] public string CustomerName;
            [DataMember] public int CustomerId;
        }


        [DataContract]
        public sealed class CustomerCreated : Define.Event
        {
            [DataMember] public int CustomerId;
            [DataMember] public string CustomerName;
        }

        [DataContract]
        public sealed class Customer
        {
            [DataMember] public string Name;
            [DataMember] public int Id;

            public Customer(int id, string name)
            {
                Name = name;
                Id = id;
            }
        }

        public sealed class CustomerHandler : Define.Handle<CreateCustomer>
        {
            readonly NuclearStorage _storage;
            readonly IMessageSender _sender;

            public CustomerHandler(NuclearStorage storage, IMessageSender sender)
            {
                _storage = storage;
                _sender = sender;
            }

            public void Consume(CreateCustomer cmd)
            {
                var customer = new Customer(cmd.CustomerId, cmd.CustomerName);
                _storage.AddEntity(customer.Id, customer);
                _sender.SendOne(new CustomerCreated
                    {
                        CustomerId = cmd.CustomerId,
                        CustomerName = cmd.CustomerName
                    });
            }
        }

        [Test, Explicit]
        public void Run_in_test()
        {
            var builder = new CqrsEngineBuilder();
            builder.Memory(m =>
                {
                    m.AddMemorySender("work");
                    m.AddMemoryProcess("work");
                });
            builder.Storage(m => m.AtomicIsInMemory());

            using (var engine = builder.Build())
            {
                //engine.Resolve<IMessageSender>().SendOne(new CreateCustomer()
                //{
                //    CustomerId = 1,
                //    CustomerName = "Rinat Abdullin"
                //});
                engine.RunForever();
            }
        }
    }
}