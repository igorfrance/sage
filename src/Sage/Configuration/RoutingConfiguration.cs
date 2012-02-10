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
