using System;
using System.IO;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class FileAtomicEntityContainer<TKey,TEntity> : IAtomicEntityReader<TKey,TEntity>, IAtomicEntityWriter<TKey,TEntity>
    {
        readonly IAtomicStorageStrategy _strategy;
        readonly string _folderPath;

        public FileAtomicEntityContainer(string directoryPath, IAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
            _folderPath = Path.Combine(directoryPath, _strategy.GetFolderForEntity(typeof (TEntity)));
        }

        public bool TryGet(TKey key, out TEntity view)
        {
            view = default(TEntity);
            try
            {
                var name = GetName(key);
                using (var stream = File.OpenRead(name))
                {
                    view = _strategy.Deserialize<TEntity>(stream);
                    return true;
                }
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
        }

        string GetName(TKey key)
        {
            return Path.Combine(_folderPath, _strategy.GetNameForEntity(typeof (TEntity), key));
        }

        public TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint)
        {
            var name = GetName(key);
            try
            {
                // we are locking this file.
                using (var file = File.Open(name, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    TEntity result;
                    if (file.Length == 0)
                    {
                        result = addFactory();
                    }
                    else
                    {
                        using (var mem = new MemoryStream())
                        {
                            file.CopyTo(mem);
                            mem.Seek(0, SeekOrigin.Begin);
                            var entity = _strategy.Deserialize<TEntity>(mem);
                            result = update(entity);
                        }
                    }
                    
                    // some serializers have nasty habbit of closing the
                    // underling stream
                    using (var mem = new MemoryStream())
                    {
                        _strategy.Serialize(result, mem);
                        var data = mem.ToArray();
                        file.Seek(0, SeekOrigin.Begin);
                        file.Write(data, 0, data.Length);
                        // truncate this file
                        file.SetLength(data.Length);
                    }
                    
                    return result;
                }
            }
            catch (DirectoryNotFoundException)
            {
                var s = string.Format(
                            "Container '{0}' does not exist. You need to initialize this atomic storage and ensure that '{1}' is known to '{2}'.",
                            _folderPath, typeof(TEntity).Name, _strategy.GetType().Name);
                throw new InvalidOperationException(s);
            }
        }

        public bool TryDelete(TKey key)
        {
            var name = GetName(key);
            if (File.Exists(name))
            {
                File.Delete(name);
                return true;
            }
            return false;
        }
    }
}