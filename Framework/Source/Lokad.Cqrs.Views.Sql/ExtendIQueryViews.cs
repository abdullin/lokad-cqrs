using System;
using System.Collections.Generic;
using Lokad.Cqrs.Views.Sql;

namespace Lokad.Cqrs
{
	public static class ExtendIQueryViews
	{

		//Maybe<object> Load(Type type, string partition, string identity);
		//Maybe<TView> Load<TView>(string partition, string identity);
		//void List<TView>(string partition, Maybe<ViewQuery> query, Action<ViewEntity<TView>> process);

		public static void Query<TView>(this IQueryViews self, string partition, Action<ViewEntity<TView>> process)
		{
			self.Query(typeof(TView), partition, ViewQuery.Empty, ve => process(ve.Cast<TView>()));
		}

		public static void Query<TView>(this IQueryViews self, string partition, ViewQuery query, Action<ViewEntity<TView>> process)
		{
			self.Query(typeof(TView), partition, query, ve => process(ve.Cast<TView>()));
		}

		public static Maybe<TView> Load<TView>(this IQueryViews self, string partition, string identity)
		{
			var result = Maybe<TView>.Empty;
			var q = new ViewQuery(1, new IndexQuery(QueryViewOperand.Equal, identity));
			self.Query(typeof(TView), partition, q, v => result = (TView)v.Value);
			return result;
		}

		public static ICollection<ViewEntity<TView>> List<TView>(this IQueryViews self, string partition)
		{
			return self.List<TView>(partition, ViewQuery.Empty);
		}

		public static ICollection<ViewEntity<TView>> List<TView>(this IQueryViews self, string partition, ViewQuery query)
		{
			var list = new List<ViewEntity<TView>>();
			self.Query<TView>(partition, query, list.Add);
			return list.AsReadOnly();
		}
	}
}