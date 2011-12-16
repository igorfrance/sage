namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml;

	public class NullModule : IModule
	{
		public ModuleResult ProcessRequest(XmlElement moduleNode, SageContext context)
		{
			return new ModuleResult(ModuleResultStatus.Ok);
		}
	}
}
