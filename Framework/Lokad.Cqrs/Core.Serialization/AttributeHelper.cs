using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Core.Serialization
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
			var v = FirstOrEmpty(_attributes
					.OfType<TAttribute>())
				.Convert(retriever, "");

			if (String.IsNullOrEmpty(v))
				return Maybe<string>.Empty;

			return v;
		}

		static Maybe<TSource> FirstOrEmpty<TSource>(IEnumerable<TSource> sequence)
		{
			foreach (var source in sequence)
			{
				return source;
			}
			return Maybe<TSource>.Empty;
		}
	}
}