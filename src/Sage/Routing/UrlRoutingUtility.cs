﻿namespace Sage.Routing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Web.Mvc;
	using System.Web.Routing;

	using log4net;
	using Sage.Configuration;

	/// <summary>
	/// Provides additional URL routing functionality.
	/// </summary>
	public static class UrlRoutingUtility
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(UrlRoutingUtility).FullName);

		/// <summary>
		/// Uses reflection to enumerate through Controller classes and automatically register routes.
		/// </summary>
		/// <param name="routes">The routes.</param>
		public static void RegisterRoutes(RouteCollection routes)
		{
			RegisterRoutesToMethodsWithAttributes(routes);
			RegisterRoutesFromRoutingConfiguration(routes);
		}

		private static void RegisterRoutesFromRoutingConfiguration(RouteCollection routes)
		{
			ProjectConfiguration config = ProjectConfiguration.Current;
			foreach (RouteInfo route in config.Routing.Values)
			{
				var controllerName = route.Controller.EndsWith("Controller")
					? route.Controller.Substring(0, route.Controller.LastIndexOf("Controller"))
					: route.Controller;

				string[] namespaces = new[] { route.Namespace ?? config.Routing.DefaultNamespace };
				routes.MapRouteLowercase(route.Name, route.Path,
					new { controller = controllerName, action = route.Action }, 
					null, 
					namespaces);

				log.DebugFormat("Automatically registering route '{0}' to {1}.{2}",
					route.Path,
					route.Controller,
					route.Action);
			}
		}

		/// <summary>
		/// Uses reflection to enumerate through Controller classes in the assembly and registers a route for each method declaring a <see cref="UrlRouteAttribute"/>.
		/// </summary>
		/// <param name="routes">Route collection to add routes to.</param>
		private static void RegisterRoutesToMethodsWithAttributes(RouteCollection routes)
		{
			List<MapRouteParams> routeParams = GetRouteParamsFromAttributes(
				ProjectConfiguration.RelevantAssemblies.ToArray());

			routeParams.Sort((a, b) => a.Order.CompareTo(b.Order));
			foreach (MapRouteParams rd in routeParams)
			{
				log.DebugFormat("Automatically registering route '{0}' for controller '{1}Controller.{2}' (Priority = {3}, Name = {4})",
					rd.Path, rd.ControllerName, rd.ActionName, rd.Order, rd.RouteName ?? "<null>");

				rd.Defaults["controller"] = rd.ControllerName;
				rd.Defaults["action"] = rd.ActionName;

				string name = rd.RouteName;
				if (string.IsNullOrEmpty(name))
					name = string.Concat(rd.ControllerName, ".", rd.ActionName);

				UrlRoutingUtility.MapRoute(routes, name, rd.Path, rd.Defaults, rd.Constraints, new[] { rd.ControllerNamespace });
			}
		}

		/// <summary>
		/// Uses reflection to enumerate all Controller classes in the assembly and registers a route for each method declaring a <see cref="UrlRouteAttribute"/>.
		/// </summary>
		/// <exception cref="ApplicationException">
		/// A controller's class name doesn't end with 'Controller'
		/// - or -
		/// A route path contains a '/' or '?' characters.
		/// </exception>
		private static List<MapRouteParams> GetRouteParamsFromAttributes(params Assembly[] assemblies)
		{
			List<MapRouteParams> routeParams = new List<MapRouteParams>();

			foreach (Assembly a in assemblies)
			{
				var controllers = from t in a.GetTypes()
								  where t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(System.Web.Mvc.Controller))
								  select t;

				foreach (Type type in controllers)
				{
					foreach (MethodInfo methodInfo in type.GetMethods())
					{
						foreach (UrlRouteAttribute routeAttrib in methodInfo.GetCustomAttributes(typeof(UrlRouteAttribute), false))
						{
							string controllerName = type.Name;
							if (!controllerName.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase))
							{
								string errorMessage = string.Format("Invalid controller class name {0}: name must end with \"Controller\"",
									controllerName);

								log.Fatal(errorMessage);
								throw new ApplicationException(errorMessage);
							}

							controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);
							if (routeAttrib.Path.StartsWith("/") || routeAttrib.Path.Contains("?"))
							{
								string errorMessage = string.Format("Invalid UrlRoute attribute \"{0}\" on method {1}.{2}: Path cannot start with \"/\" or contain \"?\".",
									routeAttrib.Path, controllerName, methodInfo.Name);

								log.Fatal(errorMessage);
								throw new ApplicationException(errorMessage);
							}

							routeParams.Add(new MapRouteParams
								{
									RouteName = String.IsNullOrEmpty(routeAttrib.Name) ? null : routeAttrib.Name,
									Path = routeAttrib.Path,
									ControllerName = controllerName,
									ActionName = methodInfo.Name,
									Order = routeAttrib.Order,
									Constraints = GetConstraints(methodInfo),
									Defaults = GetDefaults(methodInfo),
									ControllerNamespace = type.Namespace,
								});
						}
					}
				}
			}

			return routeParams;
		}

		/// <summary>
		/// Maps a route.
		/// </summary>
		/// <param name="routes">The routes.</param>
		/// <param name="name">The name.</param>
		/// <param name="url">The URL.</param>
		/// <param name="defaults">The defaults.</param>
		/// <param name="constraints">The constraints.</param>
		/// <param name="namespaces">The namespaces.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="routes"/> or <paramref name="url"/> or <paramref name="constraints"/> is <c>null</c>
		/// </exception>
		private static void MapRoute(RouteCollection routes, string name, string url, 
			IDictionary<string, object> defaults, IDictionary<string, object> constraints, ICollection<string> namespaces)
		{
			if (routes == null)
				throw new ArgumentNullException("routes");
			if (url == null)
				throw new ArgumentNullException("url");
			if (constraints == null)
				throw new ArgumentNullException("constraints");

			LowerCaseRoute route = new LowerCaseRoute(url, new MvcRouteHandler())
			{
				Name = name,
				Defaults = new RouteValueDictionary(defaults),
				Constraints = new RouteValueDictionary(constraints)
			};

			if ((namespaces != null) && (namespaces.Count > 0))
			{
				route.DataTokens = new RouteValueDictionary();
				route.DataTokens["Namespaces"] = namespaces;
			}

			routes.Add(name, route);
		}

		/// <summary>
		/// Gets the constraints.
		/// </summary>
		/// <param name="mi">The method info.</param>
		/// <returns></returns>
		/// <exception cref="ApplicationException"><c>ApplicationException</c>.</exception>
		private static Dictionary<string, object> GetConstraints(MethodInfo mi)
		{
			Dictionary<string, object> constraints = new Dictionary<string, object>();

			foreach (UrlRouteParameterConstraintAttribute attrib in mi.GetCustomAttributes(typeof(UrlRouteParameterConstraintAttribute), true))
			{
				if (String.IsNullOrEmpty(attrib.Name))
				{
					throw new ApplicationException(String.Format("UrlRouteParameterContraint attribute on {0}.{1} is missing the Name property.",
						mi.DeclaringType.Name, mi.Name));
				}

				if (String.IsNullOrEmpty(attrib.Expression))
				{
					throw new ApplicationException(String.Format("UrlRouteParameterContraint attribute on {0}.{1} is missing the RegEx property.",
						mi.DeclaringType.Name, mi.Name));
				}

				constraints.Add(attrib.Name, attrib.Expression);
			}

			return constraints;
		}

		/// <summary>
		/// Gets the defaults.
		/// </summary>
		/// <param name="mi">The mi.</param>
		/// <returns></returns>
		/// <exception cref="ApplicationException"><c>ApplicationException</c>.</exception>
		private static Dictionary<string, object> GetDefaults(MethodInfo mi)
		{
			Dictionary<string, object> defaults = new Dictionary<string, object>();

			foreach (UrlRouteParameterDefaultAttribute attrib in mi.GetCustomAttributes(typeof(UrlRouteParameterDefaultAttribute), true))
			{
				if (String.IsNullOrEmpty(attrib.Name))
				{
					throw new ApplicationException(String.Format("UrlRouteParameterDefault attribute on {0}.{1} is missing the Name property.",
						mi.DeclaringType.Name, mi.Name));
				}

				if (attrib.Value == null)
				{
					throw new ApplicationException(String.Format("UrlRouteParameterDefault attribute on {0}.{1} is missing the Value property.",
						mi.DeclaringType.Name, mi.Name));
				}

				defaults.Add(attrib.Name, attrib.Value);
			}

			return defaults;
		}

		private class MapRouteParams
		{
			public int Order { get; set; }

			public string RouteName { get; set; }

			public string Path { get; set; }

			public string ControllerNamespace { get; set; }

			public string ControllerName { get; set; }

			public string ActionName { get; set; }

			public Dictionary<string, object> Defaults { get; set; }

			public Dictionary<string, object> Constraints { get; set; }
		}
	}
}
