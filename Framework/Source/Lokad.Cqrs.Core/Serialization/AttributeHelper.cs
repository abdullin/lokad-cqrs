using System;
using System.Linq;

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
			var v = _attributes
				.OfType<TAttribute>()
				.FirstOrEmpty()
				.Convert(retriever, "");

			if (string.IsNullOrEmpty(v))
				return Maybe<string>.Empty;

			return v;
		}
	}
}