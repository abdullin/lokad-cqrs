#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Globalization;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
	/// <summary>
	/// Helper class for throwing storage exceptions in a consistent way.
	/// </summary>
	public static class StorageErrors
	{
		public static Exception ConditionFailed(IStorageItem item, StorageCondition condition, Exception inner = null)
		{
			var message = string.Format(CultureInfo.InvariantCulture, "Storage condition '{0}' failed for '{1}'", condition,
				item.FullPath);
			return new StorageConditionFailedException(message, inner);
		}

		public static Exception ItemNotFound(IStorageItem item, Exception inner = null)
		{
			var message = string.Format(CultureInfo.InvariantCulture, "Storage item was not found: '{0}'.", item.FullPath);
			return new StorageItemNotFoundException(message, inner);
		}

		public static Exception IntegrityFailure(IStorageItem item, Exception inner = null)
		{
			var message = string.Format(CultureInfo.InvariantCulture,
				"Local hash differs from metadata. Item was probably corrupted in trasfer, please retry: '{0}'.", item.FullPath);
			return new StorageItemIntegrityException(message, inner);
		}



		public static Exception ContainerNotFound(IStorageItem item, Exception inner = null)
		{
			var message = string.Format(CultureInfo.InvariantCulture, "Storage container was not found for: '{0}'.",
				item.FullPath);
			return new StorageContainerNotFoundException(message, inner);
		}
	}
}