#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.StreamingStorage.Azure
{
    static class BlobStorageUtil
    {
        public const string LokadHashFieldName = "LokadContentMD5";

        public static StorageItemInfo MapFetchedAttrbitues(CloudBlob blob)
        {
            var meta = new NameValueCollection(blob.Metadata);
            var properties = new Dictionary<string, string>(5);
            var props = blob.Properties;
            if (!string.IsNullOrEmpty(props.ContentMD5))
            {
                properties["ContentMD5"] = props.ContentMD5;
            }
            if (!string.IsNullOrEmpty(props.ContentEncoding))
            {
                properties["ContentEncoding"] = props.ContentEncoding;
            }
            if (!string.IsNullOrEmpty(props.ContentType))
            {
                properties["ContentType"] = props.ContentType;
            }
            properties["BlobType"] = props.BlobType.ToString();
            properties["Length"] = props.Length.ToString();
            properties["Uri"] = blob.Uri.ToString();

            return new StorageItemInfo(props.LastModifiedUtc, props.ETag, meta, properties);
        }

        static void ReadAndVerifyHash(Stream stream, Action<Stream> reader, string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                reader(stream);
                return;
            }

            var md5 = MD5.Create();
            // Blob streams throw NSE on Flush()
            using (var hack = new SuppressFlushForStream(stream))
            using (var crypto = new CryptoStream(hack, md5, CryptoStreamMode.Read))
            {
                reader(crypto);
            }
            var calculated = Convert.ToBase64String(md5.Hash);

            if (calculated != hash)
                throw new StorageItemIntegrityException("Hash was provided, but it does not match.");
        }


        public static void Read(BlobRequestOptions mapped, CloudBlob blob, ReaderDelegate reader)
        {
            blob.FetchAttributes(mapped);
            var props = MapFetchedAttrbitues(blob);

            var compression = blob.Properties.ContentEncoding ?? "";
            var md5 = blob.Metadata[LokadHashFieldName];

            switch (compression)
            {
                case "gzip":
                    using (var stream = blob.OpenRead(mapped))
                    {
                        ReadAndVerifyHash(stream, s =>
                            {
                                // important is not to flush the decompression stream
                                using (var decompress = new GZipStream(s, CompressionMode.Decompress, true))
                                {
                                    reader(props, decompress);
                                }
                            }, md5);
                    }

                    break;
                case "":
                    using (var stream = blob.OpenRead(mapped))
                    {
                        ReadAndVerifyHash(stream, s => reader(props, s), md5);
                    }
                    break;
                default:
                    var error = string.Format("Unsupported ContentEncoding '{0}'", compression);
                    throw new InvalidOperationException(error);
            }
        }

        public static long Write(BlobRequestOptions mapped, CloudBlob blob, Action<Stream> writer,
            StorageWriteOptions writeOptions)
        {
            var compressIfPossible = (writeOptions & StorageWriteOptions.CompressIfPossible) ==
                StorageWriteOptions.CompressIfPossible;

            // we are adding our own hashing on top, to ensure
            // consistent behavior between Azure StorageClient versions

            if (compressIfPossible)
            {
                return WriteWithCompression(blob, mapped, writer);
            }
            {
                return WriteWithoutCompression(blob, mapped, writer);
            }
        }


        static long WriteWithoutCompression(CloudBlob blob, BlobRequestOptions mapped, Action<Stream> writer)
        {
            var md5 = MD5.Create();
            long position;
            using (var stream = blob.OpenWrite(mapped))
            {
                using (var crypto = new CryptoStream(stream, md5, CryptoStreamMode.Write))
                {
                    writer(crypto);
                }

                position = stream.Position;
            }
            blob.Metadata[LokadHashFieldName] = Convert.ToBase64String(md5.Hash);
            blob.SetMetadata();
            return position;
        }

        static long WriteWithCompression(CloudBlob blob, BlobRequestOptions mapped, Action<Stream> writer)
        {
            long position;
            var md5 = MD5.Create();
            blob.Properties.ContentEncoding = "gzip";
            using (var stream = blob.OpenWrite(mapped))
            {
                using (var crypto = new CryptoStream(stream, md5, CryptoStreamMode.Write))
                using (var compress = new GZipStream(crypto, CompressionMode.Compress, true))
                {
                    writer(compress);
                }
                position = stream.Position;
            }
            blob.Metadata[LokadHashFieldName] = Convert.ToBase64String(md5.Hash);
            blob.SetMetadata();
            return position;
        }
    }
}