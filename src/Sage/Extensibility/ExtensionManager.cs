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
			foreach (string archive in archives)
			{
				var extension = new ExtensionInfo(archive, context);
				this.Add(extension);

				if (!extension.IsInstalled)
				{
					try
					{
						extension.Install();
					}
					catch (Exception ex)
					{
						log.ErrorFormat("Error installing extension '{0}': {1}", extension.Name, ex.Message);
						throw;
					}
				}
				else if (extension.IsUpdateAvailable)
				{
					try
					{
						extension.Update();
					}
					catch (Exception ex)
					{
						log.ErrorFormat("Error updating extension '{0}': {1}", extension.Name, ex.Message);
						throw;
					}
				}
				else if (extension.IsMissingResources)
				{
					try
					{
						extension.Refresh();
					}
					catch (Exception ex)
					{
						log.ErrorFormat("Error refreshing extension '{0}': {1}", extension.Name, ex.Message);
						throw;
					}
				}
			}
		}
	}
}
