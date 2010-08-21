#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;

namespace Lokad.Cqrs.Storage
{
	public sealed class FileStorageContainer : IStorageContainer
	{
		readonly DirectoryInfo _root;

		public FileStorageContainer(DirectoryInfo root)
		{
			_root = root;
		}

		public IStorageContainer GetContainer(string name)
		{
			var child = new DirectoryInfo(Path.Combine(_root.FullName, name));
			return new FileStorageContainer(child);
		}

		public IStorageItem GetItem(string name)
		{
			var file = new FileInfo(Path.Combine(_root.FullName, name));
			return new FileStorageItem(file);
		}

		public IStorageContainer Create()
		{
			_root.Create();
			return this;
		}

		public void Remove()
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

		public string FullPath
		{
			get { return _root.FullName; }
		}
	}
}