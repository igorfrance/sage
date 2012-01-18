namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;

	using Sage.ResourceManagement;

	using log4net;

	internal class ExtensionManager : List<ExtensionInfo>
	{
		private static ILog log = LogManager.GetLogger(typeof(ExtensionManager).FullName);

		public List<CacheableXmlDocument> GetDictionaries(SageContext context, string locale)
		{
			List<CacheableXmlDocument> result = new List<CacheableXmlDocument>();
			foreach (ExtensionInfo extension in this)
			{
				CacheableXmlDocument dictionaryXml = extension.GetDictionary(context, locale);
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
				ExtensionInfo extension = new ExtensionInfo(archive);
				this.Add(extension);

				if (!extension.IsInstalled)
					extension.Install(context);

				else if (extension.IsUpdateAvailable)
					extension.Update(context);
			}
		}
	}
}
