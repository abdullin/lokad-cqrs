using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.ListStorage
{
    public interface IListContainer
    {
        bool CreateTable(string tableName);
        bool DeleteTable(string tableName);

        IEnumerable<string> GetTables();

        IEnumerable<ListEntity> Get(string tableName);
        IEnumerable<ListEntity> Get(string tableName, string partitionKey);
        IEnumerable<ListEntity> Get(string tableName, string partitionKey, string startRowKey, string endRowKey);
        IEnumerable<ListEntity> Get(string tableName, string partitionKey, IEnumerable<string> rowKeys);

        void Insert(string tableName, IEnumerable<ListEntity> entities);

        void Update(string tableName, IEnumerable<ListEntity> entities, bool force);

        void Upsert(string tableName, IEnumerable<ListEntity> entities);

        void Delete(string tableName, string partitionKey, IEnumerable<string> rowKeys);
        void Delete(string tableName, IEnumerable<ListEntity> entities, bool force);
    }
}