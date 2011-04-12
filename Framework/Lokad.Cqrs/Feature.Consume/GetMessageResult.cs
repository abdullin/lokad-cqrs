#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.Consume
{
	public sealed class GetMessageResult
	{
		public static readonly GetMessageResult Wait = new GetMessageResult(null, GetMessageResultState.Wait);
		public static readonly GetMessageResult Retry = new GetMessageResult(null, GetMessageResultState.Retry);
		public readonly GetMessageResultState State;
		readonly AzureMessageContext _message;

		GetMessageResult(AzureMessageContext message, GetMessageResultState state)
		{
			_message = message;
			State = state;
		}


		public AzureMessageContext Message
		{
			get
			{
				if (State != GetMessageResultState.Success)
					throw new InvalidOperationException("State should be in success");
				return _message;
			}
		}

		public static GetMessageResult Success(AzureMessageContext message)
		{
			return new GetMessageResult(message, GetMessageResultState.Success);
		}

		public static GetMessageResult Error()
		{
			return new GetMessageResult(null, GetMessageResultState.Exception);
		}
	}
}