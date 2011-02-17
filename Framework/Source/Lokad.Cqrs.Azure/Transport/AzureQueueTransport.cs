#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

using Lokad.Cqrs.Queue;

namespace Lokad.Cqrs.Transport
{
	
	public sealed class AzureQueueTransport : IDisposable
	{
		readonly IQueueManager _factory;
		readonly IsolationLevel _isolationLevel;
		readonly ILog _log;
		readonly string[] _queueNames;
		readonly IReadMessageQueue[] _queues;
		readonly int _degreeOfParallelism;
		readonly Func<uint, TimeSpan> _threadSleepInterval;

		public AzureQueueTransport(
			AzureQueueTransportConfig config,
			ILogProvider logProvider,
			IQueueManager factory)
		{
			_factory = factory;
			_queueNames = config.QueueNames;
			_isolationLevel = config.IsolationLevel;
			_log = logProvider.Get(typeof (AzureQueueTransport).Name + "." + config.LogName);
			_threadSleepInterval = config.SleepWhenNoMessages;
			_degreeOfParallelism = config.DegreeOfParallelism;
			
			_queues = new IReadMessageQueue[_queueNames.Length];
		}

		public event Action<UnpackedMessage> MessageReceived = m => { };
		public event Action<UnpackedMessage, Exception> MessageHandlerFailed = (message, exception) => { };
		
		public void Dispose()
		{
			_disposal.Dispose();
		}

		public void Initialize()
		{
			for (int i = 0; i < _queueNames.Length; i++)
			{
				_queues[i] = _factory.GetReadQueue(_queueNames[i]);
			}

		}

		readonly CancellationTokenSource _disposal = new CancellationTokenSource();

		public Task[] Start(CancellationToken token)
		{
			_log.DebugFormat("Starting transport for {0}", _queueNames.Join(";"));

			var array = new Task[_degreeOfParallelism];
			for (int i = 0; i < _degreeOfParallelism; i++)
			{
				array[i] = Task.Factory.StartNew(() => ReceiveMessages(token), token);
			}
			return array;
		}
	

		Maybe<Exception> GetProcessingFailure(IReadMessageQueue queue, UnpackedMessage message)
		{
			try
			{
				ProcessSingleMessage(message, m => MessageReceived(m));
				return Maybe<Exception>.Empty;
			}
			catch (Exception ex)
			{
				var text = string.Format("Failed to consume '{0}' from '{1}'", message, queue.Uri);

				_log.Error(ex, text);
				return ex;
			}
		}

		void ProcessSingleMessage(UnpackedMessage message, Action<UnpackedMessage> messageHandlers)
		{
			if (messageHandlers == null)
				return;

			try
			{
				MessageContext.OverrideContext(message);
				foreach (Action<UnpackedMessage> func in messageHandlers.GetInvocationList())
				{
					func(message);
				}
			}
			finally
			{
				MessageContext.ClearContext();
			}
		}

		void MessageHandlingProblem(UnpackedMessage message, Exception ex)
		{
			// notify all subscribers
			foreach (Action<UnpackedMessage, Exception> @delegate in MessageHandlerFailed.GetInvocationList())
			{
				try
				{
					@delegate(message, ex);
				}
				catch (Exception handleEx)
				{
					_log.WarnFormat(handleEx, "Failed to handle message processing failure");
				}
			}

			// do nothing. Message will show up in the queue with the increased enqueue count.
		}

		static void FinalizeSuccess(IReadMessageQueue queue, UnpackedMessage message, TransactionScope tx)
		{
			queue.AckMessage(message);
			tx.Complete();
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

		QueueProcessingResult ProcessQueueForMessage(IReadMessageQueue queue)
		{
			var transactionOptions = GetTransactionOptions();
			try
			{
				using (var tx = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
				{
					var result = queue.GetMessage();

					switch (result.State)
					{
						case GetMessageResultState.Success:
							GetProcessingFailure(queue, result.Message)
								.Apply(ex => MessageHandlingProblem(result.Message, ex))
								.Handle(() => FinalizeSuccess(queue, result.Message, tx));
							return QueueProcessingResult.MoreWork;

						case GetMessageResultState.Wait:
							return QueueProcessingResult.Sleep;

						case GetMessageResultState.Exception:
							_log.DebugFormat(result.Exception, "Exception, while trying to get message");
							return QueueProcessingResult.MoreWork;

						case GetMessageResultState.Retry:
							tx.Complete();
							// retry immediately
							return QueueProcessingResult.MoreWork;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (TransactionAbortedException ex)
			{
				_log.Error(ex, "Aborting transaction");
				// do nothing);
				return QueueProcessingResult.MoreWork;
			}
		}

		TransactionOptions GetTransactionOptions()
		{
			return new TransactionOptions
				{
					IsolationLevel = Transaction.Current == null ? _isolationLevel : Transaction.Current.IsolationLevel,
					Timeout = Debugger.IsAttached ? 45.Minutes() : 0.Minutes(),
				};
		}

		public override string ToString()
		{
			return string.Format("Queue x {1} ({0})", _queueNames.Join(", "), _degreeOfParallelism);
		}

		enum QueueProcessingResult
		{
			MoreWork,
			Sleep
		}
	}
}