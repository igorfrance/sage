namespace Sage.DevTools.Modules
{
	using System;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Sage.Modules;

	public class ViewInspectorModule : IModule
	{
		public ModuleResult ProcessRequest(XmlElement moduleElement, SageContext context)
		{
			ModuleResult result = new ModuleResult(moduleElement);
			XmlDocument ownerDoc = moduleElement.OwnerDocument;
			XmlElement dataElement = ownerDoc.CreateElement("mod:data", XmlNamespaces.ModulesNamespace);
			XmlElement metaElement = dataElement.AppendElement("mod:meta", XmlNamespaces.ModulesNamespace);

			foreach (string name in context.Config.MetaViews.Keys)
			{
				XmlElement viewElement = metaElement.AppendElement("mod:view", XmlNamespaces.ModulesNamespace);
				viewElement.InnerText = name;
			}

			result.AppendDataNode(dataElement);
			return result;
		}
	}
}
