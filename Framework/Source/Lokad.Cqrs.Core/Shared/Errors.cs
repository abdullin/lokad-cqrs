#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Globalization;
using System.Reflection;
using Lokad.Quality;


namespace Lokad
{
	/// <summary>
	/// Helper class for generating exceptions
	/// </summary>
	[NoCodeCoverage]
	[UsedImplicitly]
	public class Errors
	{
	

		/// <summary>
		/// Creates new instance of <see cref="InvalidOperationException"/>
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments of the format string.</param>
		/// <returns>new exception instance</returns>
		[NotNull, StringFormatMethod("message")]
		public static Exception InvalidOperation([NotNull] string message, params object[] args)
		{
			return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, message, args));
		}


		/// <summary>
		/// Creates new instance of <see cref="NotSupportedException"/>
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments of the format string.</param>
		/// <returns>new exception instance</returns>
		[NotNull, StringFormatMethod("message")]
		public static Exception NotSupported([NotNull] string message, params object[] args)
		{
			return new NotSupportedException(string.Format(CultureInfo.InvariantCulture, message, args));
		}

	

		static readonly MethodInfo InternalPreserveStackTraceMethod;

		static Errors()
		{
			InternalPreserveStackTraceMethod = typeof(Exception).GetMethod("InternalPreserveStackTrace",
				BindingFlags.Instance | BindingFlags.NonPublic);
		}

		/// <summary>
		/// Returns inner exception, while preserving the stack trace
		/// </summary>
		/// <param name="e">The target invocation exception to unwrap.</param>
		/// <returns>inner exception</returns>
		[NotNull, UsedImplicitly]
		public static Exception Inner([NotNull] TargetInvocationException e)
		{
			if (e == null) throw new ArgumentNullException("e");
			InternalPreserveStackTraceMethod.Invoke(e.InnerException, new object[0]);
			return e.InnerException;
		}
	}
}