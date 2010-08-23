#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Serialization
{
	/// <summary>
	/// Provides collection of known serialization types (for prebuilt serializers)
	/// </summary>
	public interface IKnowSerializationTypes
	{
		/// <summary>
		/// Gets the known serialization types.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Type> GetKnownTypes();
	}
}