using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace LmfAdapter
{
	public static class MessageUtil
	{
		public static MemoryStream SaveReferenceMessageToStream(MessageAttributesContract attributes)
		{
			var stream = new MemoryStream();
			// skip header
			stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);

			// write reference
			Serializer.Serialize(stream, attributes);
			long attributesLength = stream.Position - MessageHeader.FixedSize;
			// write header
			stream.Seek(0, SeekOrigin.Begin);
			Serializer.Serialize(stream, MessageHeader.ForReference(attributesLength, 0));
			return stream;
		}

		public static void SaveMessageToStream(UnpackedMessage message, Stream stream, IMessageSerializer serializer)
		{
			using (var mem = SaveDataMessageToStream(message.Attributes, s => serializer.Serialize(message.Content, s)))
			{
				mem.CopyTo(stream);
			}
		}

		public static MemoryStream SaveDataMessageToStream(MessageAttributesContract messageAttributes, Action<Stream> message)
		{
			var stream = new MemoryStream();

			// skip header
			stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);

			// save attributes

			Serializer.Serialize(stream, messageAttributes);
			var attributesLength = stream.Position - MessageHeader.FixedSize;

			// save message
			message(stream);
			var bodyLength = stream.Position - attributesLength - MessageHeader.FixedSize;
			// write the header
			stream.Seek(0, SeekOrigin.Begin);
			var messageHeader = MessageHeader.ForData(attributesLength, bodyLength, 0);
			Serializer.Serialize(stream, messageHeader);
			return stream;
		}

		public static MessageAttributesContract ReadAttributes(byte[] message, MessageHeader header)
		{
			using (var stream = new MemoryStream(message, MessageHeader.FixedSize, (int) header.AttributesLength))
			{
				return Serializer.Deserialize<MessageAttributesContract>(stream);
			}
		}

		public static MessageHeader ReadHeader(byte[] buffer)
		{
			using (var stream = new MemoryStream(buffer, 0, MessageHeader.FixedSize))
			{
				return Serializer.Deserialize<MessageHeader>(stream);
			}
		}

		public static IEnumerable<UnpackedMessage> ReadDataMessagesFromStream(Stream stream, IMessageSerializer serializer)
		{
			var buffer = new byte[MessageHeader.FixedSize];
			while(stream.Position < stream.Length)
			{
				stream.Read(buffer, 0, buffer.Length);
				var header = ReadHeader(buffer);
				var dataLength = header.AttributesLength + header.ContentLength;
				var message = new byte[dataLength + buffer.Length];
				Array.Copy(buffer, message, buffer.Length);

				stream.Read(message, buffer.Length,(int)dataLength);
				yield return ReadDataMessage(message, serializer);
			}
		}

		public static string ReadReferenceMessage(byte[] buffer)
		{
			var header = ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.ReferenceMessageFormatVersion)
				throw new InvalidOperationException("Unexpected message format");

			var attributes = ReadAttributes(buffer, header);
			return attributes.GetAttributeString(MessageAttributeTypeContract.StorageReference)
				.ExposeException("Protocol violation: reference message should have storage reference");
		}

		public static UnpackedMessage ReadDataMessage(byte[] buffer, IMessageSerializer serializer)
		{
			var header = ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.DataMessageFormatVersion)
				throw new InvalidOperationException("Unexpected message format");

			var attributes = ReadAttributes(buffer, header);
			string contract = attributes
				.GetAttributeString(MessageAttributeTypeContract.ContractName)
				.ExposeException("Protocol violation: message should have contract name");
			var type = serializer
				.GetTypeByContractName(contract)
				.ExposeException("Unsupported contract name: '{0}'", contract);

			var index = MessageHeader.FixedSize + (int)header.AttributesLength;
			var count = (int)header.ContentLength;
			using (var stream = new MemoryStream(buffer, index, count))
			{
				var instance = serializer.Deserialize(stream, type);
				return new UnpackedMessage(header, attributes, instance, type);
			}
		}
	}

    /// <summary>
    /// Generic data serializer interface.
    /// </summary>
    public interface IDataSerializer
    {
        /// <summary>
        /// Serializes the object to the specified stream
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="destinationStream">The destination stream.</param>
        void Serialize(object instance, Stream destinationStream);
        /// <summary>
        /// Deserializes the object from specified source stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <returns>deserialized object</returns>
        object Deserialize(Stream sourceStream, Type type);
    }

    /// <summary>
    /// Class responsible for mapping types to contracts and vise-versa
    /// </summary>
    public interface IDataContractMapper
    {


        /// <summary>
        /// Gets the type by contract name.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <returns>type that could be used for contract deserialization (if found)</returns>
        Maybe<Type> GetTypeByContractName(string contractName);
    }

    /// <summary>
    /// Joins data serializer and contract mapper
    /// </summary>
    public interface IMessageSerializer : IDataSerializer, IDataContractMapper
    {
    }

    /// <summary>
    /// Helper class that indicates nullable value in a good-citizenship code
    /// </summary>
    /// <typeparam name="T">underlying type</typeparam>
    [Serializable]
    public sealed class Maybe<T> : IEquatable<Maybe<T>>
    {
        readonly T _value;
        readonly bool _hasValue;

        Maybe(T item, bool hasValue)
        {
            _value = item;
            _hasValue = hasValue;
        }

        internal Maybe(T value)
            : this(value, true)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            // ReSharper restore CompareNonConstrainedGenericWithNull
        }

        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T), false);

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                if (!_hasValue)
                {
                    throw new InvalidOperationException("Code should not access value when it is not available.");
                }

                return _value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
        public bool HasValue
        {
            get { return _hasValue; }
        }

        /// <summary>
        /// Retrieves value from this instance, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public T GetValue(Func<T> defaultValue)
        {
            return _hasValue ? _value : defaultValue();
        }

        /// <summary>
        /// Retrieves value from this instance, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public T GetValue(T defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }

        /// <summary>
        /// Retrieves value from this instance, using a <paramref name="defaultValue"/>
        /// factory, if it is absent
        /// </summary>
        /// <param name="defaultValue">The default value to provide.</param>
        /// <returns>maybe value</returns>
        public Maybe<T> GetValue(Func<Maybe<T>> defaultValue)
        {
            return _hasValue ? this : defaultValue();
        }

        /// <summary>
        /// Retrieves value from this instance, using a <paramref name="defaultValue"/>
        /// if it is absent
        /// </summary>
        /// <param name="defaultValue">The default value to provide.</param>
        /// <returns>maybe value</returns>
        public Maybe<T> GetValue(Maybe<T> defaultValue)
        {
            return _hasValue ? this : defaultValue;
        }

     

        /// <summary>
        /// Throws the exception if maybe does not have value.
        /// </summary>
        /// <returns>actual value</returns>
        /// <exception cref="InvalidOperationException">if maybe does not have value</exception>
        public T ExposeException(string message)
        {
            if (message == null) throw new ArgumentNullException(@"message");
            if (!_hasValue)
            {
                throw new InvalidOperationException(message);
            }

            return _value;
        }

        /// <summary>
        /// Throws the exception if maybe does not have value.
        /// </summary>
        /// <returns>actual value</returns>
        /// <exception cref="InvalidOperationException">if maybe does not have value</exception>
        public T ExposeException(string message, params object[] args)
        {
            if (message == null) throw new ArgumentNullException(@"message");
            if (!_hasValue)
            {
                var text = string.Format(message, args);
                throw new InvalidOperationException(text);
            }

            return _value;
        }

     
        /// <summary>
        /// Retrieves converted value, using a 
        /// <paramref name="defaultValue"/> if it is absent.
        /// </summary>
        /// <typeparam name="TTarget">type of the conversion target</typeparam>
        /// <param name="converter">The converter.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>value</returns>
        public TTarget Convert<TTarget>(Func<T, TTarget> converter, TTarget defaultValue)
        {
            return _hasValue ? converter(_value) : defaultValue;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Maybe{T}"/> is equal to the current <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybe">The <see cref="Maybe{T}"/> to compare with.</param>
        /// <returns>true if the objects are equal</returns>
        public bool Equals(Maybe<T> maybe)
        {
            if (ReferenceEquals(null, maybe)) return false;
            if (ReferenceEquals(this, maybe)) return true;

            if (_hasValue != maybe._hasValue) return false;
            if (!_hasValue) return true;
            return _value.Equals(maybe._value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var maybe = obj as Maybe<T>;
            if (maybe == null) return false;
            return Equals(maybe);
        }

        /// <summary>
        /// Serves as a hash function for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Maybe{T}"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable CompareNonConstrainedGenericWithNull
                return ((_value != null ? _value.GetHashCode() : 0) * 397) ^ _hasValue.GetHashCode();
                // ReSharper restore CompareNonConstrainedGenericWithNull
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Performs an implicit conversion from <typeparamref name="T"/> to <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Maybe<T>(T item)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (item == null) throw new ArgumentNullException("item");
            // ReSharper restore CompareNonConstrainedGenericWithNull

            return new Maybe<T>(item);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Maybe{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator T(Maybe<T> item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (!item.HasValue) throw new ArgumentException("May be must have value");

            return item.Value;
        }



        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_hasValue)
            {
                return "<" + _value + ">";
            }

            return "<Empty>";
        }
    }
}