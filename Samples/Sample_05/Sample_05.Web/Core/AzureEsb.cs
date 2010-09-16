using Sample_05.Contracts;

namespace Sample_05.Web
{
	public static class AzureEsb
	{
		public static void SendCommand(IDomainCommand command)
		{
			GlobalSetup.Client.SendMessage(command);
		}
	}
}