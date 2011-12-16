namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	/// <summary>
	/// Contains configuration information about a single configured URL route.
	/// </summary>
	public struct RouteInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RouteInfo"/> struct.
		/// </summary>
		/// <param name="routeConfig">The route configuration node.</param>
		/// <exception cref="ArgumentNullException"><c>routeConfig</c> is null.</exception>
		public RouteInfo(XmlElement routeConfig)
			: this()
		{
			if (routeConfig == null)
				throw new ArgumentNullException("routeConfig");

			this.Name = routeConfig.GetAttribute("name");
			this.Path = routeConfig.GetAttribute("path");
			this.Controller = routeConfig.GetAttribute("controller");
			this.Action = routeConfig.GetAttribute("action");
			this.Namespace = routeConfig.GetAttribute("namespace");

			this.Constraints = new Dictionary<string, string>();
			this.Defaults = new Dictionary<string, string>();

			this.Defaults.Add("action", this.Action);
			this.Defaults.Add("controller", this.Controller);

			foreach (XmlElement constrNode in routeConfig.SelectNodes("p:constraint", XmlNamespaces.Manager))
				this.Constraints.Add(constrNode.GetAttribute("name"), constrNode.GetAttribute("expression"));

			foreach (XmlElement constrNode in routeConfig.SelectNodes("p:default", XmlNamespaces.Manager))
				this.Defaults.Add(constrNode.GetAttribute("name"), constrNode.GetAttribute("value"));
		}

		/// <summary>
		/// Gets the name of this route.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the namespace of this controller that handles this route.
		/// </summary>
		public string Namespace
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the path of this route.
		/// </summary>
		public string Path
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of the controller that handles this route.
		/// </summary>
		public string Controller
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of the controller action that handles this route.
		/// </summary>
		public string Action
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the constraints associated with this route.
		/// </summary>
		public Dictionary<string, string> Constraints
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the defaults associated with this route.
		/// </summary>
		public Dictionary<string, string> Defaults
		{
			get;
			private set;
		}
	}
}
