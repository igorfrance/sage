namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using ICSharpCode.SharpZipLib.Core;
	using ICSharpCode.SharpZipLib.Zip;
	using Kelp.Core.Extensions;
	using log4net;

	using Sage.Configuration;
	using Sage.ResourceManagement;

	internal class ExtensionInfo
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionInfo).FullName);
		private readonly IOrderedEnumerable<InstallLog> orderedLogs;
		private bool loaded;

		internal ExtensionInfo(string pluginArchive)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(pluginArchive));
			Contract.Requires<ArgumentException>(File.Exists(pluginArchive));

			this.Name = Path.GetFileNameWithoutExtension(pluginArchive);
			this.Assets = new List<string>();
			this.Assemblies = new List<string>();
			this.SourceArchive = pluginArchive;
			this.SourceDirectory = Path.ChangeExtension(pluginArchive, null);
			this.InstallLogFile = Path.ChangeExtension(this.SourceArchive, ".history.xml");

			if (!Directory.Exists(this.SourceDirectory))
				this.ExtractArchive();

			this.ReadLog();

			string bindir = Path.Combine(this.SourceDirectory, "bin");
			if (Directory.Exists(bindir))
			{
				this.Assemblies.AddRange(
					Directory.GetFiles(bindir, "*.dll", SearchOption.AllDirectories));
			}

			string assetdir = Path.Combine(this.SourceDirectory, "assets");
			if (Directory.Exists(assetdir))
			{
				this.Assets.AddRange(
					Directory.GetFiles(assetdir, "*.*", SearchOption.AllDirectories));
			}

			string configPath = Path.Combine(this.SourceDirectory, "Plugin.config");
			if (File.Exists(configPath))
				this.Config = ProjectConfiguration.Create(configPath);

			orderedLogs = from i in InstallHistory
						  orderby i.Date
						  select i;
		}

		public string Name { get; private set; }

		public ProjectConfiguration Config { get; private set; }

		public List<string> Assemblies { get; private set; }

		public List<string> Assets { get; private set; }

		public List<InstallLog> InstallHistory { get; private set; }

		public string InstallLogFile { get; private set; }

		public string SourceArchive { get; private set; }

		public string SourceDirectory { get; private set; }

		public bool IsInstalled
		{
			get
			{
				if (this.InstallHistory.Count == 0)
					return false;

				return orderedLogs.Last().Result == InstallState.Installed;
			}
		}

		public void Install(SageContext context)
		{
			if (this.IsInstalled)
				this.Uninstall();

			InstallLog installLog = new InstallLog(DateTime.Now);

			log.DebugFormat("Installation of extension '{0}' started", this.Name);

			try
			{
				foreach (string file in this.Assets)
				{
					string childPath = file.ToLower().Replace(this.SourceDirectory.ToLower(), string.Empty);
					string targetPath = context.Path.Resolve(childPath);
					string targetDir = Path.GetDirectoryName(targetPath);

					Directory.CreateDirectory(targetDir);

					InstallFile entry = installLog.AddFile(targetPath);
					if (File.Exists(targetPath))
					{
						entry.State = FileState.NotCopied;
						continue;
					}

					File.Copy(file, targetPath);
					entry.State = FileState.Copied;
				}

				installLog.Result = InstallState.Installed;
			}
			catch (Exception ex)
			{
				log.Error(ex);

				installLog.Error = ex;
				installLog.Result = InstallState.NotInstalled;

				Rollback(installLog);
			}
			finally
			{
				SaveLog(installLog);
			}

			log.DebugFormat("Installation of extension '{0}' {1}.", this.Name, 
				installLog.Result == InstallState.Installed ? "succeeded" : "failed");
		}

		public void Uninstall()
		{
			if (!this.IsInstalled)
				return;

			log.DebugFormat("Uninstalling extension '{0}'", this.Name);
			Rollback(orderedLogs.Last());
		}

		public void LoadAssemblies()
		{
			if (loaded)
				return;

			foreach (string assemblyPath in this.Assemblies)
			{
				Assembly.LoadFrom(assemblyPath);
			}

			loaded = true;
		}

		public CacheableXmlDocument GetDictionary(SageContext context, string locale)
		{
			SageContext pluginContext = GetPluginContext(context);
			string path = pluginContext.Path.GetDictionaryPath(locale);
			if (File.Exists(path))
				return pluginContext.Resources.LoadXml(path);

			return null;
		}

		private SageContext GetPluginContext(SageContext context)
		{
			return new SageContext(context, this.Config);
		}

		private void Rollback(InstallLog installLog)
		{
			log.DebugFormat("Rolling back log extension '{0}'", this.Name);
			foreach (InstallFile file in installLog.Files.Where(f => f.State == FileState.Copied))
			{
				try
				{
					File.Delete(file.Path);
				}
				catch (Exception ex)
				{
					log.Error(ex);
				}
			}
		}

		private void SaveLog(InstallLog installLog)
		{
			XmlDocument logDoc = new XmlDocument();
			if (File.Exists(this.InstallLogFile))
				logDoc.Load(this.InstallLogFile);
			else
				logDoc.LoadXml(string.Format("<plugin name='{0}'></plugin>", this.Name));

			logDoc.DocumentElement.AppendChild(installLog.ToXml(logDoc));
			logDoc.Save(this.InstallLogFile);
		}

		private void ExtractArchive()
		{
			ZipFile zipfile = null;
			try
			{
				FileStream fs = File.OpenRead(this.SourceArchive);
				zipfile = new ZipFile(fs);

				foreach (ZipEntry zipEntry in zipfile)
				{
					if (!zipEntry.IsFile)
						continue;

					string entryFileName = zipEntry.Name;

					byte[] buffer = new byte[4096]; //// 4K is optimum
					Stream zipStream = zipfile.GetInputStream(zipEntry);

					// Manipulate the output filename here as desired.
					string fullZipToPath = Path.Combine(this.SourceDirectory, entryFileName);
					string directoryName = Path.GetDirectoryName(fullZipToPath);
					if (directoryName.Length > 0)
						Directory.CreateDirectory(directoryName);

					//// Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
					//// of the file, but does not waste memory.
					//// The "using" will close the stream even if an exception occurs.
					using (FileStream streamWriter = File.Create(fullZipToPath))
					{
						StreamUtils.Copy(zipStream, streamWriter, buffer);
					}
				}
			}
			finally
			{
				if (zipfile != null)
				{
					zipfile.IsStreamOwner = true;
					zipfile.Close();
				}
			}
		}

		private void ReadLog()
		{
			this.InstallHistory = new List<InstallLog>();

			if (!File.Exists(this.InstallLogFile))
				return;

			XmlDocument logDoc = new XmlDocument();
			logDoc.Load(this.InstallLogFile);

			foreach (XmlElement element in logDoc.SelectNodes("/plugin/install"))
				this.InstallHistory.Add(new InstallLog(element));
		}
	}
}
