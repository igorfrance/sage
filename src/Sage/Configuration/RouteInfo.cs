/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
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
		public string Name { get;  set; }

		/// <summary>
		/// Gets the namespace of this controller that handles this route.
		/// </summary>
		public string Namespace { get;  set; }

		/// <summary>
		/// Gets the path of this route.
		/// </summary>
		public string Path { get;  set; }

		/// <summary>
		/// Gets the name of the controller that handles this route.
		/// </summary>
		public string Controller { get;  set; }

		/// <summary>
		/// Gets the name of the controller action that handles this route.
		/// </summary>
		public string Action { get;  set; }

		/// <summary>
		/// Gets the constraints associated with this route.
		/// </summary>
		public Dictionary<string, string> Constraints { get;  set; }

		/// <summary>
		/// Gets the defaults associated with this route.
		/// </summary>
		public Dictionary<string, string> Defaults { get;  set; }

		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2}/{3})", this.Name, this.Path, this.Controller, this.Action);
		}
	}
}
