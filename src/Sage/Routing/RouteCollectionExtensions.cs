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
namespace Sage.Routing
{
	using System;
	using System.Web.Mvc;
	using System.Web.Routing;

	using log4net;

	/// <summary>
	/// Implements extensions to <see cref="RouteCollection"/> class.
	/// </summary>
	public static class RouteCollectionExtensions
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(RouteCollectionExtensions).FullName);

		/// <summary>
		/// Maps a route as a <see cref="LowerCaseRoute"/>.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		public static void MapRoute(this RouteCollection routes, string name, string url)
		{
			routes.MapRouteLowercase(name, url, null, null);
		}

		/// <summary>
		/// Maps a route as a <see cref="LowerCaseRoute"/>.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		public static void MapRoute(this RouteCollection routes, string name, string url, object defaults)
		{
			routes.MapRouteLowercase(name, url, defaults, null);
		}

		/// <summary>
		/// Maps a route as a <see cref="LowerCaseRoute"/>.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		/// <param name="constraints">The route parameter constraints.</param>
		public static void MapRoute(this RouteCollection routes, string name, string url, object defaults, object constraints)
		{
			routes.MapRouteLowercase(name, url, defaults, constraints);
		}

		/// <summary>
		/// Maps a route, ensuring characters are registered in lower case.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url)
		{
			routes.MapRouteLowercase(name, url, null, null);
		}

		/// <summary>
		/// Maps a route, ensuring characters are registered in lower case.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, object defaults)
		{
			routes.MapRouteLowercase(name, url, defaults, null);
		}

		/// <summary>
		/// Maps a route, ensuring characters are registered in lower case.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="namespaces">The route namespaces.</param>
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, string[] namespaces)
		{
			routes.MapRouteLowercase(name, url, null, null, namespaces);
		}

		/// <summary>
		/// Maps a route, ensuring characters are registered in lower case.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		/// <param name="namespaces">The route namespaces.</param>
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
		{
			MapRouteLowercase(routes, name, url, defaults, null, namespaces);
		}

		/// <summary>
		/// Maps a route, ensuring characters are registered in lower case.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		/// <param name="constraints">The route parameter constraints.</param>
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, object defaults, object constraints)
		{
			MapRouteLowercase(routes, name, url, defaults, constraints, null);
		}

		/// <summary>
		/// Maps a route, ensuring characters are registered in lower case.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		/// <param name="constraints">The route parameter constraints.</param>
		/// <param name="namespaces">The route namespaces.</param>
		/// <exception cref="ArgumentNullException"><paramref name="routes"/> or <paramref name="url"/> is <c>null</c></exception>
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
		{
			if (routes == null)
				throw new ArgumentNullException("routes");

			if (url == null)
				throw new ArgumentNullException("url");
			
			var route = new LowerCaseRoute(url, new MvcRouteHandler())
			{
				Name = name,
				Defaults = new RouteValueDictionary(defaults),
				Constraints = new RouteValueDictionary(constraints),
				DataTokens = new RouteValueDictionary(),
			};

			if ((namespaces != null) && (namespaces.Length > 0))
			{
				route.DataTokens["Namespaces"] = namespaces;
			}

			if (String.IsNullOrEmpty(name))
				routes.Add(route);
			else
			{
				for (var i = 0; i < routes.Count; i++)
				{
					var r = routes[i] as LowerCaseRoute;
					if (r != null && r.Name == name)
					{
						log.WarnFormat("Overwriting an identically named route ('{0}') for url '{1}' and controller '{2}' with a new route for url '{3}' and controller '{4}'",
							name, r.Url, r.Defaults["controller"], route.Url, route.Defaults["controller"]);
						
						routes.Insert(i, route);
						return;
					}
				}

				routes.Add(name, route);
			}
		}
	}
}