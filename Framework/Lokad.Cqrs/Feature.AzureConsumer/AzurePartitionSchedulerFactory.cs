#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Core.Partition;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Feature.AzureSender;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Feature.AzureConsumer
{
	public sealed class AzurePartitionSchedulerFactory : 
		IPartitionSchedulerFactory, 
		IEngineProcess
		

	{
		readonly CloudStorageAccount _account;
		readonly IMessageSerializer _serializer;
		readonly ISystemObserver _observer;

		ConcurrentDictionary<string,StatelessAzureQueueReader> _queues = new ConcurrentDictionary<string, StatelessAzureQueueReader>();
		ConcurrentDictionary<string, StatelessAzureFutureList> _futures = new ConcurrentDictionary<string, StatelessAzureFutureList>();
		ConcurrentDictionary<string, AzureWriteQueue> _writers = new ConcurrentDictionary<string, AzureWriteQueue>();

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
				.Select(n => _queues.GetOrAdd(n, name => new StatelessAzureQueueReader(_account, name, _observer, _serializer)))
				.ToArray();

			var futures = queueNames
				.Select(n => _futures.GetOrAdd(n, name => new StatelessAzureFutureList(_account, name, _observer, _serializer)))
				.ToArray();
			return new AzurePartitionNotifier(queues, futures, BuildDecayPolicy(TimeSpan.FromSeconds(2)));
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

		public void Dispose()
		{
			
		}

		public void Initialize()
		{
			
		}

		public Task Start(CancellationToken token)
		{
			return Task.Factory.StartNew(() =>
			{
				while (!token.IsCancellationRequested)
				{
					foreach (var list in _futures)
					{
						MessageEnvelope envelope;
						while (list.Value.TakePendingMessage(out envelope))
						{
							_writers
								.GetOrAdd(list.Key, n => new AzureWriteQueue(_serializer, _account, n))
								.PutMessage(envelope);
						}
					}

					token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2));
				}
			});
		}
	}
}