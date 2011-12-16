namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	/// <summary>
	/// Contains the 
	/// </summary>
	public class RoutingConfiguration : Dictionary<string, RouteInfo>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingConfiguration"/> class.
		/// </summary>
		public RoutingConfiguration()
		{
			this.DefaultNamespace = string.Empty;
		}

		/// <summary>
		/// Gets the default controller namespace for routes that don't define the namespace explicitly.
		/// </summary>
		public string DefaultNamespace
		{
			get;
			private set;
		}

		/// <summary>
		/// Parses the routing configuration from the specified configuration element.
		/// </summary>
		/// <param name="configElement">The configuration element that defines the routing.</param>
		public void ParseConfiguration(XmlElement configElement)
		{
			if (configElement == null)
				throw new ArgumentNullException("configElement");

			this.DefaultNamespace = configElement.GetAttribute("defaultNamespace");
			foreach (XmlElement routeNode in configElement.SelectNodes("p:route", XmlNamespaces.Manager))
			{
				RouteInfo route = new RouteInfo(routeNode);
				this.Add(route.Name, route);
			}
		}
	}
}
