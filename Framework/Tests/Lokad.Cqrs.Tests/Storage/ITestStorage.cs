using Lokad.Cqrs.Storage;

namespace Lokad.Cqrs.Tests.Storage
{
	public interface ITestStorage
	{
		IStorageContainer GetContainer(string name);
	}
}