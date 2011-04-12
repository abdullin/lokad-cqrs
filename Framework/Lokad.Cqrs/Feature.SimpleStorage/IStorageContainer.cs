#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.SimpleStorage
{
	/// <summary>
	/// Represents storage container reference.
	/// </summary>
	public interface IStorageContainer
	{
		/// <summary>
		/// Gets the full path.
		/// </summary>
		/// <value>The full path.</value>
		string FullPath { get; }

		/// <summary>
		/// Gets the child container nested within the current container reference.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		IStorageContainer GetContainer(string name);

		/// <summary>
		/// Gets the storage item reference within the current container.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		IStorageItem GetItem(string name);

		/// <summary>
		/// Ensures that the current reference represents valid container
		/// </summary>
		/// <returns></returns>
		IStorageContainer Create();

		/// <summary>
		/// Deletes this container
		/// </summary>
		void Delete();

		/// <summary>
		/// Checks if the underlying container exists
		/// </summary>
		/// <returns></returns>
		bool Exists();
	}
}