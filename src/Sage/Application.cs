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

	using Sage.Extensibility;

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
		private static readonly ExtensionManager extensionManager = new ExtensionManager();

		/// <summary>
		/// Gets a list with the current assembly and all assemblies loaded from the <see cref="ProjectConfiguration.AssemblyPath"/> that 
		/// reference the current assembly.
		/// </summary>
		public static List<Assembly> RelevantAssemblies
		{
			get
			{
				var currentAssembly = Assembly.GetExecutingAssembly();
				List<Assembly> result = new List<Assembly> { currentAssembly };
				try
				{
					var files = Directory.GetFiles(ProjectConfiguration.AssemblyPath, "*.dll", SearchOption.AllDirectories);
					foreach (string path in files)
					{
						Assembly asmb = Assembly.LoadFrom(path);
						if (asmb.GetReferencedAssemblies().Where(a => a.FullName == currentAssembly.FullName).Count() != 0)
						{
							result.Add(asmb);
						}
					}
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Failed to count the numer of assemblies: {0}", ex.Message);
				}

				return result;
			}
		}

		internal static ExtensionManager Extensions
		{
			get
			{
				return extensionManager;
			}
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

			ProjectConfiguration.Initialize();
			SageContext context = new SageContext(HttpContext.Current);
			extensionManager.Initialize(context);

			foreach (ExtensionInfo plugin in extensionManager)
			{
				
			}

			RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			UrlRoutingUtility.RegisterRoutesToMethodsWithAttributes(RelevantAssemblies.ToArray());
			UrlRoutingUtility.RegisterRoutesFromRoutingConfiguration(ProjectConfiguration.Current);

			log.Debug("Manually registering route '' to GenericController.Action");
			RouteTable.Routes.MapRouteLowercase(
				"GenericController.Default",
				String.Empty,
				new { controller = "Generic", action = "Action" });

			log.Debug("Manually registering route '*' to GenericController.Action");
			RouteTable.Routes.MapRouteLowercase(
				"GenericController.CatchAll",
				"{*catchall}",
				new { controller = "Generic", action = "Action" });
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
				typeof(HttpRuntime).InvokeMember(
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

			if (exception is ThreadAbortException)
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
