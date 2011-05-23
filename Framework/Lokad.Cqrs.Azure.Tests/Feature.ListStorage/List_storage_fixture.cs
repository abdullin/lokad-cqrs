using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Feature.ListStorage
{
    
    public abstract class List_storage_fixture
    {
        
        protected AzureListContainer Container = new AzureListContainer(AzureStorage.CreateConfigurationForDev().CreateTableClient(), "test-table");


        ListEntity[] Entities(int count, string partitionKey, int entitySize)
        {
            return EntitiesInternal(count, partitionKey, entitySize).ToArray();
        }

        IEnumerable<ListEntity> EntitiesInternal(int count, string partitionKey, int entitySize)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new ListEntity()
                {
                    PartitionKey = partitionKey,
                    RowKey = Guid.NewGuid().ToString(),
                    Value = RandomBuffer(entitySize)
                };
            }
        }

        public static byte[] RandomBuffer(int size)
        {
            var buffer = new byte[size];
            new Random().NextBytes(buffer);
            return buffer;
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Container.CreateTable();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Container.DeleteTable();
        }
    }

    [TestFixture]
    public sealed class When_managing_table
    {
        // ReSharper disable InconsistentNaming

        //[Test]
        //public void CreateDeleteTables()
        //{
        //    var name = "n" + Guid.NewGuid().ToString("N");
        //    Assert.IsTrue(_tableStorage.CreateTable(name), "#A01");
        //    Assert.IsFalse(_tableStorage.CreateTable(name), "#A02");
        //    Assert.IsTrue(_tableStorage.DeleteTable(name), "#A03");

        //    // replicating the test a 2nd time, to check for slow table deletion
        //    Assert.IsTrue(_tableStorage.CreateTable(name), "#A04");
        //    Assert.IsTrue(_tableStorage.DeleteTable(name), "#A05");

        //    Assert.IsFalse(_tableStorage.DeleteTable(name), "#A06");

        //    const string name2 = "IamNotATable";
        //    Assert.IsFalse(_tableStorage.DeleteTable(name2), "#A07");
        //}
        
    }
}