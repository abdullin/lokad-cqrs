namespace Bus2.Queue
{
	public enum GetMessageResultState
	{
		Success,
		Wait,
		Exception,
		Retry
	}
}