/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sage.DevTools.Modules
{
	using System;
	using System.Xml;

	using Kelp.Extensions;

	using Sage.Configuration;
	using Sage.Modules;
	using Sage.Views;

	public class DeveloperToolbarModule : IModule
	{
		public ModuleResult ProcessElement(XmlElement moduleElement, ViewConfiguration configuration)
		{
			SageContext context = configuration.Context;
			if (!context.IsDeveloperRequest)
				return null;

			ModuleResult result = new ModuleResult(moduleElement);
			XmlDocument ownerDoc = moduleElement.OwnerDocument;
			XmlElement dataElement = ownerDoc.CreateElement("mod:data", XmlNamespaces.ModulesNamespace);
			XmlElement metaElement = dataElement.AppendElement("mod:meta", XmlNamespaces.ModulesNamespace);

			foreach (MetaViewInfo info in context.ProjectConfiguration.MetaViews.Values)
			{
				XmlElement viewElement = metaElement.AppendElement("mod:view", XmlNamespaces.ModulesNamespace);
				viewElement.SetAttribute("name", info.Name);
				viewElement.InnerText = info.Description;
			}

			result.AppendDataNode(dataElement);
			return result;
		}
	}
}
