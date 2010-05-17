#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lokad;
using Lokad.Quality;

namespace CloudBus.Scheduled
{
	[UsedImplicitly]
	public sealed class ScheduledProcess : IBusProcess
	{
		public delegate void ChainProcessedDelegate(ScheduledState[] state, bool emptyChain);

		readonly ILog _log;
		readonly IBusProfiler _profiler;
		readonly TimeSpan _sleepBetweenCommands;
		readonly TimeSpan _sleepOnEmptyChain;
		readonly TimeSpan _sleepOnFailure;
		readonly ScheduledState[] _tasks;

		Thread[] _controlThreads = new Thread[0];
		bool _haveStarted;
		volatile bool _shouldContinue;


		public ScheduledProcess(
			ILogProvider provider,
			IEnumerable<ScheduledInfo> commands,
			ScheduledConfig config, IBusProfiler profiler)
		{
			_log = provider.CreateLog<ScheduledProcess>();

			_tasks = commands.ToArray(c => new ScheduledState(c.Name, c.Delegate));
			_sleepBetweenCommands = config.SleepBetweenCommands;
			_sleepOnEmptyChain = config.SleepOnEmptyChain;
			_sleepOnFailure = config.SleepOnFailure;
			_profiler = profiler;
		}

		public void Dispose()
		{
			_shouldContinue = false;

			if (_haveStarted)
				return;
		}

		public void Start()
		{
			_shouldContinue = true;
			_controlThreads = new[]
				{
					new Thread(MainMethod)
						{
							Name = "Task",
							IsBackground = true
						},
				};
			_log.DebugFormat("Starting {0} tasks in {1} threads", _tasks.Length, _controlThreads.Length);

			foreach (var thread in _controlThreads)
			{
				thread.Start();
			}
			
			_haveStarted = true;
		}

		internal event ChainProcessedDelegate ChainProcessed = (states, e) => { };
		internal event Action<Exception> ExceptionEncountered = ex => { };

		void MainMethod()
		{
			while (_shouldContinue)
			{
				bool executed = false;
				foreach (var command in _tasks)
				{
					// do we need to run this command now?
					if (command.NextRun <= SystemUtil.UtcNow)
					{
						_log.DebugFormat("Executing {0}", command.Name);
						executed = true;
						RunCommand(command);
					}
				}

				ChainProcessed(_tasks, executed);
				SleepWhileCan(executed ? _sleepBetweenCommands : _sleepOnEmptyChain);
			}
		}

		void RunCommandTillItFinishes(ScheduledState state)
		{
			using (_profiler.TrackContext(state.Name))
			{
				while (_shouldContinue)
				{
					var result = state.Happen();

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


		void SleepWhileCan(TimeSpan span)
		{
			var seconds = span.TotalSeconds;
			var wholeSeconds = (int) Math.Floor(seconds);

			for (int i = 0; i < wholeSeconds; i++)
			{
				if (false == _shouldContinue)
				{
					return;
				}
				SystemUtil.Sleep(1.Seconds());
			}
			var reminder = seconds - wholeSeconds;

			if (false == _shouldContinue)
			{
				return;
			}

			if (reminder > 0)
			{
				SystemUtil.Sleep(reminder.Seconds());
			}
		}

		void RunCommand(ScheduledState state)
		{
			try
			{
				RunCommandTillItFinishes(state);
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