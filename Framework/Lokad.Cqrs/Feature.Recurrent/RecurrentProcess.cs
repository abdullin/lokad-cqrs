#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Feature.Recurrent.Events;

namespace Lokad.Cqrs.Feature.Recurrent
{
	
	public sealed class RecurrentProcess : IEngineProcess
	{
		public delegate void ChainProcessedDelegate(RecurrentTaskState[] state, bool emptyChain);

		readonly ISystemObserver _observer;
		readonly TimeSpan _sleepBetweenCommands;
		readonly TimeSpan _sleepOnEmptyChain;
		readonly TimeSpan _sleepOnFailure;
		readonly RecurrentTaskState[] _tasks;
		
		readonly IRecurrentTaskDispatcher _dispatcher;

		readonly CancellationTokenSource _disposal = new CancellationTokenSource();

		public RecurrentProcess(
			ISystemObserver observer,
			RecurrentTaskInfo[] commands,
			RecurrentConfig config,
			IRecurrentTaskDispatcher dispatcher)
		{
			_observer = observer;

			_tasks = commands.Select(c => new RecurrentTaskState(c.Name, c)).ToArray();
			_sleepBetweenCommands = config.SleepBetweenCommands;
			_sleepOnEmptyChain = config.SleepOnEmptyChain;
			_sleepOnFailure = config.SleepOnFailure;
			_dispatcher = dispatcher;
		}

		public void Dispose()
		{
			_disposal.Cancel(true);
			_disposal.Dispose();
		}

		public void Initialize()
		{
			_observer.Notify(new ScheduledProcessInitialized(_tasks.Length));
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
					if (command.NextRun <= DateTime.UtcNow)
					{
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

		

		void RunCommandTillItFinishes(RecurrentTaskState state, CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var result = _dispatcher.Execute(state.Task);

				if (result > TimeSpan.Zero)
				{
					state.ScheduleIn(result);
					return;
				}
				state.ScheduleIn(0.Seconds());
			}
		}


		void RunCommand(RecurrentTaskState state, CancellationToken token)
		{
			try
			{
				RunCommandTillItFinishes(state, token);
			}
			catch (Exception ex)
			{
				_observer.Notify(new FailedToExecuteScheduledTask(ex, state.Name));
				ExceptionEncountered(ex);
				state.ScheduleIn(_sleepOnFailure);
			}
		}
	}
}