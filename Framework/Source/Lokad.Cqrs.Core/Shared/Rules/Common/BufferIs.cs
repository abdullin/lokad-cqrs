#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad.Rules
{
	/// <summary>
	/// Validation rules for byte arrays
	/// </summary>
	public static class BufferIs
	{
		/// <summary>
		/// Composes validator ensuring that size of the buffer 
		/// is equal or less to the speicifed limit 
		/// </summary>
		/// <param name="length">The length.</param>
		/// <returns>new rule validator instance</returns>
		public static Rule<byte[]> Limited(int length)
		{
			return (bytes, scope) =>
				{
					if (bytes.Length > length)
					{
						scope.Error(RuleResources.Buffer_cant_be_longer_than_X, length);
					}
				};
		}

		/// <summary>
		/// Composes validator ensuring that buffer has valid hash
		/// </summary>
		/// <param name="hash">The hash.</param>
		/// <returns>new instance of the rule validato</returns>
		/// <seealso cref="BufferUtil.CalculateSimpleHashCode"/>
		public static Rule<byte[]> WithValidHash(int hash)
		{
			return (bytes, scope) =>
				{
					var result = BufferUtil.CalculateSimpleHashCode(bytes);
					if (result != hash)
					{
						scope.Error(RuleResources.Buffer_must_have_valid_hash);
					}
				};
		}
	}
}