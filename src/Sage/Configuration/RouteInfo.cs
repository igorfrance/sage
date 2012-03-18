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

	/// <summary>
	/// Contains configuration information about a single URL route.
	/// </summary>
	public struct RouteInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RouteInfo"/> struct, using the specified 
		/// <paramref name="configurationElement"/>.
		/// </summary>
		/// <param name="configurationElement">The route configuration element's.</param>
		/// <exception cref="ArgumentNullException"><c>routeConfig</c> is null.</exception>
		public RouteInfo(XmlElement configurationElement)
			: this()
		{
			if (configurationElement == null)
				throw new ArgumentNullException("configurationElement");

			this.Name = configurationElement.GetAttribute("name");
			this.Path = configurationElement.GetAttribute("path");
			this.Controller = configurationElement.GetAttribute("controller");
			this.Action = configurationElement.GetAttribute("action");
			this.Namespace = configurationElement.GetAttribute("namespace");

			this.Constraints = new Dictionary<string, string>();
			this.Defaults = new Dictionary<string, string>();

			this.Defaults.Add("action", this.Action);
			this.Defaults.Add("controller", this.Controller);

			foreach (XmlElement constrNode in configurationElement.SelectNodes("p:constraint", XmlNamespaces.Manager))
				this.Constraints.Add(constrNode.GetAttribute("name"), constrNode.GetAttribute("expression"));

			foreach (XmlElement constrNode in configurationElement.SelectNodes("p:default", XmlNamespaces.Manager))
				this.Defaults.Add(constrNode.GetAttribute("name"), constrNode.GetAttribute("value"));
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
		public Dictionary<string, string> Constraints { get; private set; }

		/// <summary>
		/// Gets the default values associated with this route.
		/// </summary>
		public Dictionary<string, string> Defaults { get; private set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2}/{3})", this.Name, this.Path, this.Controller, this.Action);
		}
	}
}
