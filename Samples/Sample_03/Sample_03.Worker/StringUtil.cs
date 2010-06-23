using System;

namespace Sample_03.Worker
{
	static class StringUtil
	{
		public static string ToReadable(this Guid guid)
		{
			return guid.ToString().Substring(0, 6);
		}
	}
}