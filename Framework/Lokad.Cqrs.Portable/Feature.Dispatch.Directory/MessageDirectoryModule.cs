#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Feature.Dispatch.Directory.Default;

// ReSharper disable UnusedMember.Global

namespace Lokad.Cqrs.Feature.Dispatch.Directory
{
    /// <summary>
    /// Module for building CQRS domains.
    /// </summary>
    public class MessageDirectoryModule : HideObjectMembersFromIntelliSense
    {
        readonly DomainAssemblyScanner _scanner = new DomainAssemblyScanner();
        IMethodContextManager _contextManager;
        MethodInvokerHint _hint;


        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDirectoryModule"/> class.
        /// </summary>
        public MessageDirectoryModule()
        {
            HandlerSample<IConsume<IMessage>>(a => a.Consume(null));
            ContextFactory(
                (envelope, message) => new MessageContext(envelope.EnvelopeId, message.Index, envelope.CreatedOnUtc));
        }


        /// <summary>
        /// Allows to specify custom context factory to expose transport-level
        /// information to message handlers via IoC. By default this is configured
        /// as <see cref="Func{T}"/> returning <see cref="MessageContext"/>
        /// </summary>
        /// <typeparam name="TContext">The type of the context to return.</typeparam>
        /// <param name="contextFactory">The context factory.</param>
        public void ContextFactory<TContext>(Func<ImmutableEnvelope, ImmutableMessage, TContext> contextFactory)
            where TContext : class
        {
            if (contextFactory == null) throw new ArgumentNullException("contextFactory");
            var instance = new MethodContextManager<TContext>(contextFactory);
            _contextManager = instance;
        }

        /// <summary>
        /// Specifies expression describing your interface lookup rules for handlers and messages.
        /// Defaults to <code><![CDATA[HandlerSample<IConsume<IMessage>>(h => h.Consume(null))]]></code>
        /// </summary>
        /// <typeparam name="THandler">The base type of the handler.</typeparam>
        /// <param name="handlerSampleExpression">The handler sample expression.</param>
        public void HandlerSample<THandler>(Expression<Action<THandler>> handlerSampleExpression)
        {
            if (handlerSampleExpression == null) throw new ArgumentNullException("handlerSampleExpression");
            _hint = MethodInvokerHint.FromConsumerSample(handlerSampleExpression);
        }


        /// <summary>
        /// Includes assemblies of the specified types into the discovery process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public void InAssemblyOf<T>()
        {
            _scanner.WithAssemblyOf<T>();
        }

        public void InAssemblyOf(object instance)
        {
            _scanner.WithAssembly(instance.GetType().Assembly);
        }

        public void WhereMessages(Predicate<Type> constraint)
        {
            _scanner.WhereMessages(constraint);
        }


        public void Configure(IComponentRegistry container)
        {
            _scanner.Constrain(_hint);
            var mappings = _scanner.Build(_hint.ConsumerTypeDefinition);
            var builder = new MessageDirectoryBuilder(mappings);

            var provider = _contextManager.GetContextProvider();

            var consumers = mappings
                .Select(x => x.Consumer)
                .Where(x => !x.IsAbstract)
                .Distinct()
                .ToArray();

            var cb = new ContainerBuilder();
            foreach (var consumer in consumers)
            {
                cb.RegisterType(consumer);
            }
            cb.RegisterInstance(provider).AsSelf();
            cb.Update(container);
            container.Register<IMessageDispatchStrategy>(c =>
                {
                    var scope = c.Resolve<ILifetimeScope>();
                    var tx = TransactionEvil.Factory(TransactionScopeOption.RequiresNew);
                    return new AutofacDispatchStrategy(scope, tx, _hint.Lookup, _contextManager);
                });

            container.Register(builder);
        }
    }
}