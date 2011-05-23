using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.ListStorage
{
    /// <summary>Implementation based on the Table Storage of Windows Azure.</summary>
    public class AzureListContainer : IListContainer
    {
        // HACK: those tokens will probably be provided as constants in the StorageClient library
        const int MaxEntityTransactionCount = 100;

        // HACK: Lowering the maximal payload, to avoid corner cases #117 (ContentLengthExceeded)
        // [vermorel] 128kB is purely arbitrary, only taken as a reasonable safety margin
        const int MaxEntityTransactionPayload = 4 * 1024 * 1024 - 128 * 1024; // 4 MB - 128kB

        const string ContinuationNextRowKeyToken = "x-ms-continuation-NextRowKey";
        const string ContinuationNextPartitionKeyToken = "x-ms-continuation-NextPartitionKey";
        const string NextRowKeyToken = "NextRowKey";
        const string NextPartitionKeyToken = "NextPartitionKey";

        readonly CloudTableClient _tableStorage;
        public readonly string TableName;


        /// <summary>IoC constructor.</summary>
        public AzureListContainer(CloudTableClient tableStorage, string tableName)
        {
            _tableStorage = tableStorage;
            TableName = tableName;
        }

        public bool CreateTable()
        {
            return _tableStorage.CreateTableIfNotExist(TableName);
        }

        public bool DeleteTable()
        {
            return _tableStorage.DeleteTableIfExist(TableName);
        }

 
        public IEnumerable<ListEntity> Get()
        {
            var context = _tableStorage.GetDataServiceContext();
            return GetInternal(context, Optional<string>.Empty);
        }

        public IEnumerable<ListEntity> Get(string partitionKey)
        {
            if (null == partitionKey) throw new ArgumentNullException("partitionKey");
            if (partitionKey.Contains("'"))
                throw new ArgumentOutOfRangeException("partitionKey", "Incorrect char in partitionKey.");

            var filter = string.Format("(PartitionKey eq '{0}')", HttpUtility.UrlEncode(partitionKey));

            var context = _tableStorage.GetDataServiceContext();
            return GetInternal(context, filter);
        }

        public IEnumerable<ListEntity> Get(string partitionKey, string startRowKey, string endRowKey)
        {
            if (null == TableName) throw new ArgumentNullException("tableName");
            if (null == partitionKey) throw new ArgumentNullException("partitionKey");
            if (partitionKey.Contains("'"))
                throw new ArgumentOutOfRangeException("partitionKey", "Incorrect char.");
            if (startRowKey != null && startRowKey.Contains("'"))
                throw new ArgumentOutOfRangeException("startRowKey", "Incorrect char.");
            if (endRowKey != null && endRowKey.Contains("'"))
                throw new ArgumentOutOfRangeException("endRowKey", "Incorrect char.");

            var filter = string.Format("(PartitionKey eq '{0}')", HttpUtility.UrlEncode(partitionKey));

            // optional starting range constraint
            if (!string.IsNullOrEmpty(startRowKey))
            {
                // ge = GreaterThanOrEqual (inclusive)
                filter += string.Format(" and (RowKey ge '{0}')", HttpUtility.UrlEncode(startRowKey));
            }

            if (!string.IsNullOrEmpty(endRowKey))
            {
                // lt = LessThan (exclusive)
                filter += string.Format(" and (RowKey lt '{0}')", HttpUtility.UrlEncode(endRowKey));
            }

            var context = _tableStorage.GetDataServiceContext();
            return GetInternal(context, filter);
        }

        public IEnumerable<ListEntity> Get(string partitionKey, IEnumerable<string> rowKeys)
        {
            if (null == partitionKey) throw new ArgumentNullException("partitionKey");
            if (partitionKey.Contains("'")) throw new ArgumentOutOfRangeException("partitionKey", "Incorrect char.");

            var context = _tableStorage.GetDataServiceContext();

            foreach (var slice in Slice(rowKeys, MaxEntityTransactionCount))
            {
                // work-around the limitation of ADO.NET that does not provide a native way
                // of query a set of specified entities directly.
                var builder = new StringBuilder();
                builder.Append(string.Format("(PartitionKey eq '{0}') and (", HttpUtility.UrlEncode(partitionKey)));
                for (int i = 0; i < slice.Length; i++)
                {
                    // in order to avoid SQL-injection-like problems 
                    if (slice[i].Contains("'")) throw new ArgumentOutOfRangeException("rowKeys", "Incorrect char.");

                    builder.Append(string.Format("(RowKey eq '{0}')", HttpUtility.UrlEncode(slice[i])));
                    if (i < slice.Length - 1)
                    {
                        builder.Append(" or ");
                    }
                }
                builder.Append(")");

                foreach (var entity in GetInternal(context, builder.ToString()))
                {
                    yield return entity;
                }
            }
        }

        private IEnumerable<ListEntity> GetInternal(TableServiceContext context, Optional<string> filter)
        {
            string continuationRowKey = null;
            string continuationPartitionKey = null;

            context.MergeOption = MergeOption.AppendOnly;
            context.ResolveType = ResolveFatEntityType;

            do
            {
                var query = context.CreateQuery<AzureListEntity>(TableName);

                if (filter.HasValue)
                {
                    query = query.AddQueryOption("$filter", filter.Value);
                }

                if (null != continuationRowKey)
                {
                    query = query.AddQueryOption(NextRowKeyToken, continuationRowKey)
                        .AddQueryOption(NextPartitionKeyToken, continuationPartitionKey);
                }

                QueryOperationResponse response = null;
                AzureListEntity[] fatEntities = null;

                try
                {
                    response = query.Execute() as QueryOperationResponse;
                    fatEntities = ((IEnumerable<AzureListEntity>)response).ToArray();
                }
                catch (DataServiceQueryException ex)
                {
                    // if the table does not exist, there is nothing to return
                    var errorCode = TableStoragePolicies.GetErrorCode(ex);
                    if (TableErrorCodeStrings.TableNotFound == errorCode
                        || StorageErrorCodeStrings.ResourceNotFound == errorCode)
                    {
                        fatEntities = new AzureListEntity[0];
                        
                    }
                    else
                    {
                        throw;
                    }
                }


                foreach (var fatEntity in fatEntities)
                {
                    var etag = context.Entities.First(e => e.Entity == fatEntity).ETag;
                    context.Detach(fatEntity);
                    yield return AzureListEntity.Convert(fatEntity, etag);
                }

                Debug.Assert(context.Entities.Count == 0);

                if (null != response && response.Headers.ContainsKey(ContinuationNextRowKeyToken))
                {
                    continuationRowKey = response.Headers[ContinuationNextRowKeyToken];
                    continuationPartitionKey = response.Headers[ContinuationNextPartitionKeyToken];

                }
                else
                {
                    continuationRowKey = null;
                    continuationPartitionKey = null;
                }

            } while (null != continuationRowKey);
        }

        public void Insert(IEnumerable<ListEntity> entities)
        {
            foreach (var g in entities.GroupBy(e => e.PartitionKey))
            {
                InsertInternal(g);
            }
        }

        void InsertInternal(IEnumerable<ListEntity> entities)
        {
            var context = _tableStorage.GetDataServiceContext();
            context.MergeOption = MergeOption.AppendOnly;
            context.ResolveType = ResolveFatEntityType;

            var fatEntities = entities.Select(e => Tuple.Create(AzureListEntity.Convert(e), e));

            var noBatchMode = false;

            foreach (var slice in SliceEntities(fatEntities, e => e.Item1.GetPayload()))
            {

                var cloudEntityOfFatEntity = new Dictionary<object, ListEntity>();
                foreach (var fatEntity in slice)
                {
                    context.AddObject(TableName, fatEntity.Item1);
                    cloudEntityOfFatEntity.Add(fatEntity.Item1, fatEntity.Item2);
                }

                try
                {
                    // HACK: nested try/catch
                    try
                    {
                        context.SaveChanges(noBatchMode ? SaveChangesOptions.None : SaveChangesOptions.Batch);
                        ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                    }
                    // special casing the need for table instantiation
                    catch (DataServiceRequestException ex)
                    {
                        var errorCode = TableStoragePolicies.GetErrorCode(ex);
                        if (errorCode == TableErrorCodeStrings.TableNotFound
                            || errorCode == StorageErrorCodeStrings.ResourceNotFound)
                        {
                            try
                            {
                                _tableStorage.CreateTableIfNotExist(TableName);
                            }
                            // HACK: incorrect behavior of the StorageClient (2010-09)
                            // Fails to behave properly in multi-threaded situations
                            catch (StorageClientException cex)
                            {
                                if (cex.ExtendedErrorInformation == null
                                    || cex.ExtendedErrorInformation.ErrorCode != TableErrorCodeStrings.TableAlreadyExists)
                                {
                                    throw;
                                }
                            }
                            context.SaveChanges(noBatchMode ? SaveChangesOptions.None : SaveChangesOptions.Batch);
                            ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (DataServiceRequestException ex)
                {
                    var errorCode = TableStoragePolicies.GetErrorCode(ex);

                    if (errorCode == StorageErrorCodeStrings.OperationTimedOut)
                    {
                        // if batch does not work, then split into elementary requests
                        // PERF: it would be better to split the request in two and retry
                        context.SaveChanges();
                        ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                        noBatchMode = true;
                    }
                    // HACK: undocumented code returned by the Table Storage
                    else if (errorCode == "ContentLengthExceeded")
                    {
                        context.SaveChanges();
                        ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                        noBatchMode = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DataServiceQueryException ex)
                {
                    // HACK: code dupplicated

                    var errorCode = TableStoragePolicies.GetErrorCode(ex);

                    if (errorCode == StorageErrorCodeStrings.OperationTimedOut)
                    {
                        // if batch does not work, then split into elementary requests
                        // PERF: it would be better to split the request in two and retry
                        context.SaveChanges();
                        ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                        noBatchMode = true;
                    }
                    else
                    {
                        throw;
                    }
                }


            }
        }

        public void Update(IEnumerable<ListEntity> entities, bool force)
        {
            foreach (var g in entities.GroupBy(e => e.PartitionKey))
            {
                UpdateInternal(g, force);
            }
        }

        void UpdateInternal(IEnumerable<ListEntity> entities, bool force)
        {
            var context = _tableStorage.GetDataServiceContext();
            context.MergeOption = MergeOption.AppendOnly;
            context.ResolveType = ResolveFatEntityType;

            var fatEntities = entities.Select(e => Tuple.Create(AzureListEntity.Convert(e), e));

            var noBatchMode = false;

            foreach (var slice in SliceEntities(fatEntities, e => e.Item1.GetPayload()))
            {

                var cloudEntityOfFatEntity = new Dictionary<object, ListEntity>();
                foreach (var fatEntity in slice)
                {
                    // entities should be updated in a single round-trip
                    context.AttachTo(TableName, fatEntity.Item1, MapETag(fatEntity.Item2.ETag, force));
                    context.UpdateObject(fatEntity.Item1);
                    cloudEntityOfFatEntity.Add(fatEntity.Item1, fatEntity.Item2);
                }

                try
                {
                    context.SaveChanges(noBatchMode ? SaveChangesOptions.None : SaveChangesOptions.Batch);
                    ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                }
                catch (DataServiceRequestException ex)
                {
                    var errorCode = TableStoragePolicies.GetErrorCode(ex);

                    if (errorCode == StorageErrorCodeStrings.OperationTimedOut)
                    {
                        // if batch does not work, then split into elementary requests
                        // PERF: it would be better to split the request in two and retry
                        context.SaveChanges();
                        ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                        noBatchMode = true;
                    }
                    else if (errorCode == TableErrorCodeStrings.TableNotFound)
                    {
                        try
                        {
                            _tableStorage.CreateTableIfNotExist(TableName);
                        }
                        // HACK: incorrect behavior of the StorageClient (2010-09)
                        // Fails to behave properly in multi-threaded situations
                        catch (StorageClientException cex)
                        {
                            if (cex.ExtendedErrorInformation.ErrorCode != TableErrorCodeStrings.TableAlreadyExists)
                            {
                                throw;
                            }
                        }
                        context.SaveChanges(noBatchMode ? SaveChangesOptions.None : SaveChangesOptions.Batch);
                        ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DataServiceQueryException ex)
                {
                    // HACK: code dupplicated

                    var errorCode = TableStoragePolicies.GetErrorCode(ex);

                    if (errorCode == StorageErrorCodeStrings.OperationTimedOut)
                    {
                        // if batch does not work, then split into elementary requests
                        // PERF: it would be better to split the request in two and retry
                        context.SaveChanges();
                        ReadETagsAndDetach(context, (entity, etag) => cloudEntityOfFatEntity[entity].ETag = etag);
                        noBatchMode = true;
                    }
                    else
                    {
                        throw;
                    }
                }

            }
        }

        public void Upsert(IEnumerable<ListEntity> entities)
        {
            foreach (var g in entities.GroupBy(e => e.PartitionKey))
            {
                UpsertInternal(g);
            }
        }

        // HACK: no 'upsert' (update or insert) available at the time
        // http://social.msdn.microsoft.com/Forums/en-US/windowsazure/thread/4b902237-7cfb-4d48-941b-4802864fc274

        /// <remarks>Upsert is making several storage calls to emulate the 
        /// missing semantic from the Table Storage.</remarks>
        void UpsertInternal(IEnumerable<ListEntity> entities)
        {
            // checking for entities that already exist
            var partitionKey = entities.First().PartitionKey;
            var existingKeys = new HashSet<string>(
                Get(partitionKey, entities.Select(e => e.RowKey)).Select(e => e.RowKey));

            // inserting or updating depending on the presence of the keys
            Insert(entities.Where(e => !existingKeys.Contains(e.RowKey)));
            Update(entities.Where(e => existingKeys.Contains(e.RowKey)), true);
        }

        /// <summary>Slice entities according the payload limitation of
        /// the transaction group, plus the maximal number of entities to
        /// be embedded into a single transaction.</summary>
        static IEnumerable<T[]> SliceEntities<T>(IEnumerable<T> entities, Func<T, int> getPayload)
        {
            var accumulator = new List<T>(100);
            var payload = 0;
            foreach (var entity in entities)
            {
                var entityPayLoad = getPayload(entity);

                if (accumulator.Count >= MaxEntityTransactionCount ||
                    payload + entityPayLoad >= MaxEntityTransactionPayload)
                {
                    yield return accumulator.ToArray();
                    accumulator.Clear();
                    payload = 0;
                }

                accumulator.Add(entity);
                payload += entityPayLoad;
            }

            if (accumulator.Count > 0)
            {
                yield return accumulator.ToArray();
            }
        }

        public void Delete(string partitionKey, IEnumerable<string> rowKeys)
        {
            DeleteInternal(partitionKey, rowKeys.Select(k => Tuple.Create(k, "*")), true);
        }

        public void Delete(IEnumerable<ListEntity> entities, bool force)
        {
            foreach (var g in entities.GroupBy(e => e.PartitionKey))
            {
                DeleteInternal(g.Key, g.Select(e => Tuple.Create(e.RowKey, MapETag(e.ETag, force))), force);
            }
        }

        void DeleteInternal(string partitionKey, IEnumerable<Tuple<string, string>> rowKeysAndETags, bool force)
        {
            var context = _tableStorage.GetDataServiceContext();

            // CAUTION: make sure to get rid of potential duplicate in rowkeys.
            // (otherwise insertion in 'context' is likely to fail)
            foreach (var s in Slice(rowKeysAndETags
                // Similar effect than 'Distinct' based on 'RowKey'
                                    .ToLookup(p => p.Item1, p => p).Select(g => g.First()),
                                    MaxEntityTransactionCount))
            {
                var slice = s;

            DeletionStart: // 'slice' might have been refreshed if some entities were already deleted

                foreach (var rowKeyAndETag in slice)
                {
                    // Deleting entities in 1 roundtrip
                    // http://blog.smarx.com/posts/deleting-entities-from-windows-azure-without-querying-first
                    var mock = new AzureListEntity
                    {
                        PartitionKey = partitionKey,
                        RowKey = rowKeyAndETag.Item1
                    };

                    context.AttachTo(TableName, mock, rowKeyAndETag.Item2);
                    context.DeleteObject(mock);

                }

                try // HACK: [vermorel] if a single entity is missing, then the whole batch operation is aborded
                {

                    try // HACK: nested try/catch to handle the special case where the table is missing
                    {
                        context.SaveChanges(SaveChangesOptions.Batch);
                    }
                    catch (DataServiceRequestException ex)
                    {
                        // if the table is missing, no need to go on with the deletion
                        var errorCode = TableStoragePolicies.GetErrorCode(ex);
                        if (TableErrorCodeStrings.TableNotFound == errorCode)
                        {
                            return;
                        }

                        throw;
                    }
                }
                // if some entities exist
                catch (DataServiceRequestException ex)
                {
                    var errorCode = TableStoragePolicies.GetErrorCode(ex);

                    // HACK: Table Storage both implement a bizarre non-idempotent semantic
                    // but in addition, it throws a non-documented exception as well. 
                    if (errorCode != "ResourceNotFound")
                    {
                        throw;
                    }

                    slice = Get(partitionKey, slice.Select(p => p.Item1))
                        .Select(e => Tuple.Create(e.RowKey, MapETag(e.ETag, force))).ToArray();

                    // entities with same name will be added again
                    context = _tableStorage.GetDataServiceContext();

                    // HACK: [vermorel] yes, gotos are horrid, but other solutions are worst here.
                    goto DeletionStart;
                }

            }
        }

        static Type ResolveFatEntityType(string name)
        {
            return typeof(AzureListEntity);
        }

        static string MapETag(string etag, bool force)
        {
            return force || string.IsNullOrEmpty(etag)
                ? "*"
                : etag;
        }

        static void ReadETagsAndDetach(DataServiceContext context, Action<object, string> write)
        {
            foreach (var entity in context.Entities)
            {
                write(entity.Entity, entity.ETag);
                context.Detach(entity.Entity);
            }
        }

        /// <summary>
        /// Performs lazy splitting of the provided collection into collections of <paramref name="sliceLength"/>
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="sliceLength">Maximum length of the slice.</param>
        /// <returns>lazy enumerator of the collection of arrays</returns>
        public static IEnumerable<TItem[]> Slice<TItem>(IEnumerable<TItem> source, int sliceLength)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (sliceLength <= 0)
                throw new ArgumentOutOfRangeException("sliceLength", "value must be greater than 0");

            var list = new List<TItem>(sliceLength);
            foreach (var item in source)
            {
                list.Add(item);
                if (sliceLength == list.Count)
                {
                    yield return list.ToArray();
                    list.Clear();
                }
            }

            if (list.Count > 0)
                yield return list.ToArray();
        }
    }
}
