#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using Lokad;
using Lokad.Cqrs;
using Lokad.Cqrs.Default;

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