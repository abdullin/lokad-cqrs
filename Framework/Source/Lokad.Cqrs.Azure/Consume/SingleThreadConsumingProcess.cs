#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Dispatch;

namespace Lokad.Cqrs.Consume
{
	public sealed class SingleThreadConsumingProcess : IEngineProcess
	{
		readonly ISingleThreadMessageDispatcher _dispatcher;
		readonly ISystemObserver _observer;
		readonly AzureReadQueue[] _queues;
		readonly Func<uint, TimeSpan> _threadSleepInterval;


		public SingleThreadConsumingProcess(ISystemObserver observer,
			ISingleThreadMessageDispatcher dispatcher, Func<uint, TimeSpan> sleepWhenNoMessages, AzureReadQueue[] readQueues)
		{
			_queues = readQueues;
			_dispatcher = dispatcher;
			_observer = observer;
			_threadSleepInterval = sleepWhenNoMessages;
		}

		public void Dispose()
		{
			_disposal.Dispose();
		}

		public void Initialize()
		{
			foreach (var queue in _queues)
			{
				queue.Init();
			}
		}

		readonly CancellationTokenSource _disposal = new CancellationTokenSource();

		public Task Start(CancellationToken token)
		{
			return Task.Factory.StartNew(() => ReceiveMessages(token), token);
		}


		void ReceiveMessages(CancellationToken outer)
		{
			uint beenIdleFor = 0;

			using (var source = CancellationTokenSource.CreateLinkedTokenSource(_disposal.Token, outer))
			{
				var token = source.Token;

				while (!token.IsCancellationRequested)
				{
					var messageFound = false;
					foreach (var messageQueue in _queues)
					{
						if (token.IsCancellationRequested)
							return;

						// selector policy goes here
						if (ProcessQueueForMessage(messageQueue) == QueueProcessingResult.MoreWork)
						{
							messageFound = true;
						}
					}

					if (messageFound)
					{
						beenIdleFor = 0;
					}
					else
					{
						beenIdleFor += 1;
						var sleepInterval = _threadSleepInterval(beenIdleFor);
						token.WaitHandle.WaitOne(sleepInterval);
					}
				}
			}
		}

		QueueProcessingResult ProcessQueueForMessage(AzureReadQueue queue)
		{
			var result = queue.GetMessage();

			switch (result.State)
			{
				case GetMessageResultState.Success:
					var envelope = result.Message.Unpacked;
					try
					{
						_dispatcher.DispatchMessage(envelope);
						queue.AckMessage(result.Message);
					}
					catch (Exception ex)
					{
						_observer.Notify(new FailedToConsumeMessage(ex, envelope.EnvelopeId, queue.Name));
					}

					return QueueProcessingResult.MoreWork;

				case GetMessageResultState.Wait:
					return QueueProcessingResult.Sleep;

				case GetMessageResultState.Exception:
					return QueueProcessingResult.MoreWork;

				case GetMessageResultState.Retry:
					//tx.Complete();
					// retry immediately
					return QueueProcessingResult.MoreWork;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		enum QueueProcessingResult
		{
			MoreWork,
			Sleep
		}
	}
}