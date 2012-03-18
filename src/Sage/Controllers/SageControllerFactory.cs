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
namespace Sage.Controllers
{
	using System;
	using System.Reflection;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;

	using log4net;

	/// <summary>
	/// Implements a factory for <see cref="IController"/>s used within this application.
	/// </summary>
	public class SageControllerFactory : DefaultControllerFactory
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SageControllerFactory).FullName);

		/// <summary>
		/// Constructs the controller instance needed to service the current request.
		/// </summary>
		/// <param name="requestContext">The request context under which this method is being called.</param>
		/// <param name="type">The type of the controller to create.</param>
		/// <returns>
		/// The controller instance that services the current request.
		/// </returns>
		protected override IController GetControllerInstance(RequestContext requestContext, Type type)
		{
			if (type == null)
			{
				Route route = (Route) requestContext.RouteData.Route;

				if (route.DataTokens["namespaces"] != null && route.DataTokens["namespaces"] is string[])
				{
					string controllerName = string.Concat(route.Defaults["controller"], "Controller");
					string controllerNs = ((string[]) route.DataTokens["namespaces"])[0];

					string typeName = string.Concat(controllerNs, ".", controllerName);
					type = Application.GetType(typeName);
				}
			}

			if (type == null)
				return base.GetControllerInstance(requestContext, null);

			ConstructorInfo[] constructors = type.GetConstructors();
			IController result = (IController) constructors[0].Invoke(new object[] { });

			HttpContextBase context = requestContext.HttpContext;

			string requestUrl = null;
			if (context != null)
				requestUrl = context.Request.Url.ToString();

			if (requestUrl != null)
				log.DebugFormat("Url '{0}' resolved to controller '{1}'", requestUrl, type.FullName);
			else
				log.DebugFormat("Controller '{0}' created.", type.FullName);

			return result;
		}
	}
}
