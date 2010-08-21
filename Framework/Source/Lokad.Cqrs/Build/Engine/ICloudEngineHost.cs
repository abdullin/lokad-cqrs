#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lokad.Cqrs
{
	public interface ICloudEngineHost : IDisposable
	{
		Task Start(CancellationToken token);
		void Initialize();
		TService Resolve<TService>();
	}
}