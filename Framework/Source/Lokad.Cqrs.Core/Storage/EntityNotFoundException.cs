using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs
{
	/// <summary>
	/// When entity is not found
	/// </summary>
	[Serializable]
	public class EntityNotFoundException : Exception
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public EntityNotFoundException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public EntityNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info"/> parameter is null.
		/// </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">
		/// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
		/// </exception>
		protected EntityNotFoundException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}

	[Serializable]
	public class EntityAlreadyExistsException : Exception
	{
		public EntityAlreadyExistsException()
		{
		}

		public EntityAlreadyExistsException(string message) : base(message)
		{
		}

		public EntityAlreadyExistsException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EntityAlreadyExistsException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}

	public static class CqrsErrors
	{
		public static Exception EntityNotFound(Type type, object identity)
		{
			var message = string.Format("Failed to find '{0}' with identity '{1}'", type, identity);
			return new EntityNotFoundException(message);
		}
		public static Exception EntityAlreadyExists(Type type, object  identity)
		{
			var message = string.Format("Failed to create '{0}' with identity '{1}'", type, identity);
			return new EntityAlreadyExistsException(message);
		}
	}
}