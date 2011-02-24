using Lokad.Cqrs.SimpleStorage;

namespace Lokad.Cqrs.Tests.Storage
{
	public interface ITestStorage
	{
		IStorageContainer GetContainer(string name);
	}
}