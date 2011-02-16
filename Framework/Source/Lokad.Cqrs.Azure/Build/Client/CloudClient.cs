#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;
using System.Threading;
using Autofac;


namespace Lokad.Cqrs
{
	
	public class CloudClient : ICloudClient
	{
		readonly Lazy<IMessageClient> _client;
		readonly IComponentContext _resolver;

		public CloudClient(IComponentContext resolver)
		{
			_resolver = resolver;
			_client = new Lazy<IMessageClient>(GetClient, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public CloudClient(IComponentContext resolver, Lazy<IMessageClient> client)
		{
			_resolver = resolver;
			_client = client;
		}

		public void SendMessage(object message)
		{
			_client.Value.Send(message);
		}

		public TService Resolve<TService>()
		{
			try
			{
				return _resolver.Resolve<TService>();
			}
			catch (TargetInvocationException e)
			{
				throw Errors.Inner(e);
			}
		}

		IMessageClient GetClient()
		{
			try
			{
				return _resolver.Resolve<IMessageClient>();
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(
					"Failed to resolve message client. Have you used Builder.AddMessageClient or Builder.BuildFor(name)?", e);
			}
		}
	}
}