using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Bus2.Serialization
{
	static class DataContractUril
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
	}
}