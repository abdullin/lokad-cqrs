using System;
using Lokad;

namespace Bus2.Queue
{
	public sealed class GetMessageResult
	{
		readonly IncomingMessage _message;
		public readonly GetMessageResultState State;
		readonly Exception _exception;

		public Exception Exception
		{
			get
			{
				Enforce.That(State == GetMessageResultState.Exception, "State should be in error mode");
				return _exception;
			}
		}

		public IncomingMessage Message
		{
			get
			{
				Enforce.That(State == GetMessageResultState.Success, "State should be in success");
				return _message;
			}
		}

		GetMessageResult(IncomingMessage message, GetMessageResultState state, Exception exception)
		{
			_message = message;
			State = state;
			_exception = exception;
		}

		public static GetMessageResult Success(IncomingMessage message)
		{
			return new GetMessageResult(message, GetMessageResultState.Success, null);
		}

		public static readonly GetMessageResult Wait = new GetMessageResult(null, GetMessageResultState.Wait, null);
		public static readonly GetMessageResult Retry = new GetMessageResult(null, GetMessageResultState.Retry, null);

		public static GetMessageResult Error(Exception ex)
		{
			return new GetMessageResult(null, GetMessageResultState.Exception, ex);
		}
	}
}