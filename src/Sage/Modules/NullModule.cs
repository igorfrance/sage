namespace Sage.Modules
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using log4net;
	using Sage.Views;

	public class NullModule : IModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(NullModule).FullName);

		public ModuleResult ProcessRequest(XmlElement moduleNode, ViewConfiguration configuration)
		{
			Contract.Requires<ArgumentNullException>(moduleNode != null);

			log.ErrorFormat("The module element '{0}' maps to '{1}'. This is probably a configuration error",
				moduleNode.Name, typeof(NullModule).Name);

			return new ModuleResult(moduleNode);
		}
	}
}
