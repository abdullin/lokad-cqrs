using System;
using System.Globalization;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs
{
	public static class StorageErrors
	{
		
		public static Exception ConditionFailed(IStorageItem item, StorageCondition condition, StorageClientException inner)
		{
			var message = string.Format(CultureInfo.InvariantCulture,"Storage condition '{0}' failed for '{1}'", condition, item.FullPath);
			return new StorageConditionFailedException(message, inner);
		}

		public static Exception ItemNotFound(IStorageItem item, StorageClientException inner)
		{
			var message = string.Format(CultureInfo.InvariantCulture,"Storage item was not found: '{0}'.", item.FullPath);
			return new StorageItemNotFoundException(message, inner);
		}

		public static Exception ContainerNotFound(IStorageItem item, StorageClientException inner)
		{
			var message = string.Format(CultureInfo.InvariantCulture, "Storage container was not found for: '{0}'.", item.FullPath);
			return new StorageContainerNotFoundException(message, inner);
		}
	}
}