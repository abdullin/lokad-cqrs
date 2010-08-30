#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Globalization;

namespace Lokad
{
	/// <summary>
	/// 	Helper routines for converting strings into Maybe
	/// </summary>
	public static class MaybeParse
	{
		/// <summary>
		/// 	Tries to parse the specified string into the enum, returning empty result
		/// 	on failure. We ignore case in this scenario.
		/// </summary>
		/// <typeparam name="TEnum">
		/// 	The type of the enum.
		/// </typeparam>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either enum or an empty result
		/// </returns>
		public static Maybe<TEnum> Enum<TEnum>(string value) where TEnum : struct
		{
			return Enum<TEnum>(value, true);
		}

		/// <summary>
		/// 	Tries to parse the specified string into the enum, returning empty result
		/// 	on failure
		/// </summary>
		/// <typeparam name="TEnum">
		/// 	The type of the enum.
		/// </typeparam>
		/// <param name="value">The value.</param>
		/// <param name="ignoreCase">
		/// 	if set to
		/// 	<c>true</c>
		/// 	then parsing will ignore case.
		/// </param>
		/// <returns>
		/// 	either enum or an empty result
		/// </returns>
		public static Maybe<TEnum> Enum<TEnum>(string value, bool ignoreCase) where TEnum : struct
		{
			if (string.IsNullOrEmpty(value))
				return Maybe<TEnum>.Empty;
			try
			{
				return EnumUtil.Parse<TEnum>(value, ignoreCase);
			}
			catch (Exception)
			{
				return Maybe<TEnum>.Empty;
			}
		}

		/// <summary>
		/// 	Tries to parse the specified value into Decimal, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Decimal or an empty result
		/// </returns>
		/// <seealso cref="decimal.TryParse(string,out decimal)" />
		public static Maybe<decimal> Decimal(string value)
		{
			decimal result;
			if (!decimal.TryParse(value, out result))
				return Maybe<decimal>.Empty;
			return result;
		}

		/// <summary>
		/// 	Tries to parse the specified value into decimal, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="numberStyles">
		/// 	The number styles to use.
		/// </param>
		/// <param name="formatProvider">
		/// 	The format provider to use.
		/// </param>
		/// <returns>
		/// 	either parsed decimal or an empty result
		/// </returns>
		/// <seealso cref="decimal.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out decimal)" />
		public static Maybe<decimal> Decimal(string value,
			NumberStyles numberStyles,
			IFormatProvider formatProvider)
		{
			decimal result;
			if (!decimal.TryParse(value, numberStyles, formatProvider, out result))
				return Maybe<decimal>.Empty;
			return result;
		}

		/// <summary>
		/// 	Tries to parse the specified value into decimal, using the invariant culture
		/// 	info and returning	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed decimal or an empty result
		/// </returns>
		public static Maybe<decimal> DecimalInvariant(string value)
		{
			return Decimal(value, NumberStyles.Number, CultureInfo.InvariantCulture);
		}


		/// <summary>
		/// 	Tries to parse the specified value into Int32, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Int32 or an empty result
		/// </returns>
		/// <seealso cref="System.Int32.TryParse(string,out int)" />
		public static Maybe<Int32> Int32(string value)
		{
			Int32 result;
			if (!System.Int32.TryParse(value, out result))
				return Maybe<Int32>.Empty;
			return result;
		}

		/// <summary>
		/// 	Tries to parse the specified value into Int32, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="numberStyles">
		/// 	The number styles to use.
		/// </param>
		/// <param name="formatProvider">
		/// 	The format provider to use.
		/// </param>
		/// <returns>
		/// 	either parsed Int32 or an empty result
		/// </returns>
		/// <seealso cref="System.Int32.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out int)" />
		public static Maybe<Int32> Int32(string value,
			NumberStyles numberStyles,
			IFormatProvider formatProvider)
		{
			Int32 result;
			if (!System.Int32.TryParse(value, numberStyles, formatProvider, out result))
				return Maybe<Int32>.Empty;
			return result;
		}

		/// <summary>
		/// 	Tries to parse the specified string value into Int32, 
		/// 	using an invariant culture and returning empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Int32 or an empty result
		/// </returns>
		/// <seealso cref="System.Int32.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out int)" />
		public static Maybe<Int32> Int32Invariant(string value)
		{
			return Int32(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// 	Tries to parse the specified value into Int64, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Int64 or an empty result
		/// </returns>
		/// <seealso cref="System.Int64.TryParse(string,out long)" />
		public static Maybe<Int64> Int64(string value)
		{
			Int64 result;
			if (!System.Int64.TryParse(value, out result))
				return Maybe<Int64>.Empty;
			return result;
		}

		/// <summary>
		/// 	Tries to parse the specified value into Int64, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="numberStyles">
		/// 	The number styles to use.
		/// </param>
		/// <param name="formatProvider">
		/// 	The format provider to use.
		/// </param>
		/// <returns>
		/// 	either parsed Int64 or an empty result
		/// </returns>
		/// <seealso cref="System.Int64.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out long)" />
		public static Maybe<Int64> Int64(string value,
			NumberStyles numberStyles,
			IFormatProvider formatProvider)
		{
			Int64 result;
			if (!System.Int64.TryParse(value, numberStyles, formatProvider, out result))
				return Maybe<Int64>.Empty;
			return result;
		}

		/// <summary>
		/// 	Tries to parse the specified string value into Int64, 
		/// 	using an invariant culture and returning empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Int64 or an empty result
		/// </returns>
		/// <seealso cref="System.Int64.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out long)" />
		public static Maybe<Int64> Int64Invariant(string value)
		{
			return Int64(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}


		/// <summary>
		/// 	Tries to parse the specified value into Double, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Double or an empty result
		/// </returns>
		/// <seealso cref="System.Double.TryParse(string,out double)" />
		public static Maybe<Double> Double(string value)
		{
			Double result;
			if (!System.Double.TryParse(value, out result))
				return Maybe<Double>.Empty;
			return result;
		}

		/// <summary>
		/// 	Tries to parse the specified value into Double, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="numberStyles">
		/// 	The number styles to use.
		/// </param>
		/// <param name="formatProvider">
		/// 	The format provider to use.
		/// </param>
		/// <returns>
		/// 	either parsed Double or an empty result
		/// </returns>
		/// <seealso cref="System.Double.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out double)" />
		public static Maybe<Double> Double(string value,
			NumberStyles numberStyles,
			IFormatProvider formatProvider)
		{
			Double result;
			if (!System.Double.TryParse(value, numberStyles, formatProvider, out result))
				return Maybe<Double>.Empty;
			return result;
		}


		/// <summary>
		/// 	Attempts to parse the specified value into Double, 
		/// 	using invariant culture and returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Double or an empty result
		/// </returns>
		/// <seealso cref="System.Double.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out double)" />
		public static Maybe<Double> DoubleInvariant(string value)
		{
			return Double(value,
				NumberStyles.Float | NumberStyles.AllowThousands,
				CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// 	Tries to parse the specified value into Single, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Single or an empty result
		/// </returns>
		/// <seealso cref="System.Single.TryParse(string,out float)" />
		public static Maybe<Single> Single(string value)
		{
			Single result;
			if (!System.Single.TryParse(value, out result))
				return Maybe<Single>.Empty;
			return result;
		}

		/// <summary>
		/// 	Tries to parse the specified value into Single, returning
		/// 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="numberStyles">
		/// 	The number styles to use.
		/// </param>
		/// <param name="formatProvider">
		/// 	The format provider to use.
		/// </param>
		/// <returns>
		/// 	either parsed Single or an empty result
		/// </returns>
		/// <seealso cref="System.Single.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out float)" />
		public static Maybe<Single> Single(string value,
			NumberStyles numberStyles,
			IFormatProvider formatProvider)
		{
			Single result;
			if (!System.Single.TryParse(value, numberStyles, formatProvider, out result))
				return Maybe<Single>.Empty;
			return result;
		}


		/// <summary>
		/// 	Tries to parse the specified value into Single, using invariant culture
		/// 	and returning 	empty result on failure.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	either parsed Single or an empty result
		/// </returns>
		/// <seealso cref="System.Single.TryParse(string,System.Globalization.NumberStyles,System.IFormatProvider,out float)" />
		public static Maybe<Single> SingleInvariant(string value)
		{
			return Single(value,
				NumberStyles.Float | NumberStyles.AllowThousands,
				CultureInfo.InvariantCulture);
		}
	}
}