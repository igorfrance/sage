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
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;
	using Kelp.ResourceHandling;

	using log4net;
	using ResourceLocation=Sage.ResourceLocation;
	using Sage.Extensibility;
	using Sage.Modules;
	using Sage.ResourceManagement;
	using Sage.Views;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Provides a base class for all controllers within the application.
	/// </summary>
	public abstract class SageController : Controller, IModuleFactory
	{
		internal const string ParamNameMetaView = "view";
		internal const string DefaultController = "home";
		internal const string DefaultAction = "index";

		private static readonly ILog log = LogManager.GetLogger(typeof(SageController).FullName);
		private static readonly List<ViewXmlFilter> xmlFilters = new List<ViewXmlFilter>();

		private readonly ControllerMessages messages = new ControllerMessages();
		private readonly IModuleFactory moduleFactory = new SageModuleFactory();
		private readonly Dictionary<string, ViewInfo> viewInfos = new Dictionary<string, ViewInfo>();
		private readonly Dictionary<ResourceLocation, List<Resource>> resources = new Dictionary<ResourceLocation, List<Resource>>
		{
			{ ResourceLocation.Head, new List<Resource>() },
			{ ResourceLocation.Data, new List<Resource>() },
			{ ResourceLocation.Body, new List<Resource>() }
		};

		private readonly string typeName;

		static SageController()
		{
			SageController.DiscoverViewXmlFilters();
			Project.AssembliesUpdated += SageController.OnAssembliesUpdated;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageController"/> class.
		/// </summary>
		protected SageController()
		{
			typeName = this.GetType().Name.Replace("Controller", string.Empty).ToLower();
			messages = new ControllerMessages();
			this.ViewData["messages"] = messages;
			this.IsShared = this.GetType().GetCustomAttributes(typeof(SharedControllerAttribute), true).Count() != 0;
		}

		/// <summary>
		/// Gets the <see cref="SageContext"/> with which this controller runs.
		/// </summary>
		public SageContext Context { get; private set; }

		/// <summary>
		/// Gets or sets the list of files that this document consists of or depends on.
		/// </summary>
		public List<string> Dependencies { get; protected set; }

		/// <summary>
		/// Gets the name of this controller.
		/// </summary>
		public virtual string Name
		{
			get
			{
				return typeName;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this is a shared controller.
		/// </summary>
		/// <remarks>
		/// Shared controller is useful with multi-category projects. A shared controller's view are opened from
		/// <see cref="PathResolver.SharedViewPath"/> instead of from <see cref="PathResolver.ViewPath"/>. This makes
		/// it possible to have create views that are shared across all categories in a multi-category project.
		/// </remarks>
		internal bool IsShared { get; private set; }

		/// <summary>
		/// Gets the path template of the directory from which this controller view templates and/or configuration should be loaded.
		/// </summary>
		internal string ViewPathTemplate
		{
			get
			{
				var viewPath = this.IsShared ? this.Context.Path.SharedViewPath : this.Context.Path.ViewPath;
				var extensionInfo = Project.GetExtension(this);
				if (extensionInfo != null)
				{
					viewPath = Regex.Replace(viewPath, "^~?/", string.Format("{0}{1}/", 
						this.Context.Path.Substitute(this.Context.ProjectConfiguration.PathTemplates.Extension), 
						extensionInfo.Name));
				}

				return viewPath;
			}
		}

		/// <summary>
		/// Sets the HTTP status to not found (404) and returns an <see cref="EmptyResult" />.
		/// </summary>
		/// <param name="action">Optional name of the action that could not be found.</param>
		/// <returns>Empty result</returns>
		public ActionResult PageNotFound(string action = null)
		{
			this.Context.Response.StatusCode = 404;
			this.Context.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
			this.Context.Response.Cache.SetNoStore();

			var view404 = new ViewInfo(this, "404", "_system");
			if (view404.Exists)
			{
				var info = new ViewInfo(this, "404", "_system");
				viewInfos.Add("404", info);

				ViewModel result = this.GetViewModel(info);
				return this.View(info.Action, result);
			}

			if (this.Context.ProjectConfiguration.ErrorViews.ContainsKey("404"))
			{
				var view = this.Context.ProjectConfiguration.ErrorViews["404"];
				var content = this.Context.IsDeveloperRequest ? view.DeveloperContent : view.DefaultContent;
				content = this.Context.ProcessText(content);

				var errorMessage = content.Substitute(new NameValueCollection
				{ 
					{ "path", this.Context.Request.Path },
					{ "controller", this.Name },
					{ "action", action },
				});

				return new ContentResult { Content = errorMessage, ContentType = "text/html" };
			}

			return new EmptyResult();
		}

		/// <summary>
		/// Processes the view configuration associated with the specified <paramref name="viewName"/>, 
		/// and returns an <see cref="ActionResult"/>.
		/// </summary>
		/// <param name="viewName">The name of the view that should be rendered to response.</param>
		/// <returns>The action result.</returns>
		public ActionResult SageView(string viewName)
		{
			ViewInfo viewInfo = new ViewInfo(this, viewName);
			return this.SageView(viewInfo);
		}

		/// <summary>
		/// Processes the view descibed with the specified <paramref name="viewInfo"/>, 
		/// and returns an <see cref="ActionResult"/>.
		/// </summary>
		/// <param name="viewInfo">The object that contains information about the view.</param>
		/// <returns>The action result.</returns>
		public ActionResult SageView(ViewInfo viewInfo)
		{
			log.DebugFormat("Running view '{0}' on controller '{1}'.", viewInfo.Action, this.Name);

			if (!viewInfo.Exists)
			{
				log.FatalFormat("The specified view name '{0}' doesn't exist.", viewInfo);
				return this.PageNotFound();
			}

			var cache = this.Context.ViewCache;
			var caching = this.Context.ProjectConfiguration.ViewCaching;
			var checkCache = !viewInfo.IsNoCacheView && !this.Context.IsNoCacheRequest && caching.Enabled;
			var localName = this.Context.LocalPath;

			if (checkCache)
			{
				var cacheDate = cache.GetLastModified(localName, ViewCache.HtmlGroup);
				var isCached = cacheDate != null && cacheDate.Value > viewInfo.LastModified;

				if (isCached)
				{
					var time = DateTime.Now;
					var cachedContent = cache.Get(localName, ViewCache.HtmlGroup);
					log.DebugFormat("View {0} loaded from cache in {1}ms", localName, (DateTime.Now - time).Milliseconds);
					return new ContentResult
					{
						Content = cachedContent,
						ContentType = "text/html",
						ContentEncoding = Encoding.UTF8
					};
				}

			}

			ViewModel model = this.GetViewModel(viewInfo);
			return this.View(model.Action, model);
		}

		/// <summary>
		/// Processes the view configuration associated with the specified <paramref name="viewName"/>, 
		/// and returns a <see cref="ViewModel"/> instance that contains the result.
		/// </summary>
		/// <param name="viewName">The name of the view to process.</param>
		/// <returns>
		/// An object that contains the result of processing the view configuration
		/// </returns>
		public virtual ViewModel GetViewModel(string viewName)
		{
			return this.GetViewModel(new ViewInfo(this, viewName));
		}

		/// <summary>
		/// Processes the view configuration associated with the specified <paramref name="viewInfo"/>, 
		/// and returns a <see cref="ViewModel"/> instance that contains the result.
		/// </summary>
		/// <param name="viewInfo">The object that contains information about the view.</param>
		/// <returns>
		/// An object that contains the result of processing the view configuration
		/// </returns>
		public virtual ViewModel GetViewModel(ViewInfo viewInfo)
		{
			try
			{
				var startTime = DateTime.Now.Ticks;
				ViewConfiguration config = ViewConfiguration.Create(this, viewInfo);
				var result = config.Process();

				var elapsed = new TimeSpan(DateTime.Now.Ticks - startTime);
				log.DebugFormat("Completed processing view {0} in {1}ms", viewInfo.Action, elapsed.Milliseconds);
				return result;
			}
			catch (Exception ex)
			{
				throw new SageHelpException(new ProblemInfo(ProblemType.ViewProcessingError), ex);
			}
		}

		/// <inheritdoc/>
		public virtual IModule CreateModule(XmlElement moduleElement)
		{
			return moduleFactory.CreateModule(moduleElement);
		}

		/// <summary>
		/// Wraps the previously processed view configuration input XML with the standard XML envelope that contains 
		/// information about the current request, and the resources referenced by the modules and libraries in use by the 
		/// the view.
		/// </summary>
		/// <param name="viewContext">The view context that contains the <see cref="ViewModel"/> that resulted from
		/// previously processing the view configuration.</param>
		/// <returns>
		/// The actual XML document that will be used as input for the final XSLT transform.
		/// </returns>
		public XmlDocument PrepareViewXml(ViewContext viewContext)
		{
			var startTime = DateTime.Now.Ticks;
			ViewModel model = viewContext.ViewData.Model as ViewModel;

			string action = "action";
			if (model != null)
			{
				action = model.ViewConfiguration.Name;
			}
			else if (this.ViewData["Action"] != null)
			{
				action = this.ViewData["Action"].ToString();
			}

			XmlDocument result = new XmlDocument();
			XmlElement viewRoot = result.AppendElement("sage:view", XmlNamespaces.SageNamespace);
			viewRoot.SetAttribute("controller", this.Name);
			viewRoot.SetAttribute("action", action);
			viewRoot.AppendElement(this.Context.ToXml(result));

			log.DebugFormat("Document create");
			XmlElement responseNode = viewRoot.AppendElement("sage:response", XmlNamespaces.SageNamespace);

			if (messages.Count > 0)
			{
				XmlElement messagesNode = responseNode.AppendElement("sage:messages", XmlNamespaces.SageNamespace);
				foreach (var message in messages)
				{
					messagesNode.AppendChild(message.ToXml(result));
				}
			}

			try
			{
				if (model?.ConfigNode != null)
				{
					//// make sure resources are ordered properly:
					////  1. by resource type (icon, style, script)
					////  2. by project dependency
					////  3. by module dependency
					var installOrder = Project.InstallOrder;
					var inputResources = model.Resources
						.Where(r => r.IsValidFor(this.Context.UserAgentID))
						.OrderBy((r1, r2) =>
							{
								if (r1.Type != r2.Type)
									return r1.Type.CompareTo(r2.Type);

								if (r1.ProjectId == r2.ProjectId)
									return model.Resources.IndexOf(r1).CompareTo(model.Resources.IndexOf(r2));

								return installOrder.IndexOf(r1.ProjectId).CompareTo(installOrder.IndexOf(r2.ProjectId));
							})
						.ToList();

					log.DebugFormat("Resources ordered");
					if (inputResources.Count != 0)
					{
						XmlElement resourceRoot = responseNode.AppendElement("sage:resources", XmlNamespaces.SageNamespace);

						List<Resource> headResources = inputResources.Where(r => r.Location == Sage.ResourceLocation.Head)
							.Union(this.resources[ResourceLocation.Head]).ToList();
						List<Resource> bodyResources = inputResources.Where(r => r.Location == Sage.ResourceLocation.Body)
							.Union(this.resources[ResourceLocation.Body]).ToList();
						List<Resource> dataResources = inputResources.Where(r => r.Location == Sage.ResourceLocation.Data)
							.Union(this.resources[ResourceLocation.Data]).ToList();

						if (dataResources.Count != 0)
						{
							XmlNode dataNode = resourceRoot.AppendElement("sage:data", XmlNamespaces.SageNamespace);
							foreach (Resource resource in dataResources)
							{
								var time = DateTime.Now;
								dataNode.AppendChild(resource.ToXml(result, this.Context));
								var elapsed = (DateTime.Now - time).Milliseconds;
								if (elapsed > 0)
									log.DebugFormat("Added '{0}' to sage:data in {1}ms", resource.Name, elapsed);
							}
						}

						if (headResources.Count != 0)
						{
							XmlNode headNode = resourceRoot.AppendElement("sage:head", XmlNamespaces.SageNamespace);
							foreach (Resource resource in headResources)
							{
								var time = DateTime.Now;
								headNode.AppendChild(resource.ToXml(result, this.Context));
								var elapsed = (DateTime.Now - time).Milliseconds;
								if (elapsed > 0)
									log.DebugFormat("Added '{0}' to sage:head in {1}ms", resource.Name.Or(resource.Path), elapsed);
							}
						}

						if (bodyResources.Count != 0)
						{
							XmlNode bodyNode = resourceRoot.AppendElement("sage:body", XmlNamespaces.SageNamespace);
							foreach (Resource resource in bodyResources.OrderBy(r => r.Extension))
							{
								var time = DateTime.Now;
								bodyNode.AppendChild(resource.ToXml(result, this.Context));
								var elapsed = (DateTime.Now - time).Milliseconds;
								if (elapsed > 0)
									log.DebugFormat("Added '{0}' to sage:body in {1}ms", resource.Name.Or(resource.Path), elapsed);
							}
						}
					}

					responseNode
						.AppendElement("sage:model", XmlNamespaces.SageNamespace)
						.AppendChild(result.ImportNode(model.ConfigNode, true));
				}

				log.DebugFormat("Resources added");
			}
			catch (Exception ex)
			{
				throw new SageHelpException(new ProblemInfo(ProblemType.ResourceProcessingError), ex);
			}

			var viewDataItems = this.ViewData.Keys.Where(k => k != "messages").ToList();
			foreach (var key in viewDataItems)
			{
				object value = viewContext.ViewData[key];
				if (value == null)
					continue;

				if (value is XmlNode)
				{
					XmlNode valueNode = (XmlNode) value;
					if (valueNode.NodeType == XmlNodeType.Document)
					{
						XmlDocument doc = (XmlDocument) valueNode;
						XmlNode importedNode = result.ImportNode(((XmlDocument) valueNode).DocumentElement, true);
						if (doc.DocumentElement != null) responseNode.AppendChild(importedNode);
					}
					else
						responseNode.AppendChild(result.ImportNode(valueNode, true));
				}
				else if (value is IXmlConvertible)
				{
					XmlElement valueElement = ((IXmlConvertible) value).ToXml(result);
					if (valueElement != null)
						responseNode.AppendChild(valueElement);
				}
				else
				{
					XmlElement elem = (XmlElement) responseNode.AppendChild(result.CreateElement("sage:value", XmlNamespaces.SageNamespace));
					elem.SetAttribute("id", key);
					elem.InnerText = value.ToString();
				}

				log.DebugFormat("Added '{0}' to sage:response", key);
			}

			try
			{
				var finalResult = this.FilterViewXml(viewContext, result);

				var elapsed = new TimeSpan(DateTime.Now.Ticks - startTime);
				log.DebugFormat("Completed preparing view XML view in {0}ms", elapsed.Milliseconds);

				return finalResult;
			}
			catch (Exception ex)
			{
				throw new SageHelpException(new ProblemInfo(ProblemType.ViewXmlFilteringError), ex);
			}
		}

		/// <summary>
		/// Gets the view info corresponding to this controller and the specified <paramref name="viewName"/>.
		/// </summary>
		/// <param name="viewName">The name of the view for which to get the info.</param>
		/// <returns>
		/// An object that contains information about the view template and configuration file that correspond to 
		/// this controller and the view with the specified <paramref name="viewName"/>.
		/// </returns>
		public virtual ViewInfo GetViewInfo(string viewName)
		{
			if (!viewInfos.ContainsKey(viewName))
				viewInfos.Add(viewName,
					new ViewInfo(this, viewName));

			return viewInfos[viewName];
		}

		/// <summary>
		/// Gets the last modification date for the specified <paramref name="viewName"/>.
		/// </summary>
		/// <remarks>
		/// Each action can and should be cached by the browsers. When subsequent requests come in, browsers will
		/// send the last modification date that they received the last time they got that file, in order for the
		/// server to figure out whether to send a new version. With sage controllers views, the last modification
		/// date is actually the latest modification date of possibly a whole series of files. Those files could be
		/// the XSLT style-sheet itself or any one of its includes, or the XML configuration file or any one of its
		/// includes. Therefore it is necessary to have this extra piece of logic to effectively determine what that
		/// latest modification date is.
		/// </remarks>
		/// <param name="viewName">The name of the action for which to retrieve the last modification date.</param>
		/// <returns>
		/// The last modification date for the view with the specified <paramref name="viewName"/>.
		/// </returns>
		public virtual DateTime? GetLastModificationDate(string viewName)
		{
			ViewInfo info = this.GetViewInfo(viewName);
			return info.LastModified;
		}

		/// <summary>
		/// Provides a hook to initialize the controller from a unit test
		/// </summary>
		/// <param name="requestContext">The request context.</param>
		internal void InitializeForTesting(RequestContext requestContext)
		{
			this.Initialize(requestContext);
		}

		/// <summary>
		/// Filters the specified <paramref name="viewXml"/> by invoking all <see cref="FilterViewXml"/> delegates
		/// that are accessible by the project at the time of initialization.
		/// </summary>
		/// <param name="viewContext">The view context under which this code is executed.</param>
		/// <param name="viewXml">The XML document to filter.</param>
		/// <returns>
		/// The filtered version of the specified <paramref name="viewXml"/>.
		/// </returns>
		protected virtual XmlDocument FilterViewXml(ViewContext viewContext, XmlDocument viewXml)
		{
			foreach (ViewXmlFilter filter in xmlFilters)
			{
				viewXml = filter.Invoke(this, viewContext, viewXml);
			}

			return viewXml;
		}

		protected void AddDocumentResource(string name, XmlDocument document)
		{
			this.AddResource(ResourceLocation.Data, new Resource(name, document));
		}

		protected void AddResource(ResourceLocation location, Resource resource)
		{
			this.resources[location].Add(resource);
		}

		/// <summary>
		/// Adds a message to this controller's message collection
		/// </summary>
		/// <param name="type">The type of the message to add.</param>
		/// <param name="messageText">The message to display.</param>
		/// <param name="formatValues">Optional format values to use for formatting the message text.</param>
		protected void AddMessage(MessageType type, string messageText, params string[] formatValues)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(messageText));

			var text = string.Format(messageText, formatValues);
			var message = new ControllerMessage { Type = type, Text = text };
			messages.Add(message);
		}

		/// <summary>
		/// Adds a message to this controller's message collection, using the specified <paramref name="phraseId"/> to get the message
		/// text.
		/// </summary>
		/// <param name="type">The type of the message to add.</param>
		/// <param name="phraseId">The id of the phrase that contains the text associated with this message.</param>
		/// <param name="formatValues">Optional format values to use for formatting the phrase text.</param>
		protected void AddMessagePhrase(MessageType type, string phraseId, params string[] formatValues)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(phraseId));

			var phrase = this.Context.Resources.GetPhrase(phraseId);
			var text = string.Format(phrase, formatValues);
			var message = new ControllerMessage { Type = type, Text = text };
			messages.Add(message);
		}

		/// <summary>
		/// Initializes the controller with a new <see cref="SageContext"/> instance.
		/// </summary>
		/// <param name="requestContext">The request context.</param>
		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);
			this.Context = new SageContext(this.ControllerContext);
		}

		private static void DiscoverViewXmlFilters()
		{
			foreach (Assembly a in Project.RelevantAssemblies.ToList())
			{
				var types = from t in a.GetTypes()
							where t.IsClass && !t.IsAbstract
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods().Where(m => m.IsStatic && m.GetCustomAttributes(typeof(ViewXmlFilterAttribute), false).Count() != 0))
					{
						ViewXmlFilter del;
						try
						{
							del = (ViewXmlFilter) Delegate.CreateDelegate(typeof(ViewXmlFilter), methodInfo);
						}
						catch
						{
							log.ErrorFormat("The method {0} on type {1} marked with attribute {2} doesn't match the required delegate {3}, and will therefore not be registered as an XML filter method",
								methodInfo.Name, type.FullName, typeof(ViewXmlFilterAttribute).Name, typeof(ViewXmlFilter).Name);

							continue;
						}

						xmlFilters.Add(del);
					}
				}
			}
		}

		private static void OnAssembliesUpdated(object sender, EventArgs arg)
		{
			SageController.DiscoverViewXmlFilters();
		}
	}
}
