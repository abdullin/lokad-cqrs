#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using Lokad.Serialization;

namespace Lokad
{
	/// <summary>
	/// Syntax extensions for <see cref="ISupportSyntaxForSerialization"/>
	/// </summary>
	public static class ExtendISupportSyntaxForSerialization
	{
		/// <summary>
		/// Uses the binary formatter.
		/// </summary>
		/// <param name="module">The module to extend.</param>
		public static void UseBinaryFormatter(this ISupportSyntaxForSerialization module)
		{
			module.RegisterSerializer<BinaryMessageSerializer>();
		}

		/// <summary>
		/// Uses the data contract serializer.
		/// </summary>
		/// <param name="module">The module to extend.</param>
		public static void UseDataContractSerializer(this ISupportSyntaxForSerialization module)
		{
			module.RegisterSerializer<DataContractMessageSerializer>();
		}
	}
}