#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Globalization;
using System.Text;

namespace Lokad
{
	/// <summary>
	/// Helper methods for <see cref="string"/>
	/// </summary>
	public static class StringUtil
	{
		/// <summary>
		/// Formats the string using InvariantCulture
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="args">The args.</param>
		/// <returns>formatted string</returns>
		public static string FormatInvariant(string format, params object[] args)
		{
			return string.Format(CultureInfo.InvariantCulture, format, args);
		}

		/// <summary>
		/// Converts "Class.SomeName" to "Class - Some Name"
		/// </summary>
		public static string MemberNameToCaption([NotNull] string source)
		{
			if (source == null) throw new ArgumentNullException("source");
			var cleanedSource = source.Replace(".", " - ");
			var sb = new StringBuilder();
			bool lastWasUpper = false;
			bool lastWasEmpty = true;

			foreach (var c in cleanedSource)
			{
				if (char.IsUpper(c) && !lastWasUpper && !lastWasEmpty)
				{
					sb.Append(' ');
				}
				lastWasUpper = char.IsUpper(c);
				lastWasEmpty = char.IsWhiteSpace(c);
				sb.Append(c);
			}
			return sb.ToString();
		}
	}
}