using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using ServiceStack.Text;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class FileTapeStorageTests : TapeStorageTests
    {
        string _path;
        
        ITapeStorageFactory _storageFactory;

        protected override void PrepareEnvironment()
        {
            _path = Path.GetTempFileName();
            File.Delete(_path);
        }

        protected override ITapeStream InitializeAndGetTapeStorage()
        {
            _storageFactory = new FileTapeStorageFactory(_path);
            _storageFactory.InitializeForWriting();

            const string name = "test";
            return _storageFactory.GetOrCreateStream(name);
        }

        protected override void FreeResources()
        {
            _storageFactory = null;
        }

        protected override void TearDownEnvironment()
        {
            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }

        [Test]
        public void Contents_are_readable_as_unicode()
        {
           
            var s = new
                {
                    Company = "Lokad",
                    Project = "Lokad.CQRS",
                    Component = "Tape Storage",
                    Supported = new [] {" Azure Blob", "MS SQL", "File", "Memory"}
                }.SerializeAndFormat();
            var buffer = Encoding.UTF8.GetBytes(s);
            _stream.TryAppend(buffer);

            
            var readAllText = File.ReadAllText(Path.Combine(_path, "test"));
            foreach (var c in readAllText)
            {
                var category = char.GetUnicodeCategory(c);
                Assert.IsTrue(char.IsWhiteSpace(c)|| char.IsControl(c)|| char.IsLetterOrDigit(c) || char.IsPunctuation(c) || category == UnicodeCategory.MathSymbol, "String comes garbled at '{0}' ({1})", c, category);
            }
            //Console.WriteLine(readAllText);
        }
    }
}
