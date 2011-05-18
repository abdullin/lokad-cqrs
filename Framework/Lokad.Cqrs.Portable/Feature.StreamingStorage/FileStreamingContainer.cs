#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Storage container using <see cref="System.IO"/> for persisting data
    /// </summary>
    public sealed class FileStreamingContainer : IStreamingContainer, IStreamingRoot
    {
        readonly DirectoryInfo _root;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamingContainer"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        public FileStreamingContainer(DirectoryInfo root)
        {
            _root = root;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamingContainer"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public FileStreamingContainer(string path) : this(new DirectoryInfo(path))
        {
        }

        public IStreamingContainer GetContainer(string name)
        {
            var child = new DirectoryInfo(Path.Combine(_root.FullName, name));
            return new FileStreamingContainer(child);
        }

        public IStreamingItem GetItem(string name)
        {
            var file = new FileInfo(Path.Combine(_root.FullName, name));
            return new FileStreamingItem(file);
        }

        public IStreamingContainer Create()
        {
            _root.Create();
            return this;
        }

        public void Delete()
        {
            _root.Refresh();
            if (_root.Exists)
                _root.Delete(true);
        }

        public bool Exists()
        {
            _root.Refresh();
            return _root.Exists;
        }

        public IEnumerable<string> ListItems()
        {
            try
            {
                return _root.GetFiles().Select(f => f.Name).ToArray();
            }
            catch (DirectoryNotFoundException e)
            {
                throw StreamingErrors.ContainerNotFound(this, e);
            }
        }

        public string FullPath
        {
            get { return _root.FullName; }
        }
    }
}