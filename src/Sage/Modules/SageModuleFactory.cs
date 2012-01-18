namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using Sage.Configuration;

	public class SageModuleFactory : IModuleFactory
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SageModuleFactory).FullName);
		private static IDictionary<string, ModuleConfiguration> moduleDictionary;
		private static IDictionary<string, string> moduleTagNames;

		public static IDictionary<string, ModuleConfiguration> Modules
		{
			get
			{
				lock (log)
				{
					if (moduleDictionary == null)
					{
						lock (log)
						{
							var temp = DiscoverAvailableModules();
							foreach (ModuleConfiguration config in ProjectConfiguration.Current.Modules.Values)
							{
								if (config.Type == null)
									continue;

								string typeName = config.Type.FullName;
								if (temp.ContainsKey(typeName))
									temp[typeName] = config;
								else
									temp.Add(typeName, config);
							}

							moduleDictionary = temp;
						}
					}

					return moduleDictionary;
				}
			}
		}

		internal static IDictionary<string, string> ModuleTagNames
		{
			get
			{
				lock (log)
				{
					if (moduleTagNames == null)
					{
						var temp = new Dictionary<string, string>();
						lock (log)
						{
							foreach (string typeName in Modules.Keys)
							{
								ModuleConfiguration config = Modules[typeName];
								foreach (string tagName in config.TagNames)
								{
									if (temp.ContainsKey(tagName))
										temp[tagName] = typeName;
									else
										temp.Add(tagName, typeName);
								}
							}

							moduleTagNames = temp;
						}
					}

					return moduleTagNames;
				}
			}
		}

		public IModule CreateModule(XmlElement moduleNode)
		{
			if (moduleNode == null)
				throw new ArgumentNullException("moduleNode");

			Type moduleType = GetModuleType(moduleNode.LocalName);

			ConstructorInfo[] constructors = moduleType.GetConstructors();
			IModule result = (IModule) constructors[0].Invoke(new object[] { });
			return result;
		}

		internal static ModuleConfiguration GetModuleConfiguration(string tagName)
		{
			if (string.IsNullOrEmpty(tagName))
				throw new ArgumentNullException("tagName");

			if (!ModuleTagNames.ContainsKey(tagName))
				throw new ArgumentException(string.Format("The tag name '{0}' is not valid", tagName));

			string configName = ModuleTagNames[tagName];
			return Modules[configName];
		}

		private static Type GetModuleType(string tagName)
		{
			if (string.IsNullOrEmpty(tagName))
				throw new ArgumentNullException("tagName");

			ModuleConfiguration config = GetModuleConfiguration(tagName);
			return config.Type;
		}

		private static Dictionary<string, ModuleConfiguration> DiscoverAvailableModules()
		{
			var types = new Dictionary<string, ModuleConfiguration>();
			foreach (var asm in Application.RelevantAssemblies)
			{
				var modules = asm.GetTypes().Where(t => 
					typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && t != typeof(NullModule));

				foreach (var type in modules)
				{
					var config = new ModuleConfiguration(type);
					types.Add(config.Type.FullName, config);
				}
			}

			return types;
		}
	}
}
