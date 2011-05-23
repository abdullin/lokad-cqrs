using System;

namespace Lokad.Cqrs.Feature.ListStorage
{
    /// <summary>Entity to be stored by the <see cref="IListContainer"/>.</summary>
    /// <remarks>Once serialized the <c>CloudEntity.Value</c> should weight less
    /// than 720KB to be compatible with Table Storage limitations on entities.</remarks>
    public class ListEntity
    {
        /// <summary>Indexed system property.</summary>
        public string RowKey { get; set; }

        /// <summary>Indexed system property.</summary>
        public string PartitionKey { get; set; }

        /// <summary>Flag indicating last update. Populated by the Table Storage.</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>ETag. Indicates changes. Populated by the Table Storage.</summary>
        public string ETag { get; set; }

        /// <summary>Value carried by the entity.</summary>
        public byte[] Value { get; set; }
    }
}