using System;

namespace Bus2
{
	public interface IBusProcess : IDisposable
	{
		void Start();
	}
}