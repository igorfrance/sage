namespace Sage.Views
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Core.Extensions;

	using Sage.Configuration;

	using log4net;
	using Sage.Controllers;
	using Sage.Modules;

	/// <summary>
	/// Represents the xml configuration for the controller views.
	/// </summary>
	public class ViewConfiguration
	{
		/// <summary>
		/// Formatter string for creating the module Id
		/// </summary>
		internal const string ModuleIdPattern = "module{0}";
		private readonly XmlElement configElement;
		private readonly string name;
		private static readonly ILog log = LogManager.GetLogger(typeof(ViewConfiguration).FullName);
		private static string moduleSelectXPath;

		public ViewConfiguration(string viewName, XmlNode configNode)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(viewName));
			Contract.Requires<ArgumentNullException>(configNode != null);
			
			if (string.IsNullOrEmpty(viewName))
				throw new ArgumentNullException("name");

			if (configNode.NodeType == XmlNodeType.Document)
				configNode = ((XmlDocument)configNode).DocumentElement;

			if (configNode.NodeType != XmlNodeType.Element)
				throw new ArgumentException("The node type of the supplied xml node should be either an element or a document node", "configNode");

			this.name = viewName;
			this.configElement = (XmlElement) configNode;
			this.Modules = new Dictionary<string, IModule>();

			SageModuleFactory factory = new SageModuleFactory();

			if (!string.IsNullOrWhiteSpace(ModuleSelectXPath))
			{
				XmlNodeList moduleNodes = this.configElement.SelectNodes(ModuleSelectXPath, XmlNamespaces.Manager);
				log.DebugFormat("Found {0} module nodes in view configuration.", moduleNodes.Count);

				foreach (XmlElement moduleElem in moduleNodes)
				{
					string moduleId = moduleElem.GetAttribute("id");
					if (string.IsNullOrEmpty(moduleId))
						moduleId = string.Format(ModuleIdPattern, this.Modules.Count);

					if (this.Modules.ContainsKey(moduleId))
						throw new ConfigurationError(string.Format(
							"Duplicate module id: '{0}'. Make sure all modules in the view configuration have unique ids.", moduleId));

					IModule module = factory.CreateModule(moduleElem);
					this.Modules.Add(moduleId, module);
					moduleElem.SetAttribute("id", moduleId);
				}
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
		}

		public XmlElement ConfigurationElement
		{
			get
			{
				return configElement;
			}
		}

		internal static string ModuleSelectXPath
		{
			get
			{
				lock (log)
				{
					if (string.IsNullOrEmpty(moduleSelectXPath))
					{
						lock (log)
						{
							List<string> parts = new List<string>();
							foreach (string name in SageModuleFactory.ModuleTagNames.Keys)
							{
								parts.Add(string.Format(".//mod:{0}", name));
							}

							moduleSelectXPath = parts.Join("|");
						}
					}
				}

				return moduleSelectXPath;
			}
		}

		internal Dictionary<string, IModule> Modules
		{
			get;
			private set;
		}

		public static ViewConfiguration Create(string name, XmlNode inputDoc)
		{
			return new ViewConfiguration(name, inputDoc);
		}

		public ViewInput ProcessRequest(SageController controller)
		{
			ViewInput input = new ViewInput(this.Name, configElement);

			if (!string.IsNullOrWhiteSpace(ModuleSelectXPath))
			{
				foreach (XmlElement moduleElement in input.ConfigNode.SelectNodes(ModuleSelectXPath, XmlNamespaces.Manager))
				{
					IModule module = controller.CreateModule(moduleElement);
					ModuleResult result = module.ProcessRequest(moduleElement, controller.Context);
					input.AddModuleResult(moduleElement.LocalName, result);

					if (result.ResultElement != null)
					{
						XmlNode newElement = input.ConfigNode.OwnerDocument.ImportNode(result.ResultElement, true);
						moduleElement.ParentNode.ReplaceChild(newElement, moduleElement);
					}
				}
			}

			return input;
		}
	}
}