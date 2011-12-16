namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	/// <summary>
	/// Defines the common interface for all modules.
	/// </summary>
	public interface IModule
	{
		ModuleResult ProcessRequest(XmlElement moduleNode, SageContext context);
	}
}
