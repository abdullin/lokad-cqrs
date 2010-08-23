using System;

namespace Lokad.Serialization
{
	/// <summary>
	/// Class responsible for mapping types to contracts and vise-versa
	/// </summary>
	public interface IDataContractMapper
	{
		/// <summary>
		/// Gets the contract name by the type
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <returns>contract name (if found)</returns>
		Maybe<string> GetContractNameByType(Type messageType);
		/// <summary>
		/// Gets the type by contract name.
		/// </summary>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns>type that could be used for contract deserialization (if found)</returns>
		Maybe<Type> GetTypeByContractName(string contractName);
	}
}