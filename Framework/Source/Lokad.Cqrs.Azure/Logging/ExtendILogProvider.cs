#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;

namespace Lokad.Cqrs.Logging
{
	/// <summary>
	/// Extension methods for the <see cref="ILogProvider"/>
	/// </summary>
	
	public static class ExtendILogProvider
	{
		/// <summary>
		/// Creates new log using the type as name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		[Obsolete("Obsolete, Use LogForType overrides to specify log name precisely")]
		public static ILog CreateLog<T>(this ILogProvider logProvider) where T : class
		{
			return logProvider.Get(typeof (T).Name);
		}

		/// <summary>
		/// Creates new log with named with the class name.
		/// </summary>
		/// <param name="logProvider">The log provider.</param>
		/// <param name="instance">The instance to retrieve type for naming from.</param>
		/// <returns>new instance of the log</returns>
		public static ILog LogForName(this ILogProvider logProvider, object instance)
		{
			return logProvider.Get(instance.GetType().Name);
		}
	}
}