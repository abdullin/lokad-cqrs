using System;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage.Azure
{
	/// <summary>
	/// Azure implementation of the view reader/writer
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TView">The type of the view.</typeparam>
	public sealed class AzureAtomicEntityWriter<TKey, TView> :
		IAtomicEntityWriter<TKey, TView>
		where TView : IAtomicEntity<TKey>
	{
		readonly CloudBlobContainer _container;
		readonly IAzureAtomicStorageStrategy _convention;

		public AzureAtomicEntityWriter(CloudBlobClient client, IAzureAtomicStorageStrategy convention)
		{
			var containerName = _convention.GetFolderForEntity(typeof(TView));
			_container = client.GetContainerReference(containerName);
			_convention = convention;
		}

		public Maybe<TView> Get(TKey key)
		{
			var blob = _container.GetBlobReference(ComposeName(key));
			string text;
			try
			{
				text = blob.DownloadText();
			}
			catch (StorageClientException ex)
			{
				return Maybe<TView>.Empty;
			}
			return _convention.Deserialize<TView>(text);
		}

		public TView Load(TKey key)
		{
			return Get(key).ExposeException("Failed to load '{0}' with key '{1}'.", typeof(TView).Name, key);
		}

		public void AddOrUpdate(TKey key, Func<TView> addViewFactory, Action<TView> updateViewFactory)
		{
			var blob = _container.GetBlobReference(ComposeName(key));

			TView view;
			try
			{
				var downloadText = blob.DownloadText();
				view = _convention.Deserialize<TView>(downloadText);
				updateViewFactory(view);
			}
			catch (StorageClientException ex)
			{
				view = addViewFactory();
			}

			blob.UploadText(_convention.Serialize(view));
		}

		public void AddOrUpdate(TKey key, TView newView, Action<TView> updateViewFactory)
		{
			AddOrUpdate(key, () => newView, updateViewFactory);
		}

		public void UpdateOrThrow(TKey key, Action<TView> change)
		{
			var blob = _container.GetBlobReference(ComposeName(key));
			string text;
			try
			{
				text = blob.DownloadText();
			}
			catch (StorageClientException ex)
			{
				throw Errors.InvalidOperation("Failed to load view {0}-{1}", typeof(TView), key);
			}
			var view = _convention.Deserialize<TView>(text);
			change(view);
			blob.UploadText(_convention.Serialize(view));
		}

		public bool TryUpdate(TKey key, Action<TView> change)
		{
			var blob = _container.GetBlobReference(ComposeName(key));
			string downloadText;
			try
			{
				downloadText = blob.DownloadText();
			}
			catch (StorageClientException e)
			{
				return false;
			}
			var view = _convention.Deserialize<TView>(downloadText);
			change(view);
			blob.UploadText(_convention.Serialize(view));
			return true;
		}

		public void Delete(TKey key)
		{
			var blob = _container.GetBlobReference(ComposeName(key));
			blob.DeleteIfExists();
		}

		string ComposeName(TKey key)
		{
			return _convention.GetNameForEntity(key);
		}
	}
}