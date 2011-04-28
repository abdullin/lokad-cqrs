#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Scenarios;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    public static class Play_all_for_memory
    {
        // ReSharper disable InconsistentNaming


        [TestFixture]
        public sealed class MemoryQuarantine : When_sending_failing_messages
        {
            public MemoryQuarantine()
            {
                EnlistFixtureConfig(builder => builder.Memory(x =>
                    {
                        x.AddMemoryProcess("in");
                        x.AddMemorySender("in", m => m.IdGeneratorForTests());
                    }));
            }
        }
    }
}