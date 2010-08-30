using System;

namespace Lokad
{
	/// <summary>
	/// Log provider, that uses lambda expression
	/// </summary>
	public sealed class LambdaLogProvider : ILogProvider
	{
		readonly Func<string, ILog> _factory;

		/// <summary>
		/// Initializes a new instance of the <see cref="LambdaLogProvider"/> class.
		/// </summary>
		/// <param name="factory">The factory.</param>
		public LambdaLogProvider(Func<string, ILog> factory)
		{
			_factory = factory;
		}

		ILog IProvider<string, ILog>.Get(string key)
		{
			return _factory(key);
		}
	}
}