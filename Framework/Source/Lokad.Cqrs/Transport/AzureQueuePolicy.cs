#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Transport
{
	public static class AzureQueuePolicy
	{
		public static Func<uint, TimeSpan> BuildDecayPolicy(TimeSpan maxDecay)
		{
			//var seconds = (Rand.Next(0, 1000) / 10000d).Seconds();
			var seconds = maxDecay.TotalSeconds;
			return l =>
				{
					if (l >= 31)
					{
						return maxDecay;
					}

					var foo = Math.Pow(2, (l - 1)/5.0)/64d*seconds;
					return foo.Seconds();
				};
		}
	}
}