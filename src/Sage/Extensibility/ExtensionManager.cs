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

	using log4net;
	using Sage.ResourceManagement;

	internal class ExtensionManager : List<ExtensionInfo>
	{
		private static ILog log = LogManager.GetLogger(typeof(ExtensionManager).FullName);

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

			string pluginPath = context.Path.Resolve(context.Path.ExtensionPath);
			if (!Directory.Exists(pluginPath))
				return;

			string[] archives = Directory.GetFiles(pluginPath, "*.zip", SearchOption.TopDirectoryOnly);
			ExtensionInfo extension = null;
			string action = null;

			try
			{
				foreach (string archive in archives)
				{
					extension = new ExtensionInfo(archive, context);
					this.Add(extension);

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
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Error {0} extension '{1}': {2}", action, extension.Name, ex.Message);
				throw;
			}
		}
	}
}
