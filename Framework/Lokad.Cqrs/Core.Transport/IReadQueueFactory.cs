namespace Lokad.Cqrs.Core.Transport
{
	public interface IReadQueueFactory
	{
		IReadQueue GetReadQueue(string name);
	}
}