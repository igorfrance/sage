/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Sage.Views
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Extensions;

	using log4net;
	using Sage.Configuration;
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
		/// Gets the name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the info.
		/// </summary>
		public ViewInfo Info { get; private set; }

		/// <summary>
		/// Gets the controller.
		/// </summary>
		public SageController Controller { get; private set; }

		/// <summary>
		/// Gets the context.
		/// </summary>
		public SageContext Context
		{
			get
			{
				return this.Controller.Context;
			}
		}

		/// <summary>
		/// Gets the configuration element.
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
								parts.Add(string.Format(".//mod:{0}", name));
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
		public ViewInput ExpandModules()
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

			return input;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Controller.ControllerName);
		}
	}
}