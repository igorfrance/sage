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
namespace Sage.Views
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Xml;

	using Kelp;
	using log4net;
	using Sage.Modules;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements a class that holds the final result of processing a <see cref="ViewConfiguration"/>.
	/// </summary>
	public class ViewInput
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ViewInput).FullName);
		private readonly OrderedDictionary<string, ModuleConfiguration> modules = new OrderedDictionary<string, ModuleConfiguration>();
		private readonly SageContext context;
		private readonly List<Resource> autoResources = new List<Resource>();
		private readonly List<Resource> moduleResources = new List<Resource>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewInput"/> class.
		/// </summary>
		/// <param name="viewConfiguration">The view configuration associated with this view input.</param>
		public ViewInput(ViewConfiguration viewConfiguration)
		{
			Contract.Requires<ArgumentException>(viewConfiguration != null);

			this.context = viewConfiguration.Context;

			this.Action = viewConfiguration.Info.Action;
			this.ViewConfiguration = viewConfiguration;
			this.ModuleResults = new Dictionary<string, List<ModuleResult>>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewInput"/> class.
		/// </summary>
		/// <param name="viewConfiguration">The name of this view.</param>
		/// <param name="viewElement">The configuration element of the view template this class handles.</param>
		public ViewInput(ViewConfiguration viewConfiguration, XmlElement viewElement)
			: this(viewConfiguration)
		{
			Contract.Requires<ArgumentException>(viewElement != null);

			this.ConfigNode = (XmlElement) viewElement.CloneNode(true);

			var config = this.context.ProjectConfiguration;
			var currentPath = this.context.Request.Path;

			var matchingLibs = new List<ResourceLibraryInfo>(
				config.ResourceLibraries.Values.Where(l => l.MatchesPath(currentPath)));

			foreach (ResourceLibraryInfo library in matchingLibs)
			{
				foreach (string libraryRef in library.Dependencies)
				{
					if (!config.ResourceLibraries.ContainsKey(libraryRef))
					{
						log.ErrorFormat("Library '{0}' is referencing a non-existing library '{1}'.", library.Name, libraryRef);
						continue;
					}

					ResourceLibraryInfo referenced = config.ResourceLibraries[libraryRef];
					this.autoResources.AddRange(referenced.Resources);
				}

				this.autoResources.AddRange(library.Resources);
			}
		}

		/// <summary>
		/// Gets the action.
		/// </summary>
		public string Action { get; private set; }

		/// <summary>
		/// Gets the resources.
		/// </summary>
		public List<Resource> Resources
		{
			get
			{
				var result = new List<Resource>(autoResources);
				result.AddRange(moduleResources);

				return result.Distinct().ToList();
			}
		}

		/// <summary>
		/// Gets the view configuration.
		/// </summary>
		public ViewConfiguration ViewConfiguration { get; private set; }

		/// <summary>
		/// Gets the XML configuration node of the view this instance represents.
		/// </summary>
		public XmlElement ConfigNode { get; private set; }

		/// <summary>
		/// Gets the module result status of the controller action that was run.
		/// </summary>
		public ModuleResultStatus ResultStatus
		{
			get
			{
				ModuleResultStatus status = ModuleResultStatus.Ok;
				foreach (var type in ModuleResults.Keys)
				{
					foreach (ModuleResult result in ModuleResults[type])
					{
						if (result.Status > status)
							status = result.Status;
					}
				}

				return status;
			}
		}

		/// <summary>
		/// Gets a dictionary that contains the results of processing each module configured for the current controller.
		/// </summary>
		public Dictionary<string, List<ModuleResult>> ModuleResults { get; private set; }

		internal void AddModuleResult(string moduleTagName, ModuleResult result)
		{
			if (!this.ModuleResults.ContainsKey(moduleTagName))
				this.AddModuleType(moduleTagName);

			this.ModuleResults[moduleTagName].Add(result);
		}

		internal void AddModuleLibraryReference(string libraryName, string moduleName)
		{
			var config = this.context.ProjectConfiguration;
			if (!config.ResourceLibraries.ContainsKey(libraryName))
			{
				log.ErrorFormat("Module '{0}' is referencing non-existent library '{1}'", moduleName, libraryName);
				return;
			}

			AddLibraryReference(libraryName);
		}

		internal void AddViewLibraryReference(string libraryName)
		{
			var config = this.context.ProjectConfiguration;
			if (!config.ResourceLibraries.ContainsKey(libraryName))
			{
				log.ErrorFormat("View '{0}' is referencing non-existent library '{1}'", 
					this.ViewConfiguration.Info.ConfigName, libraryName);

				return;
			}

			AddLibraryReference(libraryName);
		}

		private void AddModuleType(string moduleTagName)
		{
			var config = this.context.ProjectConfiguration;

			this.ModuleResults.Add(moduleTagName, new List<ModuleResult>());
			if (this.modules.ContainsKey(moduleTagName))
				return;

			// add the module and all its references to this object's module dictionary
			ModuleConfiguration moduleConfig = SageModuleFactory.GetModuleConfiguration(moduleTagName);
			this.modules.Add(moduleTagName, moduleConfig);

			foreach (string name in moduleConfig.Dependencies.Where(n => !this.modules.ContainsKey(n)))
			{
				if (!config.Modules.ContainsKey(name))
				{
					log.ErrorFormat("Module '{0}' referenced from module '{1}' was not found.",
						name, moduleConfig.Name);

					continue;
				}

				this.modules.Add(name, this.context.ProjectConfiguration.Modules[name]);
			}

			// reorder the modules by dependencies
			List<ModuleConfiguration> temp = new List<ModuleConfiguration>(this.modules.Values);
			int index = 0;
			while (index < this.modules.Count)
			{
				ModuleConfiguration module = temp[index];
				bool changed = false;
				foreach (string name in module.Dependencies)
				{
					if (!context.ProjectConfiguration.Modules.ContainsKey(name))
						continue;

					ModuleConfiguration reference = context.ProjectConfiguration.Modules[name];
					int other = temp.IndexOf(reference);
					if (other <= index)
						continue;

					temp[index] = reference;
					temp[other] = module;
					changed = true;
					break;
				}

				if (!changed)
					index++;
			}

			this.modules.Clear();
			this.moduleResources.Clear();

			foreach (ModuleConfiguration module in temp)
			{
				this.modules.Add(module.Name, module);
				foreach (string name in module.Libraries)
				{
					this.AddModuleLibraryReference(name, module.Name);
				}

				this.moduleResources.AddRange(module.Resources);
			}
		}

		private void AddLibraryReference(string libraryName)
		{
			var config = this.context.ProjectConfiguration;
			ResourceLibraryInfo library = config.ResourceLibraries[libraryName];
			foreach (string libraryRef in library.Dependencies)
			{
				if (!config.ResourceLibraries.ContainsKey(libraryRef))
				{
					log.ErrorFormat("Library '{0}' is referencing a non-existing library '{1}'.", library.Name, libraryRef);
					continue;
				}

				ResourceLibraryInfo referenced = config.ResourceLibraries[libraryRef];
				this.moduleResources.AddRange(referenced.Resources);
			}

			this.moduleResources.AddRange(library.Resources);
		}
	}
}
