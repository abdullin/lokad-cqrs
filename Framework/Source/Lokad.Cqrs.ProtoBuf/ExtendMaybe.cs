using System;

namespace Lokad.Cqrs.ProtoBuf
{
	public static class ExtendMaybe
	{
		public static Maybe<TValue> GetValue<TValue>(this Maybe<TValue> value, Func<Maybe<TValue>> replacement)
		{
			if (value.HasValue)
				return value;
			return replacement();
		}
	}
}