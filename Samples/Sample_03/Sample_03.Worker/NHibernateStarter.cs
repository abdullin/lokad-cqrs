#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs;
using NHibernate;

namespace Sample_03.Worker
{
    public sealed class NHibernateStarter : IEngineProcess
    {
        // is needed to initialize NHibernate on start.
        readonly ISessionFactory _factory;

        public NHibernateStarter(ISessionFactory factory)
        {
            _factory = factory;
        }

        public void Dispose() {}

        public void Initialize()
        {
            Trace.WriteLine(string.Format("NHibernate started: {0}", _factory.Statistics.StartTime));
        }

        public Task Start(CancellationToken token)
        {
            return Task.Factory.StartNew(() => { }, token);
        }
    }
}