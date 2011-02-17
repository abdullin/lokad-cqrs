using Lokad.Cqrs.Storage;
using Lokad.Storage;

namespace Lokad.Cqrs.Tests.Storage
{
	public interface ITestStorage
	{
		IStorageContainer GetContainer(string name);
	}
}