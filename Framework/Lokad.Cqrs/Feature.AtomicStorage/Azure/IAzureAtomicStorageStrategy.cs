using System;

namespace Lokad.Cqrs.Feature.AtomicStorage.Azure
{
	public interface IAzureAtomicStorageStrategy
	{
		string GetFolderForEntity(Type entityType);
		string GetFolderForSingleton();
		string GetNameForEntity<TKey>(TKey key);
		string GetNameForSingleton(Type singletonType);
		string Serialize<TEntity>(TEntity entity);
		TEntity Deserialize<TEntity>(string source);
	}
}