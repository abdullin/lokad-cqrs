#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Build.Engine;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class Given_Memory_Configuration : Atomic_storage_scenarios
    {

        protected override void EngineConfig(CqrsEngineBuilder config)
        {
            config.Memory(m =>
                {
                    m.AddMemoryProcess("do");
                    m.AddMemorySender("do", cb => cb.IdGeneratorForTests());
                });
        }
    }
}