using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lokad.Serialization
{
	/// <summary>
	/// Helper class to work with <see cref="DataContract"/>
	/// </summary>
	public static class DataContractUtil
	{
		/// <summary>
		/// Throws detailed exception the on messages without data contracts.
		/// </summary>
		/// <param name="knownTypes">The known types.</param>
		public static void ThrowOnMessagesWithoutDataContracts(IEnumerable<Type> knownTypes)
		{
			var failures = knownTypes
				.Where(m => false == m.IsDefined(typeof(DataContractAttribute), false));

			if (failures.Any())
			{
				var list = failures.Select(f => f.FullName).Join(Environment.NewLine);

				throw new InvalidOperationException(
					"All messages must be marked with the DataContract attribute in order to be used with DCS: " + list);
			}
		}

		/// <summary>
		/// Gets the contract reference, combining contract properties.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static string GetContractReference(Type type)
		{
			var contract = (DataContractAttribute)type.GetCustomAttributes(typeof(DataContractAttribute), false).First();

			var name = string.IsNullOrEmpty(contract.Name) ? type.Name : contract.Name;
			if (string.IsNullOrEmpty(contract.Namespace))
				return name;

			return contract.Namespace + "/" + name;
		}
	}
}