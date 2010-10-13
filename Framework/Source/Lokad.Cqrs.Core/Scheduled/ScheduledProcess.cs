#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Lokad.Cqrs.Scheduled
{
	[UsedImplicitly]
	public sealed class ScheduledProcess : IEngineProcess
	{
		public delegate void ChainProcessedDelegate(ScheduledState[] state, bool emptyChain);

		readonly ILog _log;
		readonly IEngineProfiler _profiler;
		readonly TimeSpan _sleepBetweenCommands;
		readonly TimeSpan _sleepOnEmptyChain;
		readonly TimeSpan _sleepOnFailure;
		readonly ScheduledState[] _tasks;
		readonly IsolationLevel _isolationLevel;
		readonly IScheduledTaskDispatcher _dispatcher;

		readonly CancellationTokenSource _disposal = new CancellationTokenSource();

		public ScheduledProcess(
			ILogProvider provider,
			ScheduledTaskInfo[] commands,
			ScheduledConfig config,
			IEngineProfiler profiler,
			IScheduledTaskDispatcher dispatcher)
		{
			_log = provider.CreateLog<ScheduledProcess>();

			_tasks = commands.ToArray(c => new ScheduledState(c.Name, c));
			_sleepBetweenCommands = config.SleepBetweenCommands;
			_sleepOnEmptyChain = config.SleepOnEmptyChain;
			_sleepOnFailure = config.SleepOnFailure;
			_isolationLevel = config.IsolationLevel;
			_profiler = profiler;
			_dispatcher = dispatcher;
		}

		public void Dispose()
		{
			_disposal.Cancel(true);
			_disposal.Dispose();
		}

		public void Initialize()
		{
		}


		internal event ChainProcessedDelegate ChainProcessed = (states, e) => { };
		internal event Action<Exception> ExceptionEncountered = ex => { };

		readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(false);

		public Task Start(CancellationToken outer)
		{
			return Task.Factory.StartNew(() =>
				{
					_stopped.Reset();
					using (var source = CancellationTokenSource.CreateLinkedTokenSource(outer, _disposal.Token))
					{
						RunTillCancelled(source.Token);
					}
				}, outer).ContinueWith(t => _stopped.Reset());
		}

		void RunTillCancelled(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var executed = false;

				foreach (var command in _tasks)
				{
					// do we need to run this command now?
					if (command.NextRun <= SystemUtil.UtcNow)
					{
						_log.DebugFormat("Executing {0}", command.Name);
						executed = true;
						RunCommand(command, token);
					}
				}

				ChainProcessed(_tasks, executed);
				token.WaitHandle.WaitOne(executed ? _sleepBetweenCommands : _sleepOnEmptyChain);
			}
		}

		public bool WaitOne(TimeSpan timeout)
		{
			return _stopped.Wait(timeout);
		}

		TransactionOptions GetTransactionOptions()
		{
			return new TransactionOptions
				{
					IsolationLevel = Transaction.Current == null ? _isolationLevel : Transaction.Current.IsolationLevel,
					Timeout = Debugger.IsAttached ? 45.Minutes() : 0.Minutes(),
				};
		}

		void RunCommandTillItFinishes(ScheduledState state, CancellationToken token)
		{
			using (_profiler.TrackContext(state.Name))
			{
				while (!token.IsCancellationRequested)
				{
					var transactionOptions = GetTransactionOptions();
					TimeSpan result;
					using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
					{
						result = _dispatcher.Execute(state.Task);
						scope.Complete();
					}

					if (result > TimeSpan.Zero)
					{
						state.ScheduleIn(result);
						return;
					}

					_log.Debug("Repeat");
					state.ScheduleIn(0.Seconds());
				}
			}
		}


		void RunCommand(ScheduledState state, CancellationToken token)
		{
			try
			{
				RunCommandTillItFinishes(state, token);
			}
			catch (Exception ex)
			{
				ExceptionEncountered(ex);
				_log.ErrorFormat(ex, "Exception while processing {0}", state.Name);

				state.ScheduleIn(_sleepOnFailure);
			}
		}
	}
}