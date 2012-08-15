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
namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.DirectoryServices;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Xml;

	using Kelp.Extensions;

	using Sage.ResourceManagement;
	using Sage.Rewriters;

	using log4net;

	using Sage.Configuration;
	using Sage.Controllers;
	using Sage.Extensibility;
	using Sage.Routing;
	using Sage.Views;

	/// <summary>
	/// Implements the <see cref="HttpApplication"/> class for this web application.
	/// </summary>
	/// <remarks>
	/// this class is supplying methods for the initialisation and destruction of the web application
	/// </remarks>
	public class Project : HttpApplication
	{
		private const string ConfigWatchName = "ProjectConfigurationChangeWatch";
		private static readonly ILog log = LogManager.GetLogger(typeof(Project).FullName);
		private static List<Assembly> relevantAssemblies;

		private static ProblemInfo initializationProblemInfo;
		private static Exception initializationError;
		private static ProjectConfiguration configuration = ProjectConfiguration.Create();

		/// <summary>
		/// Gets a list with the current assembly and all assemblies loaded from the <see cref="AssemblyPath"/> that 
		/// reference the current assembly.
		/// </summary>
		public static List<Assembly> RelevantAssemblies
		{
			get
			{
				lock (log)
				{
					if (relevantAssemblies == null)
					{
						lock (log)
						{
							var currentAssembly = Assembly.GetExecutingAssembly();
							relevantAssemblies = new List<Assembly> { currentAssembly };
							var files = Directory.GetFiles(AssemblyPath, "*.dll", SearchOption.AllDirectories);
							foreach (string path in files)
							{
								Assembly asmb = Assembly.LoadFrom(path);
								if (asmb.GetReferencedAssemblies().Count(a => a.FullName == currentAssembly.FullName) != 0)
								{
									relevantAssemblies.Add(asmb);
								}
							}
						}
					}
				}

				return relevantAssemblies;
			}
		}

		/// <summary>
		/// Gets the physical path of the currently executing assembly.
		/// </summary>
		public static string AssemblyPath
		{
			get
			{
				return Path.GetDirectoryName(
					Assembly.GetExecutingAssembly()
						.CodeBase
						.Replace("file:///", string.Empty)
						.Replace("/", "\\"));
			}
		}

		/// <summary>
		/// Gets the configuration of this project.
		/// </summary>
		public static ProjectConfiguration Configuration
		{
			get
			{
				return configuration;
			}
		}

		private bool IsRequestAvailable
		{
			get
			{
				try
				{
					return this.Context.Request != null;
				}
				catch
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Gets the type with the specified <paramref name="typeName"/>, searching in all <see cref="RelevantAssemblies"/>.
		/// </summary>
		/// <param name="typeName">The name of the type to get.</param>
		/// <returns>The type with the specified <paramref name="typeName"/></returns>
		public static Type GetType(string typeName)
		{
			Contract.Requires<ArgumentNullException>(typeName != null);

			Type result = Type.GetType(typeName, false);
			if (result != null)
				return result;

			foreach (Assembly asm in Sage.Project.RelevantAssemblies)
			{
				result = asm.GetType(typeName, false);
				if (result != null)
					break;
			}

			if (typeName.IndexOf(",", StringComparison.Ordinal) != -1)
			{
				return GetType(typeName.ReplaceAll(@",.*$", string.Empty));
			}

			return result;
		}

		internal static Dictionary<string, string> GetVirtualDirectories(SageContext context)
		{
			Dictionary<string, string> virtualDirectories = null;

			try
			{
				string serverRootPath = context.MapPath("/").ToLower().TrimEnd('\\');
				using (DirectoryEntry iis = new DirectoryEntry("IIS://Localhost/w3svc"))
				{
					IEnumerable<DirectoryEntry> websites = iis.Children.Cast<DirectoryEntry>()
						.Where(c => c.SchemaClassName == "IIsWebServer");

					foreach (DirectoryEntry website in websites)
					{
						using (website)
						{
							DirectoryEntry root = website.Children.Find("Root", "IIsWebVirtualDir");
							string sitePath = root.Properties["path"].Value.ToString().ToLower().TrimEnd('\\');

							if (sitePath == serverRootPath)
							{
								virtualDirectories = GetVirtualDirectories(root, string.Empty);
								break;
							}
						}
					}
				}
			}
			catch (Exception)
			{
				// log.ErrorFormat("Could not retrieve virtual directories in the current application's web server: {0}", ex.Message);
				virtualDirectories = new Dictionary<string, string>();
			}

			return virtualDirectories;
		}

		/// <summary>
		/// Initializes the application using the specified project configuration instance.
		/// </summary>
		/// <param name="controllerFactory">The controller factory to use for this application.</param>
		/// <param name="context">The context in which this method is being executed.</param>
		internal static void Initialize(IControllerFactory controllerFactory, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(controllerFactory != null);

			ViewEngines.Engines.Clear();
			ViewEngines.Engines.Add(new XsltViewEngine());
			ViewEngines.Engines.Add(new WebFormViewEngine());

			ControllerBuilder.Current.SetControllerFactory(controllerFactory);

			InitializeConfiguration(context);

			UrlRoutingUtility.RegisterRoutesToMethodsWithAttributes(RelevantAssemblies.ToArray());

			log.Debug("Manually registering route '*' to GenericController.Action");
			RouteTable.Routes.MapRouteLowercase(
				"GenericController",
				"{*path}",
				new Dictionary<string, object> { { "controller", "Generic" }, { "action", "Action" } });
		}

		/// <summary>
		/// Handles the Start event of the Application control.
		/// </summary>
		protected virtual void Application_Start()
		{
			log.InfoFormat("Application started");

			IControllerFactory controllerFactory = new SageControllerFactory();
			Initialize(controllerFactory, new SageContext(this.Context));
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
			HttpRuntime runtime = (HttpRuntime) typeof(HttpRuntime).InvokeMember(
				"_theRuntime", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null);

			if (runtime == null)
				return;

			string shutDownMessage = (string) runtime.GetType().InvokeMember(
				"_shutDownMessage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

			string shutDownStack = (string) runtime.GetType().InvokeMember(
				"_shutDownStack", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null);

			log.InfoFormat("Application has shut down.");
			log.DebugFormat("	Shutdown message:{0}", shutDownMessage);
			log.DebugFormat("	Shutdown stack:\n{0}", shutDownStack);
		}

		/// <summary>
		/// Handles the BeginRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Application_BeginRequest(object sender, EventArgs e)
		{
			if (initializationError != null)
			{
				StringBuilder html = new StringBuilder();
				using (TextWriter writer = new StringWriter(html))
				{
					SageHelpException helpException = new SageHelpException(initializationProblemInfo, initializationError);
					helpException.Render(writer, new SageContext(this.Context));
				}

				this.Response.Write(html.ToString());
				this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				this.Response.Cache.SetNoStore();
				this.Response.End();
			}

			var context = new SageContext(this.Context);
			if (context.LmCache.Get(ConfigWatchName) == null)
				InitializeConfiguration(context);

			if (Thread.CurrentThread.Name != null)
				return;

			Thread.CurrentThread.Name = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
			if (this.Context != null)
			{
				log.InfoFormat(
					"Request {0} started, thread name set to {1}", HttpContext.Current.Request.Url, Thread.CurrentThread.Name);
			}
			else
			{
				log.InfoFormat("Request started, thread name set to {0}", Thread.CurrentThread.Name);
			}
		}

		/// <summary>
		/// Handles the EndRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Application_EndRequest(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(Thread.CurrentThread.Name))
				return;

			var startTime = long.Parse(Thread.CurrentThread.Name);
			var elapsed = new TimeSpan(DateTime.Now.Ticks - startTime);

			log.InfoFormat("Request completed in {0}ms.", elapsed.Milliseconds);
		}

		/// <summary>
		/// Handles the Error event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Application_Error(object sender, EventArgs e)
		{
			Exception exception = Server.GetLastError();
			if (exception == null)
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

			if (this.IsRequestAvailable)
			{
				sageException.Render(writer, context);
			}
			else
			{
				sageException.RenderWithoutContext(writer);
			}

			writer.Close();
			writer.Dispose();

			this.Response.Write(html.ToString());
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			this.Response.Cache.SetNoStore();
			this.Response.End();
		}

		private static Dictionary<string, string> GetVirtualDirectories(DirectoryEntry directory, string path)
		{
			IEnumerable<DirectoryEntry> directories = directory.Children.Cast<DirectoryEntry>()
				.Where(c => c.SchemaClassName == "IIsWebVirtualDir");

			Dictionary<string, string> result = new Dictionary<string, string>();
			foreach (DirectoryEntry entry in directories)
			{
				string key = string.Concat(path, "/", entry.Name);
				result.Add(key, entry.Properties["path"].Value.ToString().ToLower().TrimEnd('\\'));

				Dictionary<string, string> childDirs = GetVirtualDirectories(entry, key);
				foreach (string childKey in childDirs.Keys)
				{
					result.Add(childKey, childDirs[childKey]);
				}
			}

			return result;
		}

		private static void InitializeConfiguration(SageContext context)
		{
			string systemConfigPath = Path.Combine(AssemblyPath, ProjectConfiguration.SystemConfigName);
			string projectConfigPathBinDir = Path.Combine(AssemblyPath, ProjectConfiguration.ProjectConfigName);
			string projectConfigPathProjDir = Path.Combine(AssemblyPath, "..\\" + ProjectConfiguration.ProjectConfigName);

			string projectConfigPath = projectConfigPathBinDir;
			if (File.Exists(projectConfigPathProjDir))
			{
				projectConfigPath = projectConfigPathProjDir;
			}

			if (!File.Exists(projectConfigPath) && !File.Exists(systemConfigPath))
				throw new SageHelpException(ProblemType.MissingConfigurationFile);

			var projectConfig = ProjectConfiguration.Create();

			if (File.Exists(systemConfigPath))
				projectConfig.Parse(systemConfigPath);

			if (File.Exists(projectConfigPath))
				projectConfig.Parse(projectConfigPath);

			if (projectConfig.Locales.Count == 0)
			{
				var defaultLocale = new LocaleInfo();
				projectConfig.Locales.Add(defaultLocale.Name, defaultLocale);
			}

			var result = projectConfig.ValidationResult;
			if (!result.Success)
			{
				initializationError = result.Exception;
				initializationProblemInfo = new ProblemInfo(ProblemType.ProjectSchemaValidationError, result.SourceFile);
			}
			else
			{
				configuration = projectConfig;

				var validationResult = ResourceManager.ValidateElement(
					projectConfig.ToXml(new XmlDocument()), ProjectConfiguration.ConfigSchemaPath);

				log.DebugFormat("Project configuration success: {0}", validationResult.Success);

				var extensionManager = new ExtensionManager();
				result = extensionManager.Initialize(context);
				if (!result.Success)
				{
					initializationError = result.Exception;
					initializationProblemInfo = new ProblemInfo(ProblemType.ExtensionSchemaValidationError, result.SourceFile);
				}
				else
				{
					foreach (var extension in extensionManager)
					{
						Project.RelevantAssemblies.AddRange(extension.Assemblies);
						projectConfig.RegisterExtension(extension.Config);
					}

					projectConfig.RegisterRoutes();
					context.LmCache.Put(ConfigWatchName, DateTime.Now, projectConfig.Dependencies);
				}
			}
		}
	}
}
