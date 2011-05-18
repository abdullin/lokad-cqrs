#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Globalization;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Helper class for throwing storage exceptions in a consistent way.
    /// </summary>
    public static class StreamingErrors
    {
        public static Exception ConditionFailed(IStreamingItem item, StreamingCondition condition, Exception inner = null)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Storage condition '{0}' failed for '{1}'",
                condition,
                item.FullPath);
            return new StreamingConditionFailedException(message, inner);
        }

        public static Exception ItemNotFound(IStreamingItem item, Exception inner = null)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Storage item was not found: '{0}'.",
                item.FullPath);
            return new StreamingItemNotFoundException(message, inner);
        }

        public static Exception IntegrityFailure(IStreamingItem item, Exception inner = null)
        {
            var message = string.Format(CultureInfo.InvariantCulture,
                "Local hash differs from metadata. Item was probably corrupted in trasfer, please retry: '{0}'.",
                item.FullPath);
            return new StreamingItemIntegrityException(message, inner);
        }


        public static Exception ContainerNotFound(IStreamingItem item, Exception inner = null)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Storage container was not found for: '{0}'.",
                item.FullPath);
            return new StreamingContainerNotFoundException(message, inner);
        }

        public static Exception ContainerNotFound(IStreamingContainer item, Exception inner = null)
        {
            var message = string.Format(CultureInfo.InvariantCulture, "Storage container was not found: '{0}'.",
                item.FullPath);
            return new StreamingContainerNotFoundException(message, inner);
        }
    }
}