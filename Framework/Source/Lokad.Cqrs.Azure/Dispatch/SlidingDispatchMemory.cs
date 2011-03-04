using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Lokad.Cqrs.Dispatch
{
	///<summary>
	/// Shoud be registered as singleton
	///</summary>
	public sealed class SlidingDispatchMemory : IEngineProcess
	{
		readonly ConcurrentDictionary<ISingleThreadMessageDispatcher, DispatchMemory> _memories = new ConcurrentDictionary<ISingleThreadMessageDispatcher, DispatchMemory>();
		
		public void Dispose()
		{
		}

		public void Initialize()
		{
		}

		public sealed class DispatchMemory 
		{
			readonly ConcurrentDictionary<string,DateTime> _memory = new ConcurrentDictionary<string, DateTime>();

			public void RegisterMemory(string memoryId)
			{
				_memory.TryAdd(memoryId, DateTime.UtcNow);
			}

			public bool DoWeRemember(string memoryId)
			{
				DateTime value;
				return _memory.TryGetValue(memoryId, out value);
			}

			public void ForgetOlderThan(TimeSpan older)
			{
				// suboptimal for now
				var deleteBefore = DateTime.UtcNow - older;
				var keys = _memory.ToArray()
					.Where(p => p.Value < deleteBefore)
					.Select(v => v.Key);

				foreach (var key in keys)
				{
					DateTime deleted;
					_memory.TryRemove(key, out deleted);
				}
			}

		}

		public DispatchMemory AcquireMemory(ISingleThreadMessageDispatcher dispatcher)
		{
			return _memories.GetOrAdd(dispatcher, s => new DispatchMemory());
		}

		public Task Start(CancellationToken token)
		{
			return Task.Factory.StartNew(() =>
				{
					while (!token.IsCancellationRequested)
					{
						
						foreach (var memory in _memories)
						{
							memory.Value.ForgetOlderThan(TimeSpan.FromMinutes(20));
						}

						token.WaitHandle.WaitOne(TimeSpan.FromMinutes(5));
					}

				}, token);
		}
	}
}