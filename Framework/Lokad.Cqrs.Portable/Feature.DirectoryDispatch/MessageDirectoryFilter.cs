using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
{
    public sealed class MessageDirectoryFilter
    {
        readonly HashSet<Func<MessageMapping, bool>> _filters = new HashSet<Func<MessageMapping, bool>>();

        public bool DoesPassFilter(MessageMapping mapping)
        {
            return _filters.All(filter => filter(mapping));
        }

        /// <summary>
        /// Adds custom filters for <see cref="MessageMapping"/>, that will be used
        /// for configuring this message handler.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public MessageDirectoryFilter WhereMappings(Func<MessageMapping, bool> filter)
        {
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds filter to exclude all message mappings, where messages derive from the specified class
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public MessageDirectoryFilter WhereMessagesAreNot<TMessage>()
        {
            return WhereMappings(mm => !typeof (TMessage).IsAssignableFrom(mm.Message));
        }

        /// <summary>
        /// Adds filter to include only message mappings, where messages derive from the specified class
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public MessageDirectoryFilter WhereMessagesAre<TMessage>()
        {
            return WhereMappings(mm => typeof (TMessage).IsAssignableFrom(mm.Message));
        }

        /// <summary>
        /// Adds filter to include only message mappings, where consumers derive from the specified class
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public MessageDirectoryFilter WhereConsumersAre<TConsumer>()
        {
            return WhereMappings(mm => typeof (TConsumer).IsAssignableFrom(mm.Consumer));
        }

        /// <summary>
        /// Adds filter to exclude all message mappings, where consumers derive from the specified class
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public MessageDirectoryFilter WhereConsumersAreNot<TConsumer>()
        {
            return WhereMappings(mm => !typeof (TConsumer).IsAssignableFrom(mm.Consumer));
        }
    }
}