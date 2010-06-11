#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Transactions;
using Lokad.Cqrs.Queue;
using Lokad.Quality;

namespace Lokad.Cqrs.Transport
{
	[UsedImplicitly]
	public sealed class AzureQueueTransport : IMessageTransport
	{
		readonly IQueueManager _factory;
		readonly IsolationLevel _isolationLevel;
		readonly ILog _log;
		readonly IMessageProfiler _messageProfiler;
		readonly IEngineProfiler _profiler;
		readonly string[] _queueNames;
		readonly IReadMessageQueue[] _queues;
		readonly int _threadCount;
		readonly Func<uint, TimeSpan> _threadSleepInterval;
		readonly Thread[] _threads;

		bool _haveStarted;
		volatile bool _shouldContinue;

		public AzureQueueTransport(
			AzureQueueTransportConfig config,
			ILogProvider logProvider,
			IQueueManager factory,
			IEngineProfiler profiler,
			IMessageProfiler messageProfiler)
		{
			_factory = factory;
			_messageProfiler = messageProfiler;
			_profiler = profiler;
			_queueNames = config.QueueNames;
			_isolationLevel = config.IsolationLevel;
			_log = logProvider.Get(typeof (AzureQueueTransport).Name + "." + config.LogName);
			_threadCount = config.ThreadCount;
			_threadSleepInterval = config.SleepWhenNoMessages;

			_threads = new Thread[config.ThreadCount];
			_queues = new IReadMessageQueue[_queueNames.Length];
		}

		public event Action Started = () => { };
		public event Func<IncomingMessage, bool> MessageRecieved = m => false;
		public event Action<IncomingMessage, Exception> MessageHandlerFailed = (message, exception) => { };

		public void Dispose()
		{
			_shouldContinue = false;
			_log.DebugFormat("Stopping transport for {0}", _queueNames.Join(";"));


			//DisposeQueueManager();

			if (!_haveStarted)
				return;

			foreach (var thread in _threads)
			{
				thread.Join();
			}
		}

		public int ThreadCount
		{
			get { return _threadCount; }
		}

		public void Start()
		{
			_shouldContinue = true;

			_log.DebugFormat("Starting {0} threads to handle messages on {1}",
				_threadCount,
				_queueNames.Select(q => q.ToString()).Join("; "));

			for (int i = 0; i < _queueNames.Length; i++)
			{
				_queues[i] = _factory.GetReadQueue(_queueNames[i]);
			}


			for (var i = 0; i < _threadCount; i++)
			{
				_threads[i] = new Thread(ReceiveMessage)
					{
						Name = "Thread #" + i,
						IsBackground = true
					};
				_threads[i].Start();
			}
			_haveStarted = true;

			Started();
		}


		Result<IncomingMessage, Exception> Process(IReadMessageQueue queue, IncomingMessage message)
		{
			bool consumed;

			try
			{
				consumed = ProcessSingleMessage(message, MessageRecieved);
			}
			catch (Exception ex)
			{
				var info = _messageProfiler.GetReadableMessageInfo(message.Message, message.TransportMessageId);
				var text = string.Format("Failed to consume '{0}' from '{1}'. TransportId: {2}", info, queue.Uri,
					message.TransportMessageId);

				_log.Error(ex, text);
				return ex;
			}

			if (!consumed)
			{
				try
				{
					Discard(queue, message);
				}
				catch (Exception ex)
				{
					_log.ErrorFormat(ex, "Failed to discard the message {0}", message.TransportMessageId);
				}
			}

			return message;
		}

		bool ProcessSingleMessage(IncomingMessage message, Func<IncomingMessage, bool> messageRecieved)
		{
			if (messageRecieved == null)
				return false;

			using (_profiler.TrackMessage(message.Message, message.TransportMessageId))
			{
				foreach (Func<IncomingMessage, bool> func in messageRecieved.GetInvocationList())
				{
					if (func(message))
					{
						return true;
					}
				}
			}
			return false;
		}

		void Discard(IReadMessageQueue queue, IncomingMessage detail)
		{
			_log.DebugFormat("Discarding message {0} ({1}) because there are no consumers for it.",
				detail.Message, detail.TransportMessageId);

			queue.DiscardMessage(detail);
		}


		void MessageHandlingProblem(IncomingMessage message, Exception ex)
		{
			MessageHandlerFailed(message, ex);
			// do nothing. Message will show up in the queue with the increased enqueue count.
		}

		void FinalizeSuccess(IReadMessageQueue queue, IncomingMessage message, TransactionScope tx)
		{
			queue.AckMessage(message);
			tx.Complete();
		}

		public void ReceiveMessage()
		{
			//var thisThreadSleeps = _threadSleepInterval + Rand.Next(200).Milliseconds();
			uint idleFor = 0;
			while (_shouldContinue)
			{
				bool threadsWorked = false;
				foreach (var messageQueue in _queues)
				{
					if (_shouldContinue == false)
						return;

					// selector policy goes here
					if (ProcessQueueForMessage(messageQueue) == QueueProcessingResult.MoreWork)
					{
						threadsWorked = true;
					}
				}

				if (threadsWorked)
				{
					idleFor = 0;
				}
				else
				{
					idleFor += 1;
					SleepFor(_threadSleepInterval(idleFor));
				}
			}
		}

		void SleepFor(TimeSpan span)
		{
			var secondsToSleep = span;
			var precision = 0.1.Seconds();

			while ((_shouldContinue) && (secondsToSleep >= precision))
			{
				SystemUtil.Sleep(precision);
				secondsToSleep -= precision;
			}
			if ((_shouldContinue) && (secondsToSleep > TimeSpan.Zero))
			{
				SystemUtil.Sleep(secondsToSleep);
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
							Process(queue, result.Message)
								.Handle(ex => MessageHandlingProblem(result.Message, ex))
								.Apply(m => FinalizeSuccess(queue, m, tx));
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
			return string.Format("Queue x {1} ({0})", _queueNames.Join(", "), ThreadCount);
		}

		enum QueueProcessingResult
		{
			MoreWork,
			Sleep
		}
	}
}