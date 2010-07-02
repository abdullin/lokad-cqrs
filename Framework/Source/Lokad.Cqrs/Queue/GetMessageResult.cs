#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Queue
{
	public sealed class GetMessageResult
	{
		public static readonly GetMessageResult Wait = new GetMessageResult(null, GetMessageResultState.Wait, null);
		public static readonly GetMessageResult Retry = new GetMessageResult(null, GetMessageResultState.Retry, null);
		public readonly GetMessageResultState State;
		readonly Exception _exception;
		readonly UnpackedMessage _message;

		GetMessageResult(UnpackedMessage message, GetMessageResultState state, Exception exception)
		{
			_message = message;
			State = state;
			_exception = exception;
		}

		public Exception Exception
		{
			get
			{
				Enforce.That(State == GetMessageResultState.Exception, "State should be in error mode");
				return _exception;
			}
		}

		public UnpackedMessage Message
		{
			get
			{
				Enforce.That(State == GetMessageResultState.Success, "State should be in success");
				return _message;
			}
		}

		public static GetMessageResult Success(UnpackedMessage message)
		{
			return new GetMessageResult(message, GetMessageResultState.Success, null);
		}

		public static GetMessageResult Error(Exception ex)
		{
			return new GetMessageResult(null, GetMessageResultState.Exception, ex);
		}
	}
}