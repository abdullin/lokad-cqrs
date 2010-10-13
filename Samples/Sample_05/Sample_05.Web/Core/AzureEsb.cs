#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

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