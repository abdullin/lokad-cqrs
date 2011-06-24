#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;

namespace Sample_03.Worker
{
    public static class ConfigureNHibernate
    {
        public static NHibernate.Cfg.Configuration Build(string fileName)
        {
            return BuildConfig(fileName);
        }

        static NHibernate.Cfg.Configuration BuildConfig(string fileName)
        {
            // your automapping setup here
            var autoMap = AutoMap
                .AssemblyOf<AccountEntity>(type => type.Name.EndsWith("Entity"));

            var config = Fluently.Configure()
                // use SQLite file
                .Database(SQLiteConfiguration.Standard.UsingFile(fileName))
                // Generate automappings
                .Mappings(m => m.AutoMappings.Add(autoMap))
                // regenerate database on startup
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Execute(false, true, false));

            return config.BuildConfiguration();
        }
    }
}