using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.ListStorage
{
    /// <summary>This entity is basically a workaround the 64KB limitation
    /// for entity properties. 15 properties represents a total storage
    /// capability of 960KB (entity limit is at 1024KB).</summary>
    /// <remarks>This class is basically a hack against the Table Storage
    /// to work-around the 64KB limitation for properties.</remarks>
    class AzureListEntity : TableServiceEntity
    {
        /// <summary>
        /// Maximal entity size is 1MB. Out of that, we keep only
        /// 960kb (1MB - 64kb as a safety margin). Then, it should be taken
        /// into account that byte[] are Base64 encoded which represent
        /// a penalty overhead of 4/3 - hence the reduced capacity.
        /// </summary>
        public const int MaxByteCapacity = (960 * 1024 * 3) / 4;

        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Global
        public byte[] P0 { get; set; }
        public byte[] P1 { get; set; }
        public byte[] P2 { get; set; }
        public byte[] P3 { get; set; }
        public byte[] P4 { get; set; }
        public byte[] P5 { get; set; }
        public byte[] P6 { get; set; }
        public byte[] P7 { get; set; }
        public byte[] P8 { get; set; }
        public byte[] P9 { get; set; }
        public byte[] P10 { get; set; }
        public byte[] P11 { get; set; }
        public byte[] P12 { get; set; }
        public byte[] P13 { get; set; }
        public byte[] P14 { get; set; }

        IEnumerable<byte[]> GetProperties()
        {
            if (null != P0) yield return P0;
            if (null != P1) yield return P1;
            if (null != P2) yield return P2;
            if (null != P3) yield return P3;
            if (null != P4) yield return P4;
            if (null != P5) yield return P5;
            if (null != P6) yield return P6;
            if (null != P7) yield return P7;
            if (null != P8) yield return P8;
            if (null != P9) yield return P9;
            if (null != P10) yield return P10;
            if (null != P11) yield return P11;
            if (null != P12) yield return P12;
            if (null != P13) yield return P13;
            if (null != P14) yield return P14;
        }

        public byte[] GetData()
        {
            var arrays = GetProperties().ToArray();
            var buffer = new byte[arrays.Sum(a => a.Length)];

            var i = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, buffer, i, array.Length);
                i += array.Length;
            }

            return buffer;
        }

        public void SetData(byte[] data)
        {
            if (null == data) throw new ArgumentNullException("data");
            if (data.Length >= MaxByteCapacity) throw new ArgumentOutOfRangeException("data");

            var setters = new Action<byte[]>[]
                {
                    b => P0 = b,
                    b => P1 = b,
                    b => P2 = b,
                    b => P3 = b,
                    b => P4 = b,
                    b => P5 = b,
                    b => P6 = b,
                    b => P7 = b,
                    b => P8 = b,
                    b => P9 = b,
                    b => P10 = b,
                    b => P11 = b,
                    b => P12 = b,
                    b => P13 = b,
                    b => P14 = b,
                };

            for (int i = 0; i < 15; i++)
            {
                if (i * 64 * 1024 < data.Length)
                {
                    var start = i * 64 * 1024;
                    var length = Math.Min(64 * 1024, data.Length - start);
                    var buffer = new byte[length];

                    Buffer.BlockCopy(data, start, buffer, 0, buffer.Length);
                    setters[i](buffer);
                }
                else
                {
                    setters[i](null); // discarding potential leftover
                }
            }
        }

        /// <summary>Returns an upper bound approximation of the payload associated to
        /// the entity once serialized as XML Atom (used for communication with the
        /// Table Storage).</summary>
        public int GetPayload()
        {
            // measurements indicates overhead is closer to 1300 chars, but we take a bit of margin
            const int envelopOverhead = 1500;

            // Caution: there is a loss when converting byte[] to Base64 representation
            var binCharCount = (GetProperties().Sum(a => a.Length) * 4 + 3) / 3;
            var partitionKeyCount = PartitionKey.Length;
            var rowKeyCount = RowKey.Length;
            // timestamp is already accounted for in the envelop overhead.

            return binCharCount + partitionKeyCount + rowKeyCount + envelopOverhead;
        }

        /// <summary>Converts a <c>FatEntity</c> toward a <c>CloudEntity</c>.</summary>
        public static ListEntity Convert(AzureListEntity fatEntity, string etag)
        {
            return new ListEntity
                {
                    PartitionKey = fatEntity.PartitionKey,
                    RowKey = fatEntity.RowKey,
                    Timestamp = fatEntity.Timestamp,
                    ETag = etag,
                    Value = fatEntity.GetData()
                };
        }

        /// <summary>Converts a <c>CloudEntity</c> toward a <c>FatEntity</c>.</summary>
        public static AzureListEntity Convert(ListEntity listEntity)
        {
            var fatEntity = new AzureListEntity
                {
                    PartitionKey = listEntity.PartitionKey,
                    RowKey = listEntity.RowKey,
                    Timestamp = listEntity.Timestamp
                
                };

            fatEntity.SetData(listEntity.Value);
            return fatEntity;
        }
    }
}