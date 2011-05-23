using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.ListStorage
{
    public interface IListContainer
    {
        bool CreateTable();
        bool DeleteTable();

        IEnumerable<ListEntity> Get();
        IEnumerable<ListEntity> Get(string partitionKey);
        IEnumerable<ListEntity> Get(string partitionKey, string startRowKey, string endRowKey);
        IEnumerable<ListEntity> Get(string partitionKey, IEnumerable<string> rowKeys);

        void Insert(IEnumerable<ListEntity> entities);

        void Update(IEnumerable<ListEntity> entities, bool force);

        void Upsert(IEnumerable<ListEntity> entities);

        void Delete(string partitionKey, IEnumerable<string> rowKeys);
        void Delete(IEnumerable<ListEntity> entities, bool force);
    }
}