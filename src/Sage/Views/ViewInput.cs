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
	using System.Linq;
	using System.Xml;

	using Kelp;
	using log4net;
	using Sage.Modules;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements a class that handles per-request input XML handling.
	/// </summary>
	public class ViewInput
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ViewInput).FullName);
		private readonly OrderedDictionary<string, ModuleConfiguration> modules = new OrderedDictionary<string, ModuleConfiguration>();
		private readonly SageContext context;

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
		}

		public string Action { get; private set; }

		public List<Resource> Resources
		{
			get
			{
				var result = new List<Resource>();
				var config = context.ProjectConfiguration;
				var currentPath = context.Request.Path;

				var matchingLibs = new List<ResourceLibraryInfo>(
					config.ResourceLibraries.Values.Where(l => l.MatchesPath(currentPath)));

				foreach (ResourceLibraryInfo library in matchingLibs)
					result.AddRange(library.Resources);

				foreach (ModuleConfiguration module in this.modules.Values)
				{
					foreach (string name in module.Libraries)
					{
						if (!config.ResourceLibraries.ContainsKey(name))
						{
							log.ErrorFormat("Module '{0}' is referencing non-existent library '{1}'", module.Name, name);
							continue;
						}

						ResourceLibraryInfo library = config.ResourceLibraries[name];
						result.AddRange(library.Resources);
					}

					result.AddRange(module.Resources);
				}

				return result.Distinct().ToList();
			}
		}

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

		public bool IsMissingData<T>()
		{
			ModuleConfiguration config = SageModuleFactory.Modules.Values.FirstOrDefault(c => c.Type == typeof(T));
			if (config != null)
			{
				return 
					this.ModuleResults.ContainsKey(config.Name) &&
					this.ModuleResults[config.Name].Count(r => r.Status == ModuleResultStatus.NoData) == 0;
			}

			return true;
		}

		internal void AddModuleResult(string moduleTagName, ModuleResult result)
		{
			if (!this.ModuleResults.ContainsKey(moduleTagName))
				this.AddModuleType(moduleTagName);

			this.ModuleResults[moduleTagName].Add(result);
		}

		private void AddModuleType(string moduleTagName)
		{
			this.ModuleResults.Add(moduleTagName, new List<ModuleResult>());
			if (this.modules.ContainsKey(moduleTagName))
				return;

			// add the module and all its references to this object's module dictionary
			ModuleConfiguration moduleConfig = SageModuleFactory.GetModuleConfiguration(moduleTagName);
			this.modules.Add(moduleTagName, moduleConfig);
			foreach (string name in moduleConfig.Dependencies.Where(n => !modules.ContainsKey(n)))
			{
				if (!context.ProjectConfiguration.Modules.ContainsKey(name))
				{
					log.ErrorFormat("Module '{0}' referenced from module '{1}' was not found.",
						name, moduleConfig.Name);

					continue;
				}

				this.modules.Add(name, context.ProjectConfiguration.Modules[name]);
			}

			// reorder the modules by dependencies
			List<ModuleConfiguration> temp = new List<ModuleConfiguration>(modules.Values);
			int index = 0;
			while (index < modules.Count)
			{
				ModuleConfiguration module = temp[index];
				bool changed = false;
				foreach (string name in module.Dependencies)
				{
					if (!context.ProjectConfiguration.Modules.ContainsKey(name))
						continue;

					ModuleConfiguration reference = context.ProjectConfiguration.Modules[name];
					int other = temp.IndexOf(reference);
					if (other > index)
					{
						temp[index] = reference;
						temp[other] = module;
						changed = true;
						break;
					}
				}

				if (!changed)
					index++;
			}

			modules.Clear();
			foreach (ModuleConfiguration module in temp)
			{
				modules.Add(module.Name, module);
			}
		}
	}
}
