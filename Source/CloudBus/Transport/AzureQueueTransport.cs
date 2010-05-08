using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Transactions;
using Bus2.Queue;
using Lokad;
using Lokad.Quality;

namespace Bus2.Transport
{
	[UsedImplicitly]
	public sealed class AzureQueueTransport : IMessageTransport
	{
		readonly IQueueManager _factory;
		readonly int _threadCount;
		readonly Thread[] _threads;
		readonly ILog _log;
		readonly IsolationLevel _isolationLevel;
		readonly IBusProfiler _profiler;

		bool _haveStarted;
		volatile bool _shouldContinue;

		public event Action Started = () => { };
		public event Func<IncomingMessage, bool> MessageRecieved = m => false;

		readonly string[] _queueNames;
		readonly IReadMessageQueue[] _queues;
		readonly TimeSpan _threadSleepInterval;

		public AzureQueueTransport(
			AzureQueueTransportConfig config,
			ILogProvider logProvider,
			IQueueManager factory, 
			IBusProfiler profiler)
		{
			_factory = factory;
			_profiler = profiler;
			// TODO add queue selection policy

			_queueNames = config.QueueNames;
			_isolationLevel = config.IsolationLevel;
			_log = logProvider.CreateLog<AzureQueueTransport>();
			_threadCount = config.ThreadCount;
			_threadSleepInterval = config.SleepWhenNoMessages;

			_threads = new Thread[config.ThreadCount];
			_queues = new IReadMessageQueue[_queueNames.Length];
		}

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
						Name = "Lokad Service Bus Worker Thread #" + i,
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
				_log.Error(ex, "Failed to consume");
				return ex;
			}

			if (!consumed)
			{
				try
				{
					_log.DebugFormat("Discarding message {0} (no consumers)", message.TransportMessageId);
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

			using (_profiler.TrackContext(message.Message))
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


		void ProcessingProblem(Exception ex, IncomingMessage message)
		{
			// do nothing. Message will show up in the queue with the increased enqueue count.
		}

		void FinalizeSuccess(IReadMessageQueue queue, IncomingMessage message, TransactionScope tx)
		{
			queue.AckMessage(message);
			tx.Complete();
		}

		public void ReceiveMessage()
		{
			var thisThreadSleeps = _threadSleepInterval + Rand.Next(200).Milliseconds();
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
				if ((false == threadsWorked) && (false == _shouldContinue))
				{
					SystemUtil.Sleep(thisThreadSleeps);
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
							Process(queue, result.Message)
								.Handle(ex => ProcessingProblem(ex, result.Message))
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
			catch(TransactionAbortedException ex)
			{

				_log.Error(ex, "Aborting transaction");
				// do nothing);
				return QueueProcessingResult.MoreWork;
			}
		}

		enum QueueProcessingResult
		{
			MoreWork,
			Sleep
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
	}
}