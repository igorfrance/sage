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
namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;

	using log4net;

	using Sage.Configuration;
	using Sage.ResourceManagement;

	internal class ExtensionManager : List<ExtensionInfo>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionManager).FullName);
		private readonly List<string> installOrder = new List<string>();

		public List<CacheableXmlDocument> GetDictionaries(SageContext context, string locale)
		{
			List<CacheableXmlDocument> result = new List<CacheableXmlDocument>();
			foreach (ExtensionInfo extension in this)
			{
				CacheableXmlDocument dictionaryXml = extension.GetDictionary(locale);
				if (dictionaryXml != null)
					result.Add(dictionaryXml);
			}

			return result;
		}

		public void Initialize(SageContext context)
		{
			Contract.Requires<ArgumentNullException>(context != null);

			//// Discover available extensions
			string pluginPath = context.Path.Resolve(context.Path.ExtensionPath);
			if (!Directory.Exists(pluginPath))
				return;

			string[] archives = Directory.GetFiles(pluginPath, "*.zip", SearchOption.TopDirectoryOnly);
			List<ExtensionInfo> extensions = new List<ExtensionInfo>();
			foreach (string archive in archives)
			{
				try
				{
					extensions.Add(new ExtensionInfo(archive, context));
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Error initializing extension from archive {0}: {1}", archive, ex.Message);
				}
			}

			//// Add the extensions, ordered by dependency
			this.AddRange(ExtensionManager.OrderByDependency(extensions));

			//// Attempt to install all extensions			
			string action = null;
			List<string> installed = new List<string>();
			foreach (ExtensionInfo extension in this)
			{
				try
				{
					ValidationResult result = extension.Config.ValidationResult;

					if (extension.Config.ValidationResult.Success)
					{
						var missingDependencies = extension.Config.Dependencies
							.Where(name => this.Count(ex => ex.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) == 0)
							.ToList();

						if (missingDependencies.Count != 0)
						{
							string errorMessage = string.Format("Extension {0} is missing the following dependencies: {1} - installation cancelled.", 
								extension.Name, string.Join(", ", missingDependencies));

							log.ErrorFormat(errorMessage);
							throw new ProjectInitializationException(errorMessage)
							{
								Reason = ProblemType.MissingExtensionDependency, 
								SourceFile = extension.ArchiveFileName,
								Dependencies = missingDependencies,
							};
						}

						if (!extension.IsInstalled)
						{
							action = "installing";
							extension.Install();
						}
						else if (extension.IsUpdateAvailable)
						{
							action = "updating";
							extension.Update();
						}
						else if (extension.IsMissingResources)
						{
							action = "refreshing";
							extension.Refresh();
						}

						installed.Add(extension.Name);
					}
					else
					{
						string errorMessage = string.Format("Configuration error for extension {0}: {1}", extension.Name, result.Exception.Message);
						log.Error(errorMessage);

						throw new ProjectInitializationException(errorMessage, result.Exception) 
						{ 
							SourceFile = extension.ConfigurationFileName,
							Reason = ProblemType.ExtensionSchemaValidationError 
						};
					}
				}
				catch (Exception ex)
				{
					string errorMessage = string.Format("Error {0} extension '{1}': {2}", action,
						Path.GetFileNameWithoutExtension(extension.ArchiveFileName), ex.Message);

					log.ErrorFormat(errorMessage);
					throw new ProjectInitializationException(errorMessage, ex)
					{ 
						Reason = ProblemType.ExtensionInstallError, 
						SourceFile = extension.ArchiveFileName
					};
				}
			}
		}

		private static IEnumerable<ExtensionInfo> OrderByDependency(IEnumerable<ExtensionInfo> items)
		{
			List<ExtensionInfo> ordered = new List<ExtensionInfo>(items);

			int index = 0;
			while (index != ordered.Count - 1)
			{
				var curr = ordered[index];
				var swapIndex = -1;

				if (curr.Config.Dependencies.Count == 0)
				{
					index += 1;
					continue;
				}

				for (int i = index + 1; i < ordered.Count; i++)
				{
					if (curr.Config.Dependencies.Contains(ordered[i].Name))
					{
						swapIndex = i;
						break;
					}
				}

				if (swapIndex != -1)
				{
					var swap = ordered[swapIndex];
					ordered.RemoveAt(swapIndex);
					ordered.Insert(index, swap);
				}
				else
					index += 1;
			}

			return ordered;
		}
	}
}
