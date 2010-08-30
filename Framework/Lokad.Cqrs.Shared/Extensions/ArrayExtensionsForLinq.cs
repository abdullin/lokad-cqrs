#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace System.Linq
{
	/// <summary>
	/// Array extensions that belong to the LINQ namespace
	/// </summary>
	public static class ArrayExtensionsForLinq
	{
		/// <summary>
		/// Joins arrays together
		/// </summary>
		/// <typeparam name="T">type of the arrays</typeparam>
		/// <param name="self">The first array to join.</param>
		/// <param name="second">The second array to join.</param>
		/// <returns>Joined array</returns>
		public static T[] Append<T>(this T[] self, params T[] second)
		{
			if (self == null) throw new ArgumentNullException("self");
			if (second == null) throw new ArgumentNullException("second");

			var newArray = new T[self.Length + second.Length];

			Array.Copy(self, newArray, self.Length);
			Array.Copy(second, 0, newArray, self.Length, second.Length);
			return newArray;
		}


		///// <summary>
		///// Joins arrays together
		///// </summary>
		///// <typeparam name="T">type of the arrays</typeparam>
		///// <param name="self">The first array to join.</param>
		///// <param name="beginning">The second array to join.</param>
		///// <returns>Joined array</returns>
		//public static T[] Prepend<T>(this T[] self, params T[] beginning)
		//{
		//    if (self == null) throw new ArgumentNullException("self");
		//    if (beginning == null) throw new ArgumentNullException("second");

		//    var newArray = new T[self.Length + beginning.Length];

		//    Array.Copy(beginning, newArray, beginning.Length);
		//    Array.Copy(self, 0, newArray, beginning.Length, self.Length);
		//    return newArray;
		//}
	}
}