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
	using System.Linq;
	using System.Reflection;
	using System.Web.Mvc;
	using System.Web.Routing;

	using Kelp;

	using log4net;
	using Sage.Configuration;
	using Sage.ResourceManagement;

	/// <summary>
	/// Provides additional URL routing functionality.
	/// </summary>
	public static class UrlRoutingUtility
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(UrlRoutingUtility).FullName);

		/// <summary>
		/// Uses reflection to enumerate through Controller classes in the assembly and registers a route for each method declaring a <see cref="UrlRouteAttribute"/>.
		/// </summary>
		/// <param name="assemblies">Assemblies that contain the methods to register.</param>
		internal static void RegisterRoutesToMethodsWithAttributes(Assembly[] assemblies)
		{
			List<MapRouteParams> routeParams = UrlRoutingUtility.GetRouteParamsFromAttributes(
				Project.RelevantAssemblies.ToArray());

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

				UrlRoutingUtility.MapRoute(name, rd.Path, rd.Defaults, rd.Constraints, new[] { rd.ControllerNamespace });
			}
		}

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
								RouteName = string.IsNullOrEmpty(routeAttrib.Name) ? null : routeAttrib.Name,
								Path = routeAttrib.Path,
								ControllerName = controllerName,
								ActionName = methodInfo.Name,
								Order = routeAttrib.Order,
								Constraints = UrlRoutingUtility.GetConstraints(methodInfo),
								Defaults = UrlRoutingUtility.GetDefaults(methodInfo),
								ControllerNamespace = type.Namespace,
							});
						}
					}
				}
			}

			return routeParams;
		}

		private static void MapRoute(string name, string url, 
			IDictionary<string, object> defaults, IDictionary<string, object> constraints, ICollection<string> namespaces)
		{
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

			var existingRoute = RouteTable.Routes[name];
			if (existingRoute != null)
				RouteTable.Routes.Remove(existingRoute);

			RouteTable.Routes.Add(name, route);
		}

		private static Dictionary<string, object> GetConstraints(MethodInfo mi)
		{
			Dictionary<string, object> constraints = new Dictionary<string, object>();

			foreach (UrlRouteParameterConstraintAttribute attrib in mi.GetCustomAttributes(typeof(UrlRouteParameterConstraintAttribute), true))
			{
				if (string.IsNullOrEmpty(attrib.Name))
				{
					throw new ApplicationException(string.Format("UrlRouteParameterContraint attribute on {0} is missing the Name property.",
						Util.GetMethodSignature(mi)));
				}

				if (string.IsNullOrEmpty(attrib.Expression))
				{
					throw new ApplicationException(string.Format("UrlRouteParameterContraint attribute on {0} is missing the RegEx property.",
						Util.GetMethodSignature(mi)));
				}

				constraints.Add(attrib.Name, attrib.Expression);
			}

			return constraints;
		}

		private static Dictionary<string, object> GetDefaults(MethodInfo mi)
		{
			Dictionary<string, object> defaults = new Dictionary<string, object>();

			foreach (UrlRouteParameterDefaultAttribute attrib in mi.GetCustomAttributes(typeof(UrlRouteParameterDefaultAttribute), true))
			{
				if (string.IsNullOrEmpty(attrib.Name))
				{
					throw new ApplicationException(string.Format("UrlRouteParameterDefault attribute on {0} is missing the Name property.",
						Util.GetMethodSignature(mi)));
				}

				if (attrib.Value == null)
				{
					throw new ApplicationException(string.Format("UrlRouteParameterDefault attribute on {0} is missing the Value property.",
						Util.GetMethodSignature(mi)));
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
