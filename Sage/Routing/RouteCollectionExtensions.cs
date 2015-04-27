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
namespace Sage.Routing
{
	using System;
	using System.Collections.Generic;
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
			routes.MapRouteLowercase(name, url, null, null as IDictionary<string, object>);
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
			routes.MapRouteLowercase(name, url, defaults as IDictionary<string, object>, null as IDictionary<string, object>);
		}

		/// <summary>
		/// Maps a route as a <see cref="LowerCaseRoute"/>.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		/// <param name="constraints">The route parameter constraints.</param>
		public static void MapRoute(this RouteCollection routes, string name, string url, IDictionary<string, object> defaults, IDictionary<string, object> constraints)
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
			routes.MapRouteLowercase(name, url, null, null as IDictionary<string, object>);
		}

		/// <summary>
		/// Maps a route, ensuring characters are registered in lower case.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, IDictionary<string, object> defaults)
		{
			routes.MapRouteLowercase(name, url, defaults, null as IDictionary<string, object>);
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
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, IDictionary<string, object> defaults, string[] namespaces)
		{
			RouteCollectionExtensions.MapRouteLowercase(routes, name, url, defaults, null, namespaces);
		}

		/// <summary>
		/// Maps a route, ensuring characters are registered in lower case.
		/// </summary>
		/// <param name="routes">The target <see cref="RouteCollection"/>.</param>
		/// <param name="name">The name of the route.</param>
		/// <param name="url">The route URL pattern.</param>
		/// <param name="defaults">The route default values.</param>
		/// <param name="constraints">The route parameter constraints.</param>
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, IDictionary<string, object> defaults, IDictionary<string, object> constraints)
		{
			RouteCollectionExtensions.MapRouteLowercase(routes, name, url, defaults, constraints, null);
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
		public static void MapRouteLowercase(this RouteCollection routes, string name, string url, IDictionary<string, object> defaults, IDictionary<string, object> constraints, string[] namespaces)
		{
			if (routes == null)
				throw new ArgumentNullException("routes");

			if (url == null)
				throw new ArgumentNullException("url");
			
			var route = new LowerCaseRoute(url, new MvcRouteHandler())
			{
				Name = name,
				Defaults = defaults == null ? null : new RouteValueDictionary(defaults),
				Constraints = constraints == null ? null : new RouteValueDictionary(constraints),
				DataTokens = new RouteValueDictionary(),
			};

			if ((namespaces != null) && (namespaces.Length > 0))
			{
				route.DataTokens["Namespaces"] = namespaces;
			}

			if (string.IsNullOrEmpty(name))
			{
				routes.Add(route);
			}
			else
			{
				for (var i = 0; i < routes.Count; i++)
				{
					var r = routes[i] as LowerCaseRoute;
					if (r != null && r.Name == name)
					{
						log.WarnFormat("Overwriting an identically named route ('{0}') for url '{1}' and controller '{2}' with a new route for url '{3}' and controller '{4}'",
							name, r.Url, r.Defaults["controller"], route.Url, route.Defaults["controller"]);

						routes[i] = route;
						return;
					}
				}

				routes.Add(name, route);
			}
		}
	}
}