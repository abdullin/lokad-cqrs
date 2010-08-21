#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Storage;
using Lokad.Quality;
using Lokad.Serialization;

namespace Lokad.Cqrs.Views
{
	[UsedImplicitly]
	public sealed class EntityStorage : IEntityReader, IEntityWriter
	{
		readonly IStorageContainer _container;
		readonly IDataSerializer _serializer;
		readonly IDataContractMapper _mapper;

		public EntityStorage(IStorageContainer container, IDataSerializer serializer, IDataContractMapper mapper)
		{
			_container = container;
			_serializer = serializer;
			_mapper = mapper;
		}

		IStorageItem MapTypeAndIdentity(Type type, object identity)
		{
			var value = _mapper
				.GetContractNameByType(type)
				.ExposeException(
					() => Errors.InvalidOperation("Type '{0}' should have contract name defined in '{1}'.", type, _mapper.GetType()));

			var name = string.Format("{0}-{1}.view", value, identity).ToLowerInvariant();
			return _container.GetItem(name);
		}

		

		public void Write(Type type, object identity, object item)
		{
			var storage = MapTypeAndIdentity(type, identity);
			storage.Write(stream => _serializer.Serialize(item, stream));
		}

		public Maybe<object> Read(Type type, object identity)
		{
			var storage = MapTypeAndIdentity(type, identity);
			try
			{
				object loaded = null;
				storage.ReadInto((props, stream) => loaded = _serializer.Deserialize(stream, type));
				return loaded;
			}
			catch (StorageItemNotFoundException)
			{
				return Maybe<object>.Empty;
			}
		}

		public void AddOrUpdate(Type type, object key, AddEntityDelegate addEntityDelegate, UpdateEntityDelegate updateEntityDelegate)
		{
			var item = MapTypeAndIdentity(type, key);

			object source = null;
			var condition = StorageCondition.None;

			try
			{

				item.ReadInto((props, stream) =>
					{
						source = _serializer.Deserialize(stream, type);
						condition = StorageCondition.IfMatch(props.ETag);
					});
			}
			catch (StorageItemNotFoundException)
			{
			}

			if (null == source)
			{
				source = addEntityDelegate(key);
			}
			else
			{
				updateEntityDelegate(key, source);
			}

			// if we fail condition, then this means, that
			// there was a concurrency problem
			try
			{
				item.Write(stream => _serializer.Serialize(source, stream), condition);
			}
			catch (StorageConditionFailedException ex)
			{
				var msg = string.Format("Record was modified concurrently: '{0}'; Id: '{1}'. Please, retry.", type, key);
				throw new OptimisticConcurrencyException(msg, ex);
			}
		}

		public void Remove(Type type, object key)
		{
			MapTypeAndIdentity(type, key).Remove();
		}
	}
}