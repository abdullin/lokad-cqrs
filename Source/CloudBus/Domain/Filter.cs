using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudBus.Domain
{
	public sealed class Filter<TItem>
	{
		readonly HashSet<Func<TItem, bool>> _filters = new HashSet<Func<TItem, bool>>();

		public bool Any()
		{
			return _filters.Any();
		}

		public Filter<TItem> Where(Func<TItem, bool> filter)
		{
			_filters.Add(filter);
			return this;
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