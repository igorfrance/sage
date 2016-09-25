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
namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	using Kelp;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Contains the URL routing configuration settings.
	/// </summary>
	public class RoutingConfiguration : Dictionary<string, RouteInfo>, IXmlConvertible
	{
		/// <summary>
		/// The name of the controller to use if <see cref="DefaultController"/> was not set for this configuration object.
		/// </summary>
		public const string SystemDefaultController = "GenericController";

		/// <summary>
		/// The name of the action to use if <see cref="DefaultAction"/> was not set for this configuration object.
		/// </summary>
		public const string SystemDefaultAction = "Action";

		private string defaultController;
		private string defaultAction;

		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingConfiguration"/> class.
		/// </summary>
		public RoutingConfiguration()
		{
			this.DefaultNamespace = string.Empty;
		}

		internal RoutingConfiguration(RoutingConfiguration init)
			: base(init)
		{
		}

		/// <summary>
		/// Gets the default controller namespace for routes that don't define their namespace explicitly.
		/// </summary>
		public string DefaultNamespace { get; set; }

		/// <summary>
		/// Gets the name of the controller to which to route all views that don't have a specific controller defined.
		/// </summary>
		public string DefaultController
		{
			get
			{
				return defaultController ?? SystemDefaultController;
			}

			set
			{
				this.defaultController = !string.IsNullOrWhiteSpace(value) ? value : null;
			}
		}

		/// <summary>
		/// Gets the name of the view to which to route any views that don't have a specific controller defined.
		/// </summary>
		public string DefaultAction
		{
			get
			{
				return defaultAction ?? SystemDefaultAction;
			}

			set
			{
				this.defaultAction = !string.IsNullOrWhiteSpace(value) ? value : null;
			}
		}

		/// <inheritdoc/>
		public void Parse(XmlElement configElement)
		{
			if (configElement == null)
				throw new ArgumentNullException("configElement");

			this.DefaultNamespace = configElement.GetAttribute("defaultNamespace");
			this.DefaultController = configElement.GetAttribute("defaultController");
			this.DefaultAction = configElement.GetAttribute("defaultAction");

			foreach (XmlElement routeNode in configElement.SelectNodes("p:route", XmlNamespaces.Manager))
			{
				RouteInfo route = new RouteInfo(routeNode);
				this.Add(route.Name, route);
			}
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			const string Ns = XmlNamespaces.ProjectConfigurationNamespace;
			XmlElement result = document.CreateElement("routing", Ns);

			foreach (RouteInfo route in this.Values)
			{
				result.AppendChild(route.ToXml(document));
			}

			return result;
		}
	}
}
