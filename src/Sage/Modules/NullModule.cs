namespace Sage.Modules
{
	using System;
	using System.Xml;

	public class NullModule : IModule
	{
		public ModuleResult ProcessRequest(XmlElement moduleNode, SageContext context)
		{
			// The code should never hit here
			throw new NotImplementedException();
		}
	}
}
