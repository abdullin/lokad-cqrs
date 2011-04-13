using Lokad.Cqrs.Feature.StreamingStorage;

namespace Lokad.Cqrs.Tests.Storage
{
	public interface ITestStorage
	{
		IStorageContainer GetContainer(string name);
	}
}