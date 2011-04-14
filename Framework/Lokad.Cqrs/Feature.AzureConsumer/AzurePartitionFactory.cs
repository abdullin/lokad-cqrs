using System;
using System.Collections.Generic;
using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;
using System.Linq;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public sealed class AzurePartitionFactory : IPartitionFactory
	{
		readonly CloudStorageAccount _account;
		readonly IMessageSerializer _serializer;
		readonly ISystemObserver _observer;

		public AzurePartitionFactory(CloudStorageAccount account, IMessageSerializer serializer, ISystemObserver observer)
		{
			_account = account;
			_serializer = serializer;
			_observer = observer;
		}

		public IPartitionNotifier GetNotifier(string[] queueNames)
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

				var foo = Math.Pow(2, (l - 1) / 5.0) / 64d * seconds;

				return TimeSpan.FromSeconds(foo);
			};
		}
	}
}