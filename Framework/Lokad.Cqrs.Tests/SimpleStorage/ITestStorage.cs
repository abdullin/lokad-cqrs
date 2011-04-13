using Lokad.Cqrs.Feature.AzureStreamingStorage;

namespace Lokad.Cqrs.Tests.Storage
{
	public interface ITestStorage
	{
		IStorageContainer GetContainer(string name);
	}
}