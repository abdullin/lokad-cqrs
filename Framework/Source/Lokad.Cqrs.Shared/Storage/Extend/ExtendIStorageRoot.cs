#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Storage;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Extensions for the <see cref="IStorageRoot"/>
	/// </summary>
	public static class ExtendIStorageRoot
	{
		/// <summary>
		/// Deletes the child container.
		/// </summary>
		/// <param name="root">The root.</param>
		/// <param name="name">The name.</param>
		public static void DeleteChildContainer(this IStorageRoot root, string name)
		{
			root.GetContainer(name).Delete();
		}
	}
}