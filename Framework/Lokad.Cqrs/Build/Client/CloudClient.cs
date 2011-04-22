#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;
using System.Threading;
using Autofac;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Build.Client
{
	public class CloudClient : IDisposable
	{
		readonly Lazy<IMessageSender> _client;
		readonly ILifetimeScope _resolver;

		public CloudClient(ILifetimeScope resolver)
		{
			_resolver = resolver;
			_client = new Lazy<IMessageSender>(() => GetClient(resolver), LazyThreadSafetyMode.ExecutionAndPublication);
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
				throw InvocationUtil.Inner(e);
			}
		}

		static IMessageSender GetClient(ILifetimeScope resolver)
		{
			try
			{
				return resolver.Resolve<IMessageSender>();
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(
					"Failed to resolve message client. Have you used Builder.AddMessageClient or Builder.BuildFor(name)?", e);
			}
		}

		public void Dispose()
		{
			_resolver.Dispose();
		}
	}
}