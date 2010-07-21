using System;
using System.Data;
using System.Data.SqlClient;
using Lokad.Quality;
using Lokad.Serialization;

namespace Lokad.Cqrs.Views.Sql
{
	/// <summary>
	/// Simple query engine for views stored in a single SQL table
	/// </summary> 
	[UsedImplicitly]
	public sealed class QuerySqlViews : IQueryViews<SqlViewQuery>
	{
		readonly Func<SqlConnection> _factory;
		readonly IsolationLevel _isolationLevel;
		readonly IDataSerializer _serializer;

		public QuerySqlViews(Func<SqlConnection> factory, IsolationLevel isolationLevel, IDataSerializer serializer)
		{
			_factory = factory;
			_isolationLevel = isolationLevel;
			_serializer = serializer;
		}

		public QuerySqlViews(Func<SqlConnection> factory)
		{
			_factory = factory;
		}

	
		public Maybe<object> Load(Type type, string partition, string identity)
		{
			var result = Maybe<object>.Empty;
			var q = new SqlViewQuery(1, partition, new IndexQuery(QueryViewOperand.Equal, identity));
			List(type, q, a => result = a);
			return result;
		}


		public Maybe<TView> Load<TView>(string partition, string identity)
		{
			return Load(typeof(TView), partition, identity).Convert(t => (TView)t);
		}

		public void List<TView>(SqlViewQuery query, Action<ViewEntity<TView>> process)
		{
			List(typeof(TView), query, o => process(new ViewEntity<TView>(o.Partition, o.Identity, (TView)o.Value)));
		}

		public void List(Type type, SqlViewQuery query, Action<ViewEntity> process)
		{
			using (var conn = _factory())
			{
				conn.Open();
				using (var tx = conn.BeginTransaction(_isolationLevel))
				{
					using (var cmd = new SqlCommand("", conn, tx))
					{
						SqlViewDialect.ReadList(cmd, query, (s,p,i) =>
							{
								var o = _serializer.Deserialize(s, type);
								process(new ViewEntity(p,i,o));
							});
					}
				}
			}
		}
	}
}