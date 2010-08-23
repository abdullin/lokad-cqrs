#region (c)2009-2010 Lokad - New BSD license

// Copyright (c) Lokad 2009-2010 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad.Messaging
{
	/// <summary>
	/// Message types for the <see cref="ICommunicator"/>
	/// </summary>
	public enum CommunicationType
	{
		/// <summary>
		/// 
		/// </summary>
		Normal,
		/// <summary>
		/// 
		/// </summary>
		Error,
		/// <summary>
		/// 
		/// </summary>
		Chat,
		/// <summary>
		/// 
		/// </summary>
		Groupchat,
		/// <summary>
		/// 
		/// </summary>
		Headline,
	}
}