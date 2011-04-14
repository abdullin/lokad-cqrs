#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public sealed class AzurePartitionSchedulerFactory : IPartitionSchedulerFactory
	{
		readonly CloudStorageAccount _account;
		readonly IMessageSerializer _serializer;
		readonly ISystemObserver _observer;

		public AzurePartitionSchedulerFactory(CloudStorageAccount account, IMessageSerializer serializer,
			ISystemObserver observer)
		{
			_account = account;
			_serializer = serializer;
			_observer = observer;
		}

		public IPartitionScheduler GetNotifier(string[] queueNames)
		{
			var queues = queueNames
				.Select(n => new AzureReadQueue(_account, n, _observer, _serializer))
				.ToArray();
			return new AzurePartitionNotifier(queues, BuildDecayPolicy(TimeSpan.FromSeconds(2)));
		}

		static Func<uint, TimeSpan> BuildDecayPolicy(TimeSpan maxDecay)
		{
			//var seconds = (Rand.Next(0, 1000) / 10000d).Seconds();
			var seconds = maxDecay.TotalSeconds;
			return l =>
				{
					if (l >= 31)
					{
						return maxDecay;
					}

					var foo = Math.Pow(2, (l - 1)/5.0)/64d*seconds;

					return TimeSpan.FromSeconds(foo);
				};
		}
	}
}