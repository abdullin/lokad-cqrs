#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using System.Text;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Helper extensions for the <see cref="IStorageItem"/>
	/// </summary>
	public static class ExtendStorageItem
	{
		public static void WriteText(this IStorageItem item, string text)
		{
			item.Write(s =>
				{
					using (var writer = new StreamWriter(s))
					{
						writer.Write(text);
					}
				});
		}

		public static void WriteText(this IStorageItem item, string text, Encoding encoding)
		{
			item.Write(s =>
				{
					using (var writer = new StreamWriter(s, encoding))
					{
						writer.Write(text);
					}
				});
		}

		public static string ReadText(this IStorageItem item)
		{
			string result = null;
			item.ReadInto((props, stream) =>
				{
					using (var reader = new StreamReader(stream))
					{
						result = reader.ReadToEnd();
					}
				});

			return result;
		}
	}
}