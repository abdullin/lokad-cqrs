#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Feature.DirectoryDispatch;
using Lokad.Cqrs.Feature.DirectoryDispatch.Default;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Core.Directory
{
    [TestFixture]
    public sealed class Method_Invoker_tests
    {
        interface IMessage {}

        interface IConsumerWith2Arguments<in T>
        {
            void Consume(T msg, T msg2);
        }

        interface IConumerWithoutGeneric
        {
            void Consume(IMessage msg);
        }

        interface IConsumerWithoutGenericClass
        {
            void Consume<TMessage>(IMessage message);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Two_arguments_are_not_accepted()
        {
            MethodInvokerHint.FromConsumerSample<IConsumerWith2Arguments<IMessage>>(c => c.Consume(null, null));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Non_generic_handler_is_not_accepted()
        {
            MethodInvokerHint.FromConsumerSample<IConumerWithoutGeneric>(c => c.Consume(null));
        }


        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void Handler_with_generic_method_is_not_accepted()
        {
            MethodInvokerHint.FromConsumerSample<IConsumerWithoutGenericClass>(c => c.Consume<IMessage>(null));
        }

        [Test]
        public void Default_definition_is_accepted()
        {
            MethodInvokerHint.FromConsumerSample<IConsume<Feature.DirectoryDispatch.Default.IMessage>>(c => c.Consume(null));
        }
    }
}