﻿/**
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
namespace Sage.Views
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using log4net;
	using Sage.Configuration;
	using Sage.Controllers;
	using Sage.Modules;

	/// <summary>
	/// Represents the xml configuration for the controller views.
	/// </summary>
	public class ViewConfiguration
	{
		internal const string ModuleIdPattern = "module{0}";
		internal const string ModuleSelectPattern = ".//mod:{0}[not(ancestor::sage:literal)]";
		internal const string LibrarySelectXpath = ".//sage:library[@ref][not(ancestor::sage:literal)]";

		private readonly XmlElement configElement;
		private static readonly ILog log = LogManager.GetLogger(typeof(ViewConfiguration).FullName);
		private static string moduleSelectXPath;

		private ViewConfiguration(SageController controller, ViewInfo viewInfo)
		{
			Contract.Requires<ArgumentNullException>(controller != null);
			Contract.Requires<ArgumentNullException>(viewInfo != null);

			configElement = viewInfo.ConfigDocument.DocumentElement;

			this.Name = viewInfo.Action;
			this.Controller = controller;
			this.Info = viewInfo;
			this.Modules = new Dictionary<string, IModule>();

			SageModuleFactory factory = new SageModuleFactory();

			if (!string.IsNullOrWhiteSpace(ModuleSelectXPath))
			{
				XmlNodeList moduleNodes = this.Info.ConfigDocument.SelectNodes(ModuleSelectXPath, XmlNamespaces.Manager);
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

		/// <summary>
		/// Gets the name name of the action associated with this view configuration.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the object that provides file system information and access to the source document and XSLT template of this
		/// view configuration.
		/// </summary>
		public ViewInfo Info { get; private set; }

		/// <summary>
		/// Gets the controller that this view configuration is being used by.
		/// </summary>
		public SageController Controller { get; private set; }

		/// <summary>
		/// Gets the current context.
		/// </summary>
		public SageContext Context
		{
			get
			{
				return this.Controller.Context;
			}
		}

		/// <summary>
		/// Gets the root element of this view configuration.
		/// </summary>
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
							foreach (string name in SageModuleFactory.Modules.Keys)
							{
								parts.Add(string.Format(ModuleSelectPattern, name));
							}

							moduleSelectXPath = string.Join("|", parts);
						}
					}
				}

				return moduleSelectXPath;
			}
		}

		internal Dictionary<string, IModule> Modules { get; private set; }

		/// <summary>
		/// Creates a new <see cref="ViewConfiguration"/> instance for the specified <paramref name="controller"/> and
		/// <paramref name="viewInfo"/>.
		/// </summary>
		/// <param name="controller">The controller for which to create the view configuration.</param>
		/// <param name="viewInfo">The object that contains the information about the view.</param>
		/// <returns>
		/// A new <see cref="ViewConfiguration"/> instance for the specified <paramref name="controller"/> and
		/// <paramref name="viewInfo"/>.
		/// </returns>
		public static ViewConfiguration Create(SageController controller, ViewInfo viewInfo)
		{
			return new ViewConfiguration(controller, viewInfo);
		}

		/// <summary>
		/// Discovers any module XML elements in the current configuration, and creates and processed their matching module instances.
		/// </summary>
		/// <returns>
		/// An object that contains the result of processing this configuration.
		/// </returns>
		public ViewInput ExpandConfiguration()
		{
			ViewInput input = new ViewInput(this, configElement);

			if (!string.IsNullOrWhiteSpace(ModuleSelectXPath))
			{
				XmlNodeList moduleNodes = input.ConfigNode.SelectNodes(ModuleSelectXPath, XmlNamespaces.Manager);
				foreach (XmlElement moduleElement in moduleNodes)
				{
					IModule module = this.Controller.CreateModule(moduleElement);
					ModuleResult result = module.ProcessElement(moduleElement, this);

					if (result == null)
					{
						moduleElement.ParentNode.RemoveChild(moduleElement);
					}
					else
					{
						input.AddModuleResult(moduleElement.LocalName, result);
						if (result.ResultElement != null)
						{
							XmlNode newElement = input.ConfigNode.OwnerDocument.ImportNode(result.ResultElement, true);
							moduleElement.ParentNode.ReplaceChild(newElement, moduleElement);
						}
					}
				}
			}

			XmlNodeList libraries = input.ConfigNode.SelectNodes(LibrarySelectXpath, XmlNamespaces.Manager);
			foreach (XmlElement library in libraries)
			{
				string libraryName = library.GetAttribute("ref");
				input.AddViewLibraryReference(libraryName);
				library.ParentNode.RemoveChild(library);
			}

			return input;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Controller.ControllerName);
		}
	}
}