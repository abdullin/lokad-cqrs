using System;

namespace Lokad.Cqrs
{
	public static class MessageContext
	{
		[ThreadStatic] static UnpackedMessage _current;

		public static UnpackedMessage Current { get { return _current; } }

		internal static void OverrideContext(UnpackedMessage message)
		{
			_current = message;
		}
	}
}