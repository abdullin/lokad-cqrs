#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AzureStreamingStorage
{
	/// <summary>
	/// Storage root (Azure Blob account or file drive)
	/// </summary>
	public interface IStorageRoot
	{
		/// <summary>
		/// Gets the container reference, identified by it's name
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>new container referece</returns>
		IStorageContainer GetContainer(string name);
	}
}