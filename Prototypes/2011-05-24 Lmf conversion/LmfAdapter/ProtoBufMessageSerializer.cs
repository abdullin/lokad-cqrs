using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProtoBuf;

namespace LmfAdapter
{
    public class ProtoBufMessageSerializer : IMessageSerializer
    {
        readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
        readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();
        readonly IDictionary<Type, IFormatter> _type2Formatter = new Dictionary<Type, IFormatter>();

        public ProtoBufMessageSerializer(ICollection<Type> knownTypes)
        {
            if (knownTypes.Count == 0)
                throw new InvalidOperationException("ProtoBuf requires some known types to serialize. Have you forgot to supply them?");
            foreach (var type in knownTypes)
            {
                var reference = ProtoBufUtil.GetContractReference(type);
                var formatter = ProtoBufUtil.CreateFormatter(type);

                _contract2Type.Add(reference, type);
                _type2Contract.Add(type, reference);
                _type2Formatter.Add(type, formatter);
            }
        }

        public void Serialize(object instance, Stream destination)
        {
            _type2Formatter
                .GetValue(instance.GetType())
                .ExposeException("Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?", instance.GetType())
                .Serialize(destination, instance);
        }

        public object Deserialize(Stream source, Type type)
        {
            return _type2Formatter
                .GetValue(type)
                .ExposeException("Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?", type)
                .Deserialize(source);
        }

        public Maybe<string> GetContractNameByType(Type messageType)
        {
            return _type2Contract.GetValue(messageType);
        }

        public Maybe<Type> GetTypeByContractName(string contractName)
        {
            return _contract2Type.GetValue(contractName);
        }
    }

    /// <summary>
    /// Extensions for <see cref="IDictionary{TKey,TValue}"/>
    /// </summary>
    public static class ExtendIDictionary
    {
       

     
        /// <summary>
        /// Gets the value from the <paramref name="dictionary"/> in form of the <see cref="Maybe{T}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>value from the dictionary</returns>
        public static Maybe<TValue> GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return Maybe<TValue>.Empty;
        }
    }

    sealed class AttributeHelper
    {
        readonly object[] _attributes;

        public AttributeHelper(object[] attributes)
        {
            _attributes = attributes;
        }

        public Maybe<string> GetString<TAttribute>(Func<TAttribute, string> retriever)
            where TAttribute : Attribute
        {
            var v = _attributes
                .OfType<TAttribute>().FirstOrEmpty()
                .Convert(retriever, "");

            if (string.IsNullOrEmpty(v))
                return Maybe<string>.Empty;

            return v;
        }
    }

    /// <summary>
    /// Helper methods for the <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class ExtendIEnumerable
    {
       

        /// <summary>
        /// Retrieves first value from the <paramref name="sequence"/>
        /// </summary>
        /// <typeparam name="TSource">The type of the source sequence.</typeparam>
        /// <param name="sequence">The source.</param>
        /// <returns>first value or empty result, if it is not found</returns>
        public static Maybe<TSource> FirstOrEmpty<TSource>( this IEnumerable<TSource> sequence)
        {
            if (sequence == null) throw new ArgumentNullException("sequence");
            foreach (var source in sequence)
            {
                return source;
            }
            return Maybe<TSource>.Empty;
        }

    }

    public static class ProtoBufUtil
    {
        public static string GetContractReference(Type type)
        {
            var attribs = type.GetCustomAttributes(false);
            var helper = new AttributeHelper(attribs);


            var name = Maybe<string>.Empty
                .GetValue(() => helper.GetString<ProtoContractAttribute>(p => p.Name))
                .GetValue(() => helper.GetString<DataContractAttribute>(p => p.Name))
                .GetValue(() => helper.GetString<XmlTypeAttribute>(p => p.TypeName))
                .GetValue(() => type.Name);

            var ns = Maybe<string>.Empty
                .GetValue(() => helper.GetString<DataContractAttribute>(p => p.Namespace))
                .GetValue(() => helper.GetString<XmlTypeAttribute>(p => p.Namespace))
                .Convert(s => s.Trim() + "/", "");

            return ns + name;
        }


        public static IFormatter CreateFormatter(Type type)
        {
            try
            {
                typeof(Serializer)
                    .GetMethod("PrepareSerializer")
                    .MakeGenericMethod(type)
                    .Invoke(null, null);
            }
            catch (TargetInvocationException tie)
            {
                var message = string.Format("Failed to prepare ProtoBuf serializer for '{0}'.", type);
                throw new InvalidOperationException(message, tie.InnerException);
            }

            try
            {
                return (IFormatter)typeof(Serializer)
                    .GetMethod("CreateFormatter")
                    .MakeGenericMethod(type)
                    .Invoke(null, null);
            }
            catch (TargetInvocationException tie)
            {
                var message = string.Format("Failed to create ProtoBuf formatter for '{0}'.", type);
                throw new InvalidOperationException(message, tie.InnerException);
            }
        }
    }
}