#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Represents storage container reference.
    /// </summary>
    public interface IStreamingContainer
    {
        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        string FullPath { get; }

        /// <summary>
        /// Gets the child container nested within the current container reference.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IStreamingContainer GetContainer(string name);

        /// <summary>
        /// Gets the storage item reference within the current container.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IStreamingItem GetItem(string name);

        /// <summary>
        /// Ensures that the current reference represents valid container
        /// </summary>
        /// <returns></returns>
        IStreamingContainer Create();

        /// <summary>
        /// Deletes this container
        /// </summary>
        void Delete();

        /// <summary>
        /// Checks if the underlying container exists
        /// </summary>
        /// <returns></returns>
        bool Exists();
    }
}