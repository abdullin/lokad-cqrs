namespace Bus2
{
	public interface IConsumeMessage<TMessage>
	{
		void Consume(TMessage message);
	}
}