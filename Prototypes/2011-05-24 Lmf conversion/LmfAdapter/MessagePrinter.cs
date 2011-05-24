#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.IO;

namespace LmfAdapter
{
	public static class MessagePrinter
	{
		/// <summary>
		/// Nicely prints the attributes to the text writer.
		/// </summary>
		/// <param name="attributes">The attributes.</param>
		/// <param name="writer">The writer.</param>
		/// <param name="indent">The indent.</param>
		public static void PrintAttributes(MessageAttributesContract attributes, TextWriter writer, string indent = "")
		{
			var max = attributes.Items.Max(a => a.GetName().Length);

			foreach (var item in attributes.Items)
			{
				writer.Write(indent);
				writer.WriteLine("{0,-" + (max + 2) + "} : {1}", item.GetName(), GetNiceValue(item));
			}
		}

		static object GetNiceValue(MessageAttributeContract attrib)
		{
			switch (attrib.Type)
			{
				case MessageAttributeTypeContract.CreatedUtc:
					return DateTime.FromBinary(attrib.NumberValue);
				default:
					return attrib.GetValue();
			}
		}
	}
}