#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using NUnit.Framework;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    [TestFixture]
    public sealed class When_DefaultAzureAtomicStorageStrategy_is_used
    {
        // ReSharper disable InconsistentNaming


        static IAzureAtomicStorageStrategy Default = new DefaultAzureAtomicStorageStrategyBuilder().Build();

        [Test]
        public void Valid_name_for_type_with_underscore()
        {
            var type = typeof (Nested_Underscore);
            var key = "000-00A";
            Assert.AreEqual("atomic-nested-underscore", Default.GetFolderForEntity(type));
            Assert.AreEqual("nested-underscore-000-00a.pb", Default.GetNameForEntity(type, key));

            Assert.AreEqual("nested-underscore.pb", Default.GetNameForSingleton(type));
        }


        [Test]
        public void Valid_name_for_nested_entity()
        {
            var entity = typeof (NestedAComposite);
            var key = "000-00A";

            Assert.AreEqual("atomic-nested-acomposite", Default.GetFolderForEntity(entity));
            Assert.AreEqual("nested-acomposite-000-00a.pb", Default.GetNameForEntity(entity, key));
            Assert.AreEqual("nested-acomposite.pb", Default.GetNameForSingleton(entity));
        }


        [Test]
        public void Valid_name_for_key()
        {
            var name = Default.GetNameForEntity(typeof (NestedAComposite), "Abs-23");
            Assert.AreEqual("nested-acomposite-abs-23.pb", name);
        }

        public sealed class NestedAComposite {}

        public sealed class Nested_Underscore {}
    }
}