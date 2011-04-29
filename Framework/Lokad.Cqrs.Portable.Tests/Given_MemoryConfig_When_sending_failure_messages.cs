#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Scenarios;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs
{
    [TestFixture]
    public sealed class Given_MemoryConfig_When_sending_failure_messages : When_sending_failing_messages
    {
        public Given_MemoryConfig_When_sending_failure_messages()
        {
            EnlistFixtureConfig(builder => builder.Memory(x =>
                {
                    x.AddMemoryProcess("in");
                    x.AddMemorySender("in", m => m.IdGeneratorForTests());
                }));
        }
    }
}