namespace Sage.DevTools.Modules
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Sage.Modules;
	using Sage.Views;

	public class ViewInspectorModule : IModule
	{
		public ModuleResult ProcessRequest(XmlElement moduleElement, ViewConfiguration configuration)
		{
			Contract.Requires<ArgumentNullException>(moduleElement != null);
			Contract.Requires<ArgumentNullException>(configuration != null);

			ModuleResult result = new ModuleResult(moduleElement);
			XmlDocument ownerDoc = moduleElement.OwnerDocument;
			XmlElement dataElement = ownerDoc.CreateElement("mod:data", XmlNamespaces.ModulesNamespace);
			XmlElement metaElement = dataElement.AppendElement("mod:meta", XmlNamespaces.ModulesNamespace);

			SageContext context = configuration.Context;
			foreach (string name in context.ProjectConfiguration.MetaViews.Keys)
			{
				XmlElement viewElement = metaElement.AppendElement("mod:view", XmlNamespaces.ModulesNamespace);
				viewElement.InnerText = name;
			}

			result.AppendDataNode(dataElement);
			return result;
		}
	}
}
