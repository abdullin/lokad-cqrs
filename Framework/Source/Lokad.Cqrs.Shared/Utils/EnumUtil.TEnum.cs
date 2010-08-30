#region (c)2009-2010 Lokad - New BSD license

// Copyright (c) Lokad 2009-2010 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad
{
	/// <summary>
	/// Strongly-typed enumeration util
	/// </summary>
	/// <typeparam name="TEnum">The type of the enum.</typeparam>
	public static class EnumUtil<TEnum> where TEnum : struct, IComparable
	{
		/// <summary>
		/// Values of the <typeparamref name="TEnum"/>
		/// </summary>
		public static readonly TEnum[] Values;

		/// <summary>
		/// Values of the <typeparamref name="TEnum"/> without the default value.
		/// </summary>
		public static readonly TEnum[] ValuesWithoutDefault;

		internal static readonly string EnumPrefix = typeof (TEnum).Name + "_";

		/// <summary>
		/// Efficient comparer for the enum
		/// </summary>
		public static readonly IEqualityComparer<TEnum> Comparer;

		static EnumUtil()
		{
			Values = GetValues();
			var def = default(TEnum);
			ValuesWithoutDefault = Values.Where(x => !def.Equals(x)).ToArray();
			Comparer = EnumComparer<TEnum>.Instance;
		}

		/// <summary>
		/// Converts the specified enum safely from the target enum. Matching is done
		/// via the efficient <see cref="Comparer"/> initialized with <see cref="MaybeParse.Enum{TEnum}(string)"/>
		/// </summary>
		/// <typeparam name="TSourceEnum">The type of the source enum.</typeparam>
		/// <param name="enum">The @enum to convert from.</param>
		/// <returns>converted enum</returns>
		/// <exception cref="ArgumentException"> when conversion is not possible</exception>
		public static TEnum ConvertSafelyFrom<TSourceEnum>(TSourceEnum @enum)
			where TSourceEnum : struct, IComparable
		{
			return EnumUtil<TSourceEnum, TEnum>.Convert(@enum);
		}

		static TEnum[] GetValues()
		{
			Type enumType = typeof (TEnum);

			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Type is not an enum: '" + enumType.Name);
			}

#if !SILVERLIGHT2

			return Enum
				.GetValues(enumType)
				.Cast<TEnum>()
				.ToArray();
#else
			return enumType
				.GetFields()
				.Where(field => field.IsLiteral)
				.ToArray(f => (TEnum) f.GetValue(enumType));
#endif
		}
	}
}