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
namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Xml;

	using log4net;

	/// <summary>
	/// Provides the default factory for Sage modules.
	/// </summary>
	public class SageModuleFactory : IModuleFactory
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SageModuleFactory).FullName);
		private static IDictionary<string, ModuleConfiguration> moduleDictionary;

		/// <summary>
		/// Gets the dictionary of modules that can be used in the current project.
		/// </summary>
		/// <remarks>
		/// The keys in this dictionary are the module tag names.
		/// </remarks>
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
							var temp = new Dictionary<string, ModuleConfiguration>();
							foreach (ModuleConfiguration config in Project.Configuration.Modules.Values)
							{
								if (config.Type == null)
									continue;

								foreach (string tagName in config.TagNames)
								{
									if (temp.ContainsKey(tagName))
										temp[tagName] = config;
									else
										temp.Add(tagName, config);
								}
							}

							moduleDictionary = temp;
						}
					}

					return moduleDictionary;
				}
			}
		}

		/// <inheritdoc/>
		public IModule CreateModule(XmlElement moduleNode)
		{
			if (moduleNode == null)
				throw new ArgumentNullException("moduleNode");

			string tagName = moduleNode.LocalName;
			if (!Modules.ContainsKey(tagName))
				return null;

			Type moduleType = Modules[tagName].Type;

			ConstructorInfo[] constructors = moduleType.GetConstructors();
			IModule result = (IModule) constructors[0].Invoke(new object[] { });
			return result;
		}

		internal static ModuleConfiguration GetModuleConfiguration(string tagName)
		{
			if (string.IsNullOrEmpty(tagName))
				throw new ArgumentNullException("tagName");

			if (!Modules.ContainsKey(tagName))
				throw new ArgumentException(string.Format("The tag name '{0}' is not valid", tagName));

			return Modules[tagName];
		}

		////private static Dictionary<string, ModuleConfiguration> DiscoverAvailableModules()
		////{
		////	var types = new Dictionary<string, ModuleConfiguration>();
		////	foreach (var asm in Project.RelevantAssemblies)
		////	{
		////		var modules = asm.GetTypes().Where(t => 
		////			typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && t != typeof(NullModule));

		////		foreach (var type in modules)
		////		{
		////			var config = new ModuleConfiguration(type);
		////			foreach (string tagName in config.TagNames)
		////			{
		////				types.Add(tagName, config);
		////			}
		////		}
		////	}

		////	return types;
		////}
	}
}
