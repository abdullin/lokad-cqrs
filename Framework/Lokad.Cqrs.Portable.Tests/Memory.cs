#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Dispatch.Events;
using Lokad.Cqrs.Scenarios;
using Lokad.Cqrs.Tests;
using NUnit.Framework;

namespace Lokad.Cqrs
{
    public static class Memory
    {
        // ReSharper disable InconsistentNaming

        public sealed class MemoryConfig : IConfigureEngineForFixture
        {
            public void Config(CloudEngineBuilder builder)
            {
                builder
                    .Memory(x => x.AddPartition("memory:in"))
                    .AddMessageClient("memory:in");
            }
        }

        [TestFixture]
        public sealed class MemoryQuarantine : When_sending_failing_messages<MemoryConfig> {}
    }

}