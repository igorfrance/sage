namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;

	using Kelp.Core;
	using log4net;

	using Sage.Configuration;
	using Sage.Controllers;
	using Sage.Routing;
	using Sage.Views;

	/// <summary>
	/// Implements the <see cref="HttpApplication"/> class for this web application.
	/// </summary>
	/// <remarks>
	/// this class is supplying methods for the initialisation and destruction of the web application
	/// </remarks>
	public class Application : HttpApplication
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Application).FullName);

		/// <summary>
		/// Registers the URL routes in use by this application
		/// </summary>
		/// <param name="routes">The collection of routes to register.</param>
		/// <remarks>
		/// the routes for the home page and custom pages are added manually (hardcoded) here.
		/// </remarks>
		public static void RegisterRoutes(RouteCollection routes)
		{
			Contract.Requires<ArgumentNullException>(routes != null);

			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			UrlRoutingUtility.RegisterRoutes(routes);

			log.Debug("Manually registering route '' to GenericController.Action");
			routes.MapRouteLowercase(
				"GenericController.Default",
				string.Empty,
				new { controller = "Generic", action = "Action" });

			log.Debug("Manually registering route '*' to GenericController.Action");
			routes.MapRouteLowercase(
				"GenericController.CatchAll",
				"{*catchall}",
				new { controller = "Generic", action = "Action" });
		}

		/// <summary>
		/// Initializes the application using the specified project configuration instance.
		/// </summary>
		/// <param name="controllerFactory">The controller factory to use for this application. This argument is optional and
		/// can be <c>null</c>.</param>
		internal static void Initialize(IControllerFactory controllerFactory)
		{
			Contract.Requires<ArgumentNullException>(controllerFactory != null);

			ViewEngines.Engines.Clear();
			ViewEngines.Engines.Add(new XsltViewEngine());
			ViewEngines.Engines.Add(new WebFormViewEngine());

			ControllerBuilder.Current.SetControllerFactory(controllerFactory);

			RegisterRoutes(RouteTable.Routes);
		}

		/// <summary>
		/// Handles the Start event of the Application control.
		/// </summary>
		protected virtual void Application_Start()
		{
			log.DebugFormat("Application started");

			IControllerFactory controllerFactory = new SageControllerFactory();
			Initialize(controllerFactory);
		}

		/// <summary>
		/// Handles the End event of the Application control.
		/// </summary>
		/// <remarks>
		/// Logs the application shutdown event, together with the reason and detail of the shutdown.
		/// </remarks>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Application_End(object sender, EventArgs e)
		{
			HttpRuntime runtime =
				(HttpRuntime)
				typeof(System.Web.HttpRuntime).InvokeMember(
					"_theRuntime", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);

			if (runtime == null)
				return;

			string shutDownMessage =
				(string)
				runtime.GetType().InvokeMember(
					"_shutDownMessage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

			string shutDownStack =
				(string)
				runtime.GetType().InvokeMember(
					"_shutDownStack", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

			log.DebugFormat("Application has shut down with message\n{0}\nStack:\n{1}", shutDownMessage, shutDownStack);
		}

		/// <summary>
		/// Handles the BeginRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Application_BeginRequest(object sender, EventArgs e)
		{
			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			// ReSharper disable HeuristicUnreachableCode
			if (Thread.CurrentThread.Name == null)
			{
				Thread.CurrentThread.Name = DateTime.Now.Ticks.ToString();
				if (HttpContext.Current != null)
				{
					log.DebugFormat(
						"Request {0} started, thread name set to {1}", HttpContext.Current.Request.Url, Thread.CurrentThread.Name);
				}
				else
				{
					log.DebugFormat("Request started, thread name set to {0}", Thread.CurrentThread.Name);
				}
			}
		}

		/// <summary>
		/// Handles the EndRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Application_EndRequest(object sender, EventArgs e)
		{
			log.DebugFormat("Request ended");
		}

		/// <summary>
		/// Handles the Error event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Application_Error(object sender, EventArgs e)
		{
			Exception exception = Server.GetLastError();
			if (exception == null || this.Context.Request == null)
				return;

			if (exception is System.Threading.ThreadAbortException)
				return;

			log.Fatal(exception.Message, exception);

			StringBuilder html = new StringBuilder();
			TextWriter writer = new StringWriter(html);
			SageContext context = new SageContext(this.Context);

			SageException sageException = exception is SageException 
				? (SageException) exception 
				: new SageException(exception);

			sageException.Render(context, writer);

			writer.Close();
			writer.Dispose();

			this.Response.Write(html.ToString());
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			this.Response.Cache.SetNoStore();
			this.Response.End();
		}
	}
}
