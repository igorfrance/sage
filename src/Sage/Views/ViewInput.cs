namespace Sage.Views
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Xml;

	using log4net;
	using Sage.Configuration;
	using Sage.Modules;

	/// <summary>
	/// Implements a class that handles per-request input XML handling.
	/// </summary>
	public class ViewInput
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ViewInput).FullName);

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewInput"/> class.
		/// </summary>
		/// <param name="viewName">The name of this view.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="viewName"/> is <c>null</c> or empty.</exception>
		public ViewInput(string viewName)
		{
			if (string.IsNullOrEmpty(viewName))
				throw new ArgumentNullException("viewName");

			this.ViewName = viewName;
			this.ModuleResults = new Dictionary<string, List<ModuleResult>>();
			this.Resources = new List<ModuleResource>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewInput"/> class.
		/// </summary>
		/// <param name="viewName">The name of this view.</param>
		/// <param name="viewConfig">The configuration node of the view template this class handles.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="viewConfig"/> is <c>null</c>.</exception>
		public ViewInput(string viewName, XmlElement viewConfig)
			: this(viewName)
		{
			if (viewConfig == null)
				throw new ArgumentNullException("viewConfig");

			this.ConfigNode = (XmlElement) viewConfig.CloneNode(true);
		}

		public List<ModuleResource> Resources { get; private set; }

		public string ViewName { get; private set; }

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

			ModuleConfiguration config = SageModuleFactory.GetModuleConfiguration(moduleTagName);

			this.Resources.AddRange(config.Resources.Where(r => 
				this.Resources.Where(r1 => r1.Path == r.Path).Count() == 0));
		}
	}
}
