using System;
using System.Diagnostics;
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
            _path = Path.Combine(Directory.GetCurrentDirectory(), "file-tape-store");
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

        [Test,Explicit]
        public void Performance_tests()
        {
            // random write.
            var gen = new RNGCryptoServiceProvider();
            var bytes = 1024;
            var data = new byte[bytes];
            gen.GetNonZeroBytes(data);
            var stopwatch = Stopwatch.StartNew();
            var records = 100000;

            var total = records * bytes;
            Console.WriteLine("Data is {0} records of {1} bytes == {2} bytes or {3} MB", records, bytes, total, total/1024/1024);

            for (int i = 0; i < records; i++)
            {
                _stream.TryAppend(data);
            }

            var timeSpan = stopwatch.Elapsed;
            Console.WriteLine("Writing one by one in {0}",timeSpan.TotalSeconds);

            var length = new FileInfo(Path.Combine(_path, "test")).Length;
            Console.WriteLine("Total file size: {0}", length);

            int counter = 0;
            var reading = Stopwatch.StartNew();
            foreach (var tapeRecord in _stream.ReadRecords(0, int.MaxValue))
            {
                counter += tapeRecord.Data.Length;
            }
            Console.WriteLine("Reading in {0} seconds", reading.Elapsed.TotalSeconds);
            Console.WriteLine("Read {0} bytes of raw data", counter);

        }
    }
}
