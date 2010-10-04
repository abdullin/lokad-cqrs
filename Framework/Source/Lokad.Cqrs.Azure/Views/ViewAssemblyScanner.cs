using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lokad.Cqrs.Domain
{
	public sealed class ViewAssemblyScanner
	{
		readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
		readonly Filter<Type> _serializableSelector = new Filter<Type>();

		public ViewAssemblyScanner WithAssemblyOf<T>()
		{
			_assemblies.Add(typeof(T).Assembly);
			return this;
		}

		public ViewAssemblyScanner WhereEntities(Func<Type, bool> filter)
		{
			_serializableSelector.Where(filter);
			return this;
		}

		public ViewAssemblyScanner WithAssembly(Assembly assembly)
		{
			_assemblies.Add(assembly);
			return this;
		}

		public IEnumerable<Type> Build()
		{
			if (!_assemblies.Any())
				throw new InvalidOperationException("There are no assemblies to scan");

			var types = _assemblies
				.SelectMany(a => a.GetExportedTypes())
				.ToList();

			return _serializableSelector
				.Apply(types)
				.ToArray();
		}
	}
}