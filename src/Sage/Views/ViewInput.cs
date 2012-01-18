namespace Sage.Views
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Collections.Generic;
	using System.Xml;

	using Kelp.Core;

	using Sage.ResourceManagement;

	using log4net;
	using Sage.Configuration;
	using Sage.Modules;

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

		public List<ModuleResource> Resources
		{
			get
			{
				var result = new List<ModuleResource>();
				foreach (ModuleConfiguration module in this.modules.Values)
				{
					foreach (ModuleResource resource in module.Resources)
					{
						if (result.Where(r => r.Path == resource.Path).Count() != 0)
							continue;

						result.Add(resource);
					}
				}

				return result;
			}
		}

		public List<ResourceLibraryInfo> Libraries
		{
			get
			{
				var config = this.ViewConfiguration.Context.ProjectConfiguration;
				var result = new List<ResourceLibraryInfo>(config.ResourceLibraries.Values.Where(l => l.IsGlobal));
				foreach (ModuleConfiguration module in this.modules.Values)
				{
					foreach (string name in module.Libraries)
					{
						if (!config.ResourceLibraries.ContainsKey(name))
							continue;

						if (result.Where(r => r.Name == name).Count() != 0)
							continue;

						result.Add(config.ResourceLibraries[name]);
					}
				}

				return result;
			}
		}

		public ViewConfiguration ViewConfiguration { get; private set; }

		/// <summary>
		/// Gets the XML configuration node of the view this instance represents.
		/// </summary>
		public XmlElement ConfigNode { get; private set; }

		/// <summary>
		/// Gets or sets the module result status of the controller action that was run.
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
		/// Provides a dictionary that contains the results of processing each module configured for the current controller.
		/// </summary>
		public Dictionary<string, List<ModuleResult>> ModuleResults
		{
			get;
			private set;
		}

		public bool IsMissingData<T>()
		{
			ModuleConfiguration config = SageModuleFactory.Modules.Values.Where(c => c.Type == typeof(T)).FirstOrDefault();
			if (config != null)
			{
				return 
					this.ModuleResults.ContainsKey(config.Name) &&
					this.ModuleResults[config.Name].Where(r => r.Status == ModuleResultStatus.NoData)
						.Count() == 0;
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
