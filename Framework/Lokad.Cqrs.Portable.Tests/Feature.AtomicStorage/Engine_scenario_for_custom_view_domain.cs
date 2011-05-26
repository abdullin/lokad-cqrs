#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;
using Autofac;
using Lokad.Cqrs.Build.Engine;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class Engine_scenario_for_custom_view_domain : FiniteEngineScenario
    {
        [DataContract]
        public sealed class CustomDomainViewWithTypedKey : ICqrsView<int>
        {
            [DataMember(Order = 1)]
            public int Value { get; set; }
        }
        public interface ICqrsView<TKey>  {}

        public sealed class ViewUpdater<TKey, TView> : IAtomicEntityWriter<TKey, TView>
            where TView : ICqrsView<TKey>
        {
            readonly IAtomicEntityWriter<TKey, TView> _inner;

            public ViewUpdater(IAtomicEntityWriter<TKey, TView> inner)
            {
                _inner = inner;
            }

            public TView AddOrUpdate(TKey key, Func<TView> addFactory, Func<TView, TView> update,
                AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
            {
                return _inner.AddOrUpdate(key, addFactory, update, hint);
            }

            public bool TryDelete(TKey key)
            {
                return _inner.TryDelete(key);
            }
        }

        [DataContract]
        public sealed class Message : Define.Command {}
        [DataContract]
        public sealed class FinishMessage : Define.Command { }

        public sealed class Handler : Define.Handle<Message>
        {
            readonly NuclearStorage _storage;
            readonly ViewUpdater<int, CustomDomainViewWithTypedKey> _view;
            readonly IMessageSender _sender;

            public Handler(NuclearStorage storage, ViewUpdater<int, CustomDomainViewWithTypedKey> view, IMessageSender sender)
            {
                _storage = storage;
                _sender = sender;
                _view = view;
            }

            public void Consume(Message message)
            {
                _view.UpdateEnforcingNew(1, v => v.Value += 1);
                _storage.UpdateEntity<CustomDomainViewWithTypedKey>(1, t => t.Value += 1);

                var actual = _storage.GetEntity<CustomDomainViewWithTypedKey>(1).Convert(c => c.Value).GetValue(0);

                _sender.SendOne(new FinishMessage(), meb =>
                    {
                        if (actual != 2)
                        {
                            meb.AddString("fail", "Expected 2 but got " + actual);
                        }
                        else
                        {
                            meb.AddString("finish");
                        }
                    });
            }
        }


        protected override void Configure(CqrsEngineBuilder config)
        {
            config.Advanced.ConfigureContainer(cb => cb.RegisterGeneric(typeof (ViewUpdater<,>)));
            EnlistMessage(new Message());
        }
    }

    
}