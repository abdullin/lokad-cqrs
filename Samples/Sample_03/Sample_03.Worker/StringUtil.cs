#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

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