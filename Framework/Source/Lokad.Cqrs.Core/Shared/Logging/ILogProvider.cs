#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

namespace Lokad
{
	/// <summary>
	/// Creates logs using the name
	/// </summary>
	public interface ILogProvider : INamedProvider<ILog>
	{
	}
}