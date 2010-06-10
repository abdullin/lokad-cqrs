#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;

namespace CloudBus.Build.Cloud
{
	public interface ICloudBusHost
	{
		void Start();
		void Initialize();
		void Stop();
		IContainer Container { get; }
	}
}