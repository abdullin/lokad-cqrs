using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs
{
	public sealed class Filter<TItem>
	{
		readonly HashSet<Func<TItem, bool>> _filters = new HashSet<Func<TItem, bool>>();

		public void AddFilter(Func<TItem, bool> filter)
		{
			_filters.Add(filter);
		}

		public IEnumerable<TItem> Apply(IEnumerable<TItem> types)
		{
			var func = BuildFilter();
			return types.Where(func);
		}

		public Func<TItem, bool> BuildFilter()
		{
			if (_filters.Count == 0)
				return item => true;

			return item => _filters.All(func => func(item));
		}
	}
}