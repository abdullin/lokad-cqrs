#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Quality;
using NHibernate;

namespace Lokad.Cqrs.NHibernate
{
	[UsedImplicitly]
	public sealed class NHibernateStarter : IEngineProcess
	{
		// is needed to initialize NHibernate on start.
		readonly ISessionFactory _factory;
		readonly ILog _log;

		public NHibernateStarter(ISessionFactory factory, ILogProvider provider)
		{
			_factory = factory;
			_log = provider.LogForName<NHibernateStarter>();
		}


		public void Dispose()
		{
		}

		public void Initialize()
		{
			_log.DebugFormat("NHibernate started: {0}", _factory.Statistics.StartTime);
		}

		public Task Start(CancellationToken token)
		{
			return Task.Factory.StartNew(() => { }, token);
		}
	}
}