#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Partition;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{
	public sealed class AzurePartitionFactory : IEngineProcess
	{
		readonly CloudStorageAccount _account;
		readonly IEnvelopeStreamer _streamer;
		readonly ISystemObserver _observer;

		readonly ConcurrentBag<AzurePartitionScheduler> _schedulers = new ConcurrentBag<AzurePartitionScheduler>();


		public AzurePartitionFactory(CloudStorageAccount account, IEnvelopeStreamer streamer,
			ISystemObserver observer)
		{
			_account = account;
			_streamer = streamer;
			_observer = observer;
		}

		public IPartitionInbox GetNotifier(string[] queueNames)
		{
			var queues = queueNames
				.Select(name => new StatelessAzureQueueReader(_account, name, _observer, _streamer))
				.ToArray();

			var futures = queueNames
				.Select(name => new StatelessAzureFutureList(_account, name, _observer, _streamer))
				.ToArray();

			var writers = queueNames
				.Select(name => new StatelessAzureQueueWriter(_streamer, _account, name))
				.ToArray();

			_schedulers.Add(new AzurePartitionScheduler(writers, futures));

			return new AzurePartitionInbox(queues, futures, BuildDecayPolicy(TimeSpan.FromSeconds(2)));
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
						foreach (var scheduler in _schedulers)
						{
							scheduler.DispatchDelayedMessages();
						}

						token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2));
					}
				});
		}
	}
}