using System.Runtime.Serialization;

namespace Lokad.Cqrs.ProtoBuf.Tests
{
	static class FormatterCache<T>
	{
		static IFormatter _formatter;

		public static IFormatter Get()
		{
			if (null == _formatter)
			{
				_formatter = ProtoBufUtil.CreateFormatter(typeof (T));
			}
			return _formatter;
		}
	}
}