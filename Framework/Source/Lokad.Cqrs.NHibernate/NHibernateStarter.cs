#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Quality;
using NHibernate;

namespace Lokad.Cqrs.NHibernate
{
	[UsedImplicitly]
	public sealed class NHibernateStarter : IStartable
	{
		// is needed to initialize NHibernate on start.
		readonly ISessionFactory _factory;
		readonly ILog _log;

		public NHibernateStarter(ISessionFactory factory, ILogProvider provider)
		{
			_factory = factory;
			_log = provider.CreateLog<NHibernateStarter>();
		}


		public void Dispose()
		{
		}

		public void StartUp()
		{
			_log.DebugFormat("NHibernate started: {0}", _factory.Statistics.StartTime);
		}
	}
}