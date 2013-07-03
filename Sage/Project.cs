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

	using Kelp;
	using Kelp.Extensions;

	using log4net;

	using Sage.Configuration;
	using Sage.Controllers;
	using Sage.Extensibility;
	using Sage.ResourceManagement;
	using Sage.Routing;
	using Sage.Views;

	/// <summary>
	/// Implements the <see cref="HttpApplication"/> class for this web application.
	/// </summary>
	public class Project : HttpApplication
	{
		private const string ConfigWatchName = "ProjectConfigurationChangeWatch";
		private static readonly ILog log = LogManager.GetLogger(typeof(Project).FullName);
		private static readonly string[] threadNamePrefixes = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

		private static DateTime? assemblyDate;
		private static ProjectConfiguration configuration = ProjectConfiguration.Create();
		private static Exception initializationError;
		private static ProblemInfo initializationProblemInfo;
		private static List<Assembly> relevantAssemblies;
		private static IList<string> installOrder;
		private static OrderedDictionary<string, ExtensionInfo> extensions;
		private static IList<Type> modules;

		private static int threadPrefixIndex;

		/// <summary>
		/// Gets the last modification date of the current assembly.
		/// </summary>
		/// <value>The assembly date.</value>
		public static DateTime AssemblyDate
		{
			get
			{
				if (assemblyDate == null)
					assemblyDate = Util.GetAssemblyDate(typeof(Project).Assembly) ?? new DateTime(DateTime.Now.Ticks);

				return assemblyDate.Value;
			}
		}

		/// <summary>
		/// Gets the path to the directory that contains the source Sage assembly.
		/// </summary>
		public static string AssemblyCodeBaseDirectory
		{
			get
			{
				return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			}
		}

		/// <summary>
		/// Gets the path to the directory that contains the runtime copy of the Sage assembly
		/// (the .net <c>temp</c> directory).
		/// </summary>
		public static string AssemblyLocationDirectory
		{
			get
			{
				return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);
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

		/// <summary>
		/// Gets a list with the current assembly and all assemblies loaded from the <see cref="AssemblyCodeBaseDirectory"/> that
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
							var files = Directory.GetFiles(AssemblyCodeBaseDirectory, "*.dll", SearchOption.AllDirectories);
							log.DebugFormat("Scanning for dependent assmblies in '{0}'", AssemblyCodeBaseDirectory);
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

		internal static IList<string> InstallOrder
		{
			get
			{
				return Project.installOrder;
			}
		}

		internal static OrderedDictionary<string, ExtensionInfo> Extensions
		{
			get
			{
				return extensions;
			}
		}

		/// <summary>
		/// Registers the module.
		/// </summary>
		/// <param name="module">The type of the module to register.</param>
		public static void RegisterModule(Type module)
		{
			if (module == null)
				return;

			if (!typeof(IHttpModule).IsAssignableFrom(module))
			{
				log.ErrorFormat("The module type '{0}' is not an instance of IHttpModule", module.FullName);
				return;
			}

			if (modules == null)
				modules = new List<Type>();

			modules.Add(module);
		}

		private bool IsRequestAvailable
		{
			get
			{
				try
				{
					// ReSharper disable ConditionIsAlwaysTrueOrFalse
					// HttpContext might not be nullable, but accessing it may well generate an exception
					return this.Context != null && this.Context.Request != null;
					// ReSharper restore ConditionIsAlwaysTrueOrFalse
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

		internal static void Start(HttpContextBase httpContext)
		{
			if (Thread.CurrentThread.Name == null)
			{
				Thread.CurrentThread.Name = Project.GenerateThreadId(true);
				log.InfoFormat("Thread name set to {0}", Thread.CurrentThread.Name);
			}

			log.InfoFormat("Application started");

			IControllerFactory controllerFactory = new SageControllerFactory();
			Initialize(controllerFactory, new SageContext(httpContext));
		}

		/// <summary>
		/// Handles the Start event of the Application control.
		/// </summary>
		protected virtual void Application_Start()
		{
			Project.Start(new HttpContextWrapper(this.Context));
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

			initializationError = null;
		}

		/// <summary>
		/// Handles the BeginRequest event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Application_BeginRequest(object sender, EventArgs e)
		{
			if (Thread.CurrentThread.Name == null)
			{
				Thread.CurrentThread.Name = Project.GenerateThreadId();
				log.InfoFormat("Thread name set to {0}", Thread.CurrentThread.Name);
			}

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

			if (this.Context != null)
				log.InfoFormat("Request {0} started.", HttpContext.Current.Request.Url);
			else
				log.InfoFormat("Request started (no context)");
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

			var startTime = long.Parse(Thread.CurrentThread.Name.ReplaceAll(@"[^\d]|", string.Empty));
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
			string systemConfigPath = Path.Combine(AssemblyCodeBaseDirectory, ProjectConfiguration.SystemConfigName);
			string projectConfigPathBinDir = Path.Combine(AssemblyCodeBaseDirectory, ProjectConfiguration.ProjectConfigName);
			string projectConfigPathProjDir = Path.Combine(AssemblyCodeBaseDirectory, "..\\" + ProjectConfiguration.ProjectConfigName);

			string projectConfigPath = projectConfigPathBinDir;
			if (File.Exists(projectConfigPathProjDir))
			{
				projectConfigPath = projectConfigPathProjDir;
			}

			var projectConfig = ProjectConfiguration.Create();

			if (!File.Exists(projectConfigPath) && !File.Exists(systemConfigPath))
			{
				log.Warn("Nither system nor project configuration files found; configuration initialized with default values");

				//// initializationProblemInfo = new ProblemInfo(ProblemType.MissingConfigurationFile);
				//// initializationError = new SageHelpException(initializationProblemInfo);
				return;
			}

			installOrder = new List<string>();
			extensions = new OrderedDictionary<string, ExtensionInfo>();

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

				// this will ensure the new context uses the just
				// created configuration immediately
				context = new SageContext(context);

				var extensionManager = new ExtensionManager();
				try
				{
					extensionManager.Initialize(context);
				}
				catch (ProjectInitializationException ex)
				{
					initializationError = ex;
					initializationProblemInfo = new ProblemInfo(ex.Reason, ex.SourceFile);
					if (ex.Reason == ProblemType.MissingExtensionDependency)
					{
						initializationProblemInfo.InfoBlocks
							.Add("Dependencies", ex.Dependencies.ToDictionary(name => name));
					}
				}

				if (initializationError == null)
				{
					var missingDependencies = projectConfig.Dependencies
						.Where(name => extensionManager.Count(ex => ex.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) == 0)
						.ToList();

					if (missingDependencies.Count != 0)
					{
						string errorMessage = 
							string.Format("Project is missing one or more dependencies ({0}) - installation cancelled.",
							string.Join(", ", missingDependencies));

						initializationError = new ProjectInitializationException(errorMessage);
						initializationProblemInfo = new ProblemInfo(ProblemType.MissingDependency);
						initializationProblemInfo.InfoBlocks
							.Add("Dependencies", missingDependencies.ToDictionary(name => name));
					}

					foreach (var extension in extensionManager)
					{
						installOrder.Add(extension.Config.Id);
						Project.RelevantAssemblies.AddRange(extension.Assemblies);
						projectConfig.RegisterExtension(extension.Config);
						extensions.Add(extension.Config.Id, extension);
					}

					installOrder.Add(projectConfig.Id);
					projectConfig.RegisterRoutes();
					context.LmCache.Put(ConfigWatchName, DateTime.Now, projectConfig.Files);
				}
			}
		}

		private static string GenerateThreadId(bool appStart = false)
		{
			string prefix;
			if (appStart)
				prefix = "INIT";
			else
			{
				if (Project.threadPrefixIndex < threadNamePrefixes.Length - 1)
					Project.threadPrefixIndex += 1;
				else
					Project.threadPrefixIndex = 0;

				prefix = threadNamePrefixes[Project.threadPrefixIndex];
			}

			return string.Format("{0}-{1}", prefix, DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
		}
	}
}
