namespace Lokad.Cqrs.Storage
{
	public interface IStorageContainer
	{
		IStorageContainer GetContainer(string name);
		IStorageItem GetItem(string name);

		IStorageContainer Create();
		void Remove();
		bool Exists();

		string FullPath { get; }
	}

	public interface IStorageRoot
	{
		IStorageContainer GetContainer(string name);
	}
}