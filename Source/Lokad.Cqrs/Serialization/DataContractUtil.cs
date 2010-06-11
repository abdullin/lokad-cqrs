#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Serialization
{
	static class DataContractUtil
	{
		public static void ThrowOnMessagesWithoutDataContracts(IEnumerable<Type> knownTypes)
		{
			var failures = knownTypes
				.Where(m => false == m.IsDefined(typeof (DataContractAttribute), false));

			if (failures.Any())
			{
				var list = failures.Select(f => f.FullName).Join(Environment.NewLine);

				throw new InvalidOperationException(
					"All messages must be marked with the DataContract attribute in order to be used with DCS: " + list);
			}
		}

		public static string GetContractReference(Type type)
		{
			var contract = (DataContractAttribute) type.GetCustomAttributes(typeof (DataContractAttribute), false).First();

			string ns = string.IsNullOrEmpty(contract.Namespace) ? "" : contract.Namespace.TrimEnd();
			string name = string.IsNullOrEmpty(contract.Name) ? type.Name : contract.Name;

			return ns + "/" + name;
		}
	}
}