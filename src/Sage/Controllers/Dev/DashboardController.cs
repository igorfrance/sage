namespace Sage.Controllers.Dev
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Web.Mvc;
	using System.Xml;

	using Kelp.Core;
	using Kelp.Core.Extensions;
	using Sage.Configuration;
	using Sage.ResourceManagement;
	using Sage.Resources.DevTools;
	using Sage.Routing;

	/// <summary>
	/// Implements a controller that serves the developer tools view.
	/// </summary>
	[SharedController]
	public class DashboardController : SageController
	{
		internal const string DevHome = "dev/";
		internal const string DevDashboard = "dev/dashboard/";
		internal const string DevLogin = "dev/login/";
		internal const string DevCommandList = "dev/commands/";
		internal const string DevSettings = "dev/settings/";
		internal const string DevVariables = "dev/variables/";
		internal const string DevRoutes = "dev/routes/";
		internal const string DevLog = "dev/log/";
		internal const string DevIntl = "dev/intl/";
		internal const string DevViewsTree = "dev/views/tree/{*path}";
		internal const string DevViewsCollect = "dev/views/collect";

		internal const string ApiDocDirNet = "api/net";
		internal const string ApiDocDirJs = "api/js";

		private const string Username = "ri0t";
		private const string Password = "9764315";

		private static Dictionary<string, Dictionary<string, ControllerViewInfo>> categoryPages =
			new Dictionary<string, Dictionary<string, ControllerViewInfo>>();

		private bool AreDevToolsInstalled
		{
			get
			{
				return PackageManager.GetMissingResources(Context.Path.PhysicalSharedViewPath).Count == 0;
			}
		}

		/// <summary>
		/// Services requests for the index view.
		/// </summary>
		/// <returns>The result of processing this view.</returns>
		[UrlRoute(Path = DevHome)]
		public ActionResult Index()
		{
			if (!AreDevToolsInstalled)
				return RedirectToRoute(InstallationController.DevToolsInstallationHome);

			if (!Context.IsDeveloperRequest)
				return RedirectToAction("login");

			var inputData = new XmlDocument();
			var validCategories = inputData.CreateElement("categories");
			var categories = this.GetValidCategories();

			foreach (var category in categories)
			{
				var elem = validCategories.AppendElement(inputData.CreateElement("category"));
				var context = new SageContext(this.Context, category.Name);
				
				elem.SetAttribute("name", category.Name);
				elem.SetAttribute("link", context.Url.PrefixUrl("/"));
			}

			var docsDirectories = inputData.CreateElement("docs");
			var netDocs = docsDirectories.AppendElement("net");
			var jscDocs = docsDirectories.AppendElement("js");

			netDocs.SetAttribute("path", ApiDocDirNet);
			netDocs.SetAttribute("exists", Directory.Exists(Context.MapPath(ApiDocDirNet)) ? "1" : "0");

			jscDocs.SetAttribute("path", ApiDocDirJs);
			jscDocs.SetAttribute("exists", Directory.Exists(Context.MapPath(ApiDocDirJs)) ? "1" : "0");

			ViewData["categories"] = validCategories;
			ViewData["docs"] = docsDirectories;

			return View("index");
		}

		/// <summary>
		/// Services requests for the dashboard view.
		/// </summary>
		/// <returns>The result of processing this view.</returns>
		[UrlRoute(Path = DevDashboard)]
		public ActionResult Dashboard()
		{
			if (!AreDevToolsInstalled)
				return RedirectToRoute(InstallationController.DevToolsInstallationHome);

			if (!Context.IsDeveloperRequest)
				return RedirectToAction("login");

			string page = Request.QueryString["page"];
			string tool = Request.QueryString["tool"];

			if (string.IsNullOrEmpty(page))
				page = Context.Url.PrefixUrl(DevVariables);

			ViewData["pageToLoad"] = page;
			ViewData["toolToLoad"] = tool;
			ViewData["customTools"] = GetCustomToolCommands(new XmlDocument());

			return View("dashboard");
		}

		/// <summary>
		/// Services requests for the index view.
		/// </summary>
		/// <returns>The result of processing this view.</returns>
		[UrlRoute(Path = DevCommandList)]
		public ActionResult CommandList()
		{
			if (!Context.IsDeveloperRequest)
				return RedirectToAction("login");

			return new ContentResult 
				{ 
					Content = "<commands/>", 
					ContentType = "text/xml", 
					ContentEncoding = Encoding.UTF8 
				};
		}

		/// <summary>
		/// Services requests for the login view.
		/// </summary>
		/// <returns>The result of processing this view.</returns>
		[UrlRoute(Path = DevLogin)]
		public ActionResult Login()
		{
			if (Request.HttpMethod == "POST")
			{
				if (Request.Form["username"] == Username && Request.Form["password"] == Password)
				{
					Session["developer"] = true;
					return RedirectToAction("index");
				}
			}

			if (!AreDevToolsInstalled)
				return RedirectToRoute(InstallationController.DevToolsInstallationHome);

			return View("login");
		}

		/// <summary>
		/// Services requests for the settings view.
		/// </summary>
		/// <returns>The result of processing this view.</returns>
		[UrlRoute(Path = DevSettings)]
		public ActionResult Settings()
		{
			if (!AreDevToolsInstalled)
				return RedirectToRoute(InstallationController.DevToolsInstallationHome);

			if (!Context.IsDeveloperRequest)
				return RedirectToAction("login");

			return View("settings");
		}

		/// <summary>
		/// Services requests for the variables view.
		/// </summary>
		/// <returns>The result of processing this view.</returns>
		[UrlRoute(Path = DevVariables)]
		public ActionResult Variables()
		{
			if (!AreDevToolsInstalled)
				return RedirectToRoute(InstallationController.DevToolsInstallationHome);

			if (!Context.IsDeveloperRequest)
				return RedirectToAction("login");

			var links = new QueryString();
			ProjectConfiguration config = ProjectConfiguration.Current;
			foreach (CategoryInfo category in config.Categories.Values)
			{
				SageContext ctx = new SageContext(Context, category.Name);
				if (CategoryExists(category.Name))
					links.Add(category.Name, ctx.Url.PrefixUrl(DevVariables));
			}

			ViewData["links"] = links.ToXml(new XmlDocument(), "links");
			ViewData["variables"] = GetResolvedCategoryVariables();

			return View("variables");
		}

		/// <summary>
		/// Serves the folder contents of the specified path as an xml document.
		/// </summary>
		/// <param name="path">The pat of the folder whose contents should be shown.</param>
		/// <returns>The action result of running this method.</returns>
		[UrlRoute(Path = DevViewsTree)]
		public ActionResult ServeSavedViewTree(string path)
		{
			return new ContentResult
			{
				Content = "<root/>",
				ContentType = "text/xml",
				ContentEncoding = Encoding.UTF8
			};
		}

		private XmlElement GetCustomToolCommands(XmlDocument ownerDoc)
		{
			XmlElement commands = ownerDoc.CreateElement("commands");
			AddCustomViewCommand(commands, "settings", "Application settings", DevSettings);
			AddCustomViewCommand(commands, "variables", "Context variables", DevVariables);
			AddCustomViewCommand(commands, "router", "Route debugger", DevRoutes);
			AddCustomViewCommand(commands, "viewmanager", "Saved view manager", DevViewsCollect);

			return commands;
		}

		private void AddCustomViewCommand(XmlNode ownerNode, string viewId, string viewTitle, string viewPath)
		{
			var commandNode = ownerNode.AppendElement(ownerNode.OwnerDocument.CreateElement("command"));
			commandNode.SetAttribute("id", viewId);
			commandNode.SetAttribute("title", viewTitle);
			commandNode.SetAttribute("url", Context.Url.PrefixUrl(viewPath));
		}

		private XmlNode GetResolvedCategoryVariables()
		{
			CategoryConfiguration categoryConfig = Context.CategoryConfiguration;
			if (categoryConfig == null || categoryConfig.VariablesElement == null)
				return null;

			XmlDocument doc = categoryConfig.VariablesElement.OwnerDocument;
			XmlNode result = categoryConfig.VariablesElement.CloneNode(true);
			foreach (XmlElement varDefinition in result.SelectNodes("//intl:variable", XmlNamespaces.Manager))
				varDefinition.AppendChild(doc.CreateElement("resolved"));

			XmlNode variables = doc.CreateElement("resolved");
			foreach (XmlElement varDefinition in categoryConfig.VariablesElement.SelectNodes("*"))
			{
				XmlElement varElement = variables.AppendElement(doc.CreateElement("variable"));
				varElement.SetAttribute("name", varDefinition.GetAttribute("id"));
				varElement.AppendElement(doc.CreateElement("intl:variable", XmlNamespaces.InternationalizationNamespace))
					.SetAttribute("ref", varDefinition.GetAttribute("id"));
			}

			CategoryInfo categoryInfo = ProjectConfiguration.Current.Categories[categoryConfig.Name];
			DictionaryFileCollection dictionaries = new DictionaryFileCollection(Context);
			foreach (string locale in categoryInfo.Locales)
			{
				XmlDocument subject = new XmlDocument();
				subject.LoadXml(variables.OuterXml);

				StringBuilder sb = new StringBuilder();
				XmlWriter writer = XmlWriter.Create(sb);

				Globalizer globalizer = new Globalizer(this.Context);
				globalizer.Transform(subject, categoryConfig, dictionaries, locale, writer, GlobalizeType.Diagnose);
				writer.Close();

				XmlElement temp = doc.CreateElement("temp");
				temp.InnerXml = sb.ToString();

				foreach (XmlElement e in temp.SelectNodes("//intl:variable", XmlNamespaces.Manager))
				{
					string varName = e.GetAttribute("ref");
					string varSource = e.GetAttribute("source");

					XmlElement resolved = result.SelectSingleElement(string.Format("//intl:variable[@id='{0}']/resolved", varName), XmlNamespaces.Manager);
					XmlElement value = resolved.AppendElement(doc.CreateElement("value"));
					value.SetAttribute("locale", locale);
					value.SetAttribute("source", varSource);
					value.InnerText = e.InnerText;
				}
			}

			return result;
		}

		private IEnumerable<CategoryInfo> GetValidCategories()
		{
			var result = new List<CategoryInfo>();

			foreach (var category in ProjectConfiguration.Current.Categories.Values)
			{
				var context = new SageContext(this.Context, category.Name);
				if (CategoryExists(category.Name))
				{
					result.Add(category);
				}
			}

			return result;
		}

		private Dictionary<string, ControllerViewInfo> GetCategoryPages(string category)
		{
			if (categoryPages.ContainsKey(category))
				return categoryPages[category];

			var result = new Dictionary<string, ControllerViewInfo>();

			string[] directories = Directory.GetDirectories(Context.Path.PhysicalViewPath, "*", SearchOption.TopDirectoryOnly);
			foreach (string directory in directories)
			{
				if ((new DirectoryInfo(directory).Attributes & FileAttributes.Hidden) != 0)
					continue;

				var controllerName = Path.GetFileName(directory);
				var info = new ControllerViewInfo(controllerName);
				var files = Directory.GetFiles(directory, "*.xml", SearchOption.TopDirectoryOnly);

				foreach (ResourceName name in files
					.Select(filePath => new ResourceName(filePath, category))
					.Where(name => !info.Views.Contains(name.FileName)))
				{
					info.Views.Add(name.FileName);
				}

				if (info.Views.Count != 0)
					result.Add(controllerName, info);
			}

			return result;

		}

		private bool CategoryExists(string category)
		{
			return Directory.Exists(Context.Path.GetPhysicalCategoryPath(category));
		}

		private class ControllerViewInfo
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="Configuration.ControllerViewInfo"/> class.
			/// </summary>
			/// <param name="controller">The controller.</param>
			public ControllerViewInfo(string controller)
			{
				this.Controller = controller;
				this.Views = new List<string>();
			}

			/// <summary>
			/// Gets the controller part of the struct.
			/// </summary>
			/// <value>The controller.</value>
			public string Controller { get; private set; }

			/// <summary>
			/// Gets the views that exist for the controller .
			/// </summary>
			/// <value>The views.</value>
			public List<string> Views { get; private set; }
		}

	}
}
