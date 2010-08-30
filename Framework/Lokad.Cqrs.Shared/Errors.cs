#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Globalization;
using System.Reflection;
using Lokad.Quality;
using Lokad.Reflection;

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
		/// Creates new instance of <see cref="KeyInvalidException"/>
		/// </summary>
		/// <returns>new exception instance</returns>
		[NotNull]
		public static Exception KeyInvalid()
		{
			var s = string.Format(CultureInfo.InvariantCulture, "Key has invalid value");
			return new KeyInvalidException(s);
		}

		/// <summary>
		/// Creates new instance of <see cref="KeyInvalidException"/>
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>new exception instance</returns>
		[NotNull]
		public static Exception KeyInvalid(object value)
		{
			var s = string.Format(CultureInfo.InvariantCulture, "Key has invalid value '{0}'", value);
			return new KeyInvalidException(s);
		}

		/// <summary>
		/// Creates new instance of the <see cref="ResolutionException"/>
		/// </summary>
		/// <param name="valueType">Type of the service.</param>
		/// <param name="key">The service key.</param>
		/// <param name="inner">The inner.</param>
		/// <returns>new exception instance</returns>
		[NotNull]
		public static Exception Resolution([NotNull] Type valueType, object key, [NotNull] Exception inner)
		{
			var s = string.Format(CultureInfo.InvariantCulture, "Error while resolving {0} with key '{1}'", valueType, key);
			return new ResolutionException(s, inner);
		}

		/// <summary>
		/// Creates new instance of the <see cref="ResolutionException"/>
		/// </summary>
		/// <param name="valueType">Type of the service.</param>
		/// <param name="inner">The inner.</param>
		/// <returns>new exception instance</returns>
		[NotNull]
		public static Exception Resolution([NotNull] Type valueType, [NotNull] Exception inner)
		{
			var s = string.Format(CultureInfo.InvariantCulture, "Error while resolving {0}", valueType);
			return new ResolutionException(s, inner);
		}

		/// <summary>
		/// Creates new instance of <see cref="KeyInvalidException"/>
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="inner">The inner.</param>
		/// <returns>new exception instance</returns>
		[NotNull]
		public static Exception KeyInvalid(object value, [NotNull] Exception inner)
		{
			var s = string.Format(CultureInfo.InvariantCulture, "Key has invalid value '{0}'", value);
			return new KeyInvalidException(s, inner);
		}


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
		

		[NotNull]
		internal static Exception ArgumentNullOrEmpty([InvokerParameterName] string paramName)
		{
			return new ArgumentException("Provided string can't be null or empty", paramName);
		}

		[NotNull]
		internal static Exception InvalidOperation<T>([NotNull] string message, [NotNull] Func<T> variableReference)
		{
			return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, message, Reflect.VariableName(variableReference)));
		}

		[NotNull]
		internal static Exception ArgumentNull<T>([NotNull] Func<T> argumentReference)
		{
			var message = StringUtil.FormatInvariant("Parameter of type '{0}' can't be null", typeof (T));
			var paramName = Reflect.VariableName(argumentReference);
			return new ArgumentNullException(paramName, message);
		}

		[NotNull]
		internal static Exception Argument<T>([NotNull] Func<T> argumentReference, [NotNull] string message)
		{
			var paramName = Reflect.VariableName(argumentReference);
			return new ArgumentException(message, paramName);
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