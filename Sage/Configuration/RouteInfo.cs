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
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Contains configuration information about a single URL route.
	/// </summary>
	public class RouteInfo : IXmlConvertible
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RouteInfo"/> class, using the specified 
		/// <paramref name="configurationElement"/>.
		/// </summary>
		/// <param name="configurationElement">The route configuration element's.</param>
		/// <exception cref="ArgumentNullException"><c>routeConfig</c> is null.</exception>
		public RouteInfo(XmlElement configurationElement)
		{
			Contract.Requires<ArgumentNullException>(configurationElement != null);

			this.Parse(configurationElement);
		}

		/// <summary>
		/// Gets the name of this route.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the namespace of this controller that handles this route.
		/// </summary>
		public string Namespace { get; private set; }

		/// <summary>
		/// Optional name of extension that defines this route.
		/// </summary>
		public string Extension { get; internal set; }

		/// <summary>
		/// Gets the path associated with this route.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// Gets the name of the controller that handles this route.
		/// </summary>
		public string Controller { get; private set; }

		/// <summary>
		/// Gets the name of the controller action that handles this route.
		/// </summary>
		public string Action { get; private set; }

		/// <summary>
		/// Gets the constraints associated with this route.
		/// </summary>
		public Dictionary<string, object> Constraints { get; private set; }

		/// <summary>
		/// Gets the default values associated with this route.
		/// </summary>
		public Dictionary<string, object> Defaults { get; private set; }

		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			this.Name = element.GetAttribute("name");
			this.Path = element.GetAttribute("path");
			this.Controller = element.GetAttribute("controller");
			this.Action = element.GetAttribute("action");
			this.Namespace = element.GetAttribute("namespace");

			this.Constraints = new Dictionary<string, object>();
			this.Defaults = new Dictionary<string, object>();

			this.Defaults.Add("action", this.Action);
			this.Defaults.Add("controller", this.Controller);

			foreach (XmlElement constrNode in element.SelectNodes("p:constraint", XmlNamespaces.Manager))
				this.Constraints[constrNode.GetAttribute("name")] = constrNode.GetAttribute("expression");

			foreach (XmlElement constrNode in element.SelectNodes("p:default", XmlNamespaces.Manager))
				this.Defaults[constrNode.GetAttribute("name")] = constrNode.GetAttribute("value");
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			const string Ns = XmlNamespaces.ProjectConfigurationNamespace;
			XmlElement result = document.CreateElement("route", Ns);
			result.SetAttribute("name", this.Name);
			if (!string.IsNullOrEmpty(this.Extension))
				result.SetAttribute("extension", this.Extension);
			result.SetAttribute("path", this.Path);
			result.SetAttribute("controller", this.Controller);
			result.SetAttribute("action", this.Action);
			result.SetAttribute("namespace", this.Namespace);

			foreach (KeyValuePair<string, object> constraint in this.Constraints)
			{
				XmlElement element = result.AppendElement(document.CreateElement("constraint", Ns));
				element.SetAttribute("name", constraint.Key);
				element.SetAttribute("expression", constraint.Value == null ? string.Empty : constraint.Value.ToString());
			}

			foreach (KeyValuePair<string, object> dflt in this.Defaults)
			{
				if (dflt.Key == "action" || dflt.Key == "controller")
					continue;

				XmlElement element = result.AppendElement(document.CreateElement("default", Ns));
				element.SetAttribute("name", dflt.Key);
				element.SetAttribute("value", dflt.Value == null ? string.Empty : dflt.Value.ToString());
			}

			return result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2}/{3})", this.Name, this.Path, this.Controller, this.Action);
		}
	}
}
