using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Bus2.Profiling
{
	public static class ProfilerUtil
	{
		static readonly Hashtable<Type, Func<object, string>> Type2ToString = new Hashtable<Type, Func<object, string>>();


		public static string ToString(object msg)
		{
			Func<object, string> val = null;
			var type = msg.GetType();
			Type2ToString.Read(reader => reader.TryGetValue(type, out val));
			if (val != null)
				return val(msg);

			Type2ToString.Write(writer =>
				{
					bool overrideToString = type.GetMethod("ToString").DeclaringType != typeof(object);
					if (overrideToString)
						val = o => o.ToString();
					else
						val = o => type.Name;
					writer.Add(type, val);
				});

			return val(msg);
		}


		class Hashtable<TKey, TVal> : IEnumerable<KeyValuePair<TKey, TVal>>
		{
			readonly Dictionary<TKey, TVal> _dictionary = new Dictionary<TKey, TVal>();
			readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

			#region IEnumerable<KeyValuePair<TKey,TVal>> Members

			public IEnumerator<KeyValuePair<TKey, TVal>> GetEnumerator()
			{
				_readerWriterLockSlim.EnterReadLock();
				try
				{
					return _dictionary
						.ToList()
						.GetEnumerator();
				}
				finally
				{
					_readerWriterLockSlim.ExitReadLock();
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			#endregion

			public void Write(Action<Writer> action)
			{
				_readerWriterLockSlim.EnterWriteLock();
				try
				{
					action(new Writer(this));
				}
				finally
				{
					_readerWriterLockSlim.ExitWriteLock();
				}
			}

			public void Read(Action<Reader> read)
			{
				_readerWriterLockSlim.EnterReadLock();
				try
				{
					read(new Reader(this));
				}
				finally
				{
					_readerWriterLockSlim.ExitReadLock();
				}
			}



			public class Reader
			{
				protected Hashtable<TKey, TVal> parent;

				public Reader(Hashtable<TKey, TVal> parent)
				{
					this.parent = parent;
				}

				public bool TryGetValue(TKey key, out TVal val)
				{
					return parent._dictionary.TryGetValue(key, out val);
				}
			}



			public class Writer : Reader
			{
				public Writer(Hashtable<TKey, TVal> parent)
					: base(parent)
				{
				}

				public void Add(TKey key, TVal val)
				{
					parent._dictionary[key] = val;
				}

				public bool Remove(TKey key)
				{
					return parent._dictionary.Remove(key);
				}
			}
		}

	}
}
