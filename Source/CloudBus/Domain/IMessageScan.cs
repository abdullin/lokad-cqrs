using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudBus.Domain
{
	public interface IMessageScan
	{
		IEnumerable<DomainMessageMapping> GetMappings();

		IMessageDirectory BuildDirectory(Func<DomainMessageMapping, bool> filter);
	}

	public static class ExtendIMessageScan
	{
		public static IMessageDirectory BuildDirectory(this IMessageScan scan)
		{
			return scan.BuildDirectory(m => true);
		}

		public static IMessageDirectory BuildDirectory(this IMessageScan scan, IEnumerable<Func<DomainMessageMapping, bool>> filter)
		{
			var cached = filter.ToArray();
			return scan.BuildDirectory(m =>cached.Any(f => f(m)));
		}
	}
}