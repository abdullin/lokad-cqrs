using System;
using System.Linq;
using ExtendIEnumerable = Lokad.ExtendIEnumerable;

namespace Lokad.Serialization
{
	sealed class AttributeHelper
	{
		readonly object[] _attributes;

		public AttributeHelper(object[] attributes)
		{
			_attributes = attributes;
		}

		public Maybe<string> GetString<TAttribute>(Func<TAttribute, string> retriever)
			where TAttribute : Attribute
		{
			var v = ExtendIEnumerable.FirstOrEmpty<TAttribute>(_attributes
					.OfType<TAttribute>())
				.Convert(retriever, "");

			if (string.IsNullOrEmpty(v))
				return Maybe<string>.Empty;

			return v;
		}
	}
}