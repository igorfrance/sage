namespace Sage.Controllers.Dev
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using System.Web.Mvc;
	using System.Xml;
	using System.Xml.Xsl;

	using Kelp.Core;
	using Kelp.Core.Extensions;
	
	using Mvp.Xml.Common.Xsl;
	
	using Sage.ResourceManagement;
	using Sage.Resources.DevTools;
	using Sage.Routing;

	/// <summary>
	/// Controls installation of developer tools
	/// </summary>
	public class InstallationController : SageController
	{
		internal const string XsltPath = "sageresx://sage/resources/xslt/installer.xslt";
		internal const string DevInstall = "dev/install";
		internal const string DevToolsInstallationHome = "DevToolsInstallationHome";

		/// <summary>
		/// Serves the index action request.
		/// </summary>
		/// <returns>The action result</returns>
		[UrlRoute(Name = DevToolsInstallationHome, Path = DevInstall)]
		public ActionResult Index()
		{
			if (Request.HttpMethod == "POST")
			{
				PackageManager.InstallResourcesTo(Context.Path.PhysicalSharedViewPath, Context.Path.SharedViewPath);
				return RedirectToAction("Index", "Dashboard");
			}

			string installDir = Context.Path.PhysicalSharedViewPath;

			SortedSet<string> devToolsResourceList = new SortedSet<string>(PackageManager.GetPackageResources(installDir));
			SortedSet<string> devToolsMissingResources = new SortedSet<string>(PackageManager.GetMissingResources(installDir));

			XmlDocument input = new XmlDocument();
			input.LoadXml("<installer><resources/></installer>");

			XmlElement root = input.SelectSingleElement("/installer/resources");

			foreach (var item in devToolsResourceList)
			{
				XmlElement itemNode = root.AppendElement(input.CreateElement("resource"));
				itemNode.InnerText = item.Replace(installDir.Replace("\\", "/"), string.Empty).TrimStart('/');

				if (devToolsMissingResources.Contains(item))
					itemNode.SetAttribute("missing", "yes");
			}

			return new ContentResult { Content = GenerateView(input), ContentType = "text/html" };
		}

		private string GenerateView(XmlDocument input)
		{
			StringBuilder sb = new StringBuilder();
			XmlWriter writer = XmlWriter.Create(sb);

			CacheableXslTransform stylesheet = ResourceManager.LoadXslStylesheet(XsltPath);
			XsltArgumentList arguments = new XsltArgumentList();
			stylesheet.Processor.Transform(input, arguments, writer);
	 
	        return sb.ToString();
		}
	}
}
