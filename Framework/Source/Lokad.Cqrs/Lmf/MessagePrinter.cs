#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Linq;
using System.Text;

namespace Lokad.Cqrs
{
	public static class MessagePrinter
	{
		public static string PrintAttributes(MessageAttributes attributes, string indent = "")
		{
			var builder = new StringBuilder();
			var max = attributes.Items.Max(a => a.GetName().Length);

			foreach (var item in attributes.Items)
			{
				builder
					.AppendFormat("{2}{0,-" + (max + 2) + "}: {1}", item.GetName(), item.GetValue(), indent)
					.AppendLine();
			}
			return builder.ToString();
		}
	}
}