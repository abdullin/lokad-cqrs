using System;
using Lokad.Cqrs.Build.Engine;
using Autofac;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class Engine_scenario_for_custom_view_domain : IFiniteEngineScenario
    {

        public interface ICqrsView<TKey>
        {
            
        }
        public sealed class ViewUpdater <TKey,TView> : IAtomicEntityWriter<TKey,TView>
            where TView : ICqrsView<TKey>
        {
            readonly IAtomicEntityWriter<TKey, TView> _inner;

            public ViewUpdater(IAtomicEntityWriter<TKey, TView> inner)
            {
                _inner = inner;
            }

            public TView AddOrUpdate(TKey key, Func<TView> addFactory, Func<TView, TView> update, AddOrUpdateHint hint=AddOrUpdateHint.ProbablyExists)
            {
                return _inner.AddOrUpdate(key, addFactory, update, hint);
            }

            public bool TryDelete(TKey key)
            {
                return _inner.TryDelete(key);
            }
        }

        public sealed class CqrsViewWithTypedKey : ICqrsView<int>
        {
            public int Value;
        }

        public sealed class Message : Define.Command{}
        public sealed class Handler : Define.Handle<Message>
        {
            readonly NuclearStorage _storage;
            readonly ViewUpdater<int, CqrsViewWithTypedKey> _view;
            readonly IMessageSender _sender;

            public Handler(NuclearStorage storage, ViewUpdater<int, CqrsViewWithTypedKey> view, IMessageSender sender)
            {
                _storage = storage;
                _sender = sender;
                _view = view;
            }

            public void Consume(Message atomicMessage, MessageContext context)
            {
                _view.AddOrUpdate(1, v => v.Value += 1);
                _storage.UpdateOrThrowEntity<CqrsViewWithTypedKey>(1, t => t.Value += 1);

                var actual = _storage.GetEntity<CqrsViewWithTypedKey>(1).Convert(c => c.Value).GetValue(0);

                _sender.SendOne(new Message(), meb =>
                    {
                        if (actual != 2)
                        {
                            meb.AddString("fail", "Unexpected value"); 
                        }
                        else
                        {
                            meb.AddString("finish");
                        }
                    });
            }
        }

        

        public override void Configure(CloudEngineBuilder config)
        {
            config.Memory(m =>
            {
                m.AddMemoryProcess("do");
                m.AddMemorySender("do", cb => cb.IdGeneratorForTests());
            });

            config.Advanced(cb => cb.RegisterGeneric(typeof (ViewUpdater<,>)));

            StartupMessages.Add(new Message());
        }
    }
}