namespace Lokad.Cqrs.Feature.Consume
{
	public interface IReadQueueFactory
	{
		IReadQueue GetReadQueue(string name);
	}
}