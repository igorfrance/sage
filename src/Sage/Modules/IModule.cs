namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	using Sage.Views;

	/// <summary>
	/// Defines the common interface for all modules.
	/// </summary>
	public interface IModule
	{
		ModuleResult ProcessRequest(XmlElement moduleNode, ViewConfiguration configuration);
	}
}
