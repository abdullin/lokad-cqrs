#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using Lokad;
using Lokad.Cqrs;
using Lokad.Default;

namespace Sample_05.Web
{
	public static class AzureViews
	{
		public static Maybe<TView> Get<TView>(object identity)
			where TView : IEntity
		{
			return GlobalSetup.Views.Read<TView>(identity);
		}
	}
}