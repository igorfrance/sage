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
namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using Sage.Configuration;

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
							var temp = DiscoverAvailableModules();
							foreach (ModuleConfiguration config in ProjectConfiguration.Current.Modules.Values)
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
					foreach (string tagName in config.TagNames)
					{
						types.Add(tagName, config);
					}
				}
			}

			return types;
		}
	}
}
