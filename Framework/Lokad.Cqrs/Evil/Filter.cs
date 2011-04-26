#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Evil
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