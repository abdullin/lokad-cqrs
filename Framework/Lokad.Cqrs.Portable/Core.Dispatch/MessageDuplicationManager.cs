#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Lokad.Cqrs.Core.Dispatch
{
    ///<summary>
    /// Shoud be registered as singleton, manages actual memories
    /// and performs cleanups in async
    ///</summary>
    public sealed class MessageDuplicationManager : IEngineProcess
    {
        readonly ConcurrentDictionary<ISingleThreadMessageDispatcher, MessageDuplicationMemory> _memories =
            new ConcurrentDictionary<ISingleThreadMessageDispatcher, MessageDuplicationMemory>();

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public MessageDuplicationMemory GetOrAdd(ISingleThreadMessageDispatcher dispatcher)
        {
            return _memories.GetOrAdd(dispatcher, s => new MessageDuplicationMemory());
        }

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        foreach (var memory in _memories)
                        {
                            memory.Value.ForgetOlderThan(TimeSpan.FromMinutes(20));
                        }

                        token.WaitHandle.WaitOne(TimeSpan.FromMinutes(5));
                    }
                }, token);
        }
    }
}