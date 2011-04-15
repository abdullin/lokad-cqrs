#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Partition.Events;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Core.Partition
{
	public sealed class SingleThreadConsumingProcess : IEngineProcess
	{
		readonly ISingleThreadMessageDispatcher _dispatcher;
		readonly ISystemObserver _observer;
		readonly IPartitionInbox _notifier;

		public SingleThreadConsumingProcess(ISystemObserver observer,
			ISingleThreadMessageDispatcher dispatcher, IPartitionInbox notifier)
		{
			_dispatcher = dispatcher;
			_observer = observer;
			_notifier = notifier;
		}

		public void Dispose()
		{
			_disposal.Dispose();
		}

		public void Initialize()
		{
			_notifier.Init();
		}

		readonly CancellationTokenSource _disposal = new CancellationTokenSource();

		public Task Start(CancellationToken token)
		{
			return Task.Factory.StartNew(() => ReceiveMessages(token), token);
		}


		void ReceiveMessages(CancellationToken outer)
		{
			using (var source = CancellationTokenSource.CreateLinkedTokenSource(_disposal.Token, outer))
			{
				var token = source.Token;
				MessageContext context;
				while (_notifier.TakeMessage(token, out context))
				{
					try
					{
						_dispatcher.DispatchMessage(context.Unpacked);
					}
					catch (Exception ex)
					{
						_observer.Notify(new FailedToConsumeMessage(ex, context.Unpacked.EnvelopeId, context.QueueName));
						// not a big deal
						_notifier.TryNotifyNack(context);
					}
					try
					{
						_notifier.AckMessage(context);
					}
					catch (Exception ex)
					{
						// not a big deal. Message will be processed again.
						_observer.Notify(new FailedToAckMessage(ex, context.Unpacked.EnvelopeId, context.QueueName));
					}
				}
			}
		}
	}
}