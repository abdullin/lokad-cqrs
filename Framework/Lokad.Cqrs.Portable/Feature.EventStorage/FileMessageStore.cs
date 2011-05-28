using System.IO;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.EventStorage
{
    public sealed class FileEventStoreWriter
    {
        FileInfo _dataFile;
        FileInfo _rootFile;

        int _version;
        long _pointer;

        public void Init()
        {
            if (!_dataFile.Exists)
                using(_dataFile.Create()){}

        }

        ThreadAffinity _affinity;
        public void Append(byte[] envelope)
        {
            if (_affinity == null)
            {
                _affinity = new ThreadAffinity();
            }
            _affinity.Check();
            
            using (var f = new FileStream(_dataFile.FullName,FileMode.Append,FileAccess.Write,FileShare.Read))
            {
                f.Write(envelope,);
            }


        }
    }

    public sealed class FileEventStoreReader
    {
        
    }
}