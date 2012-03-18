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
	using System.Reflection;
	using System.Xml;

	using ICSharpCode.SharpZipLib.Core;
	using ICSharpCode.SharpZipLib.Zip;
	using Kelp.Extensions;
	using Kelp.IO;
	using log4net;

	using Sage.Configuration;
	using Sage.ResourceManagement;

	internal class ExtensionInfo
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionInfo).FullName);

		private readonly IOrderedEnumerable<InstallLog> orderedLogs;
		private readonly SageContext context;
		private List<Assembly> assemblies;
		private bool loaded;

		internal ExtensionInfo(string pluginArchive, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(pluginArchive));
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentException>(File.Exists(pluginArchive));

			this.context = context;

			this.Name = Path.GetFileNameWithoutExtension(pluginArchive);
			this.ArchiveDate = File.GetLastWriteTime(pluginArchive).Max(File.GetCreationTime(pluginArchive));
			this.SourceAssets = new List<string>();
			this.AssembyNames = new List<string>();
			this.SourceArchive = pluginArchive;
			this.SourceDirectory = Path.ChangeExtension(pluginArchive, null);
			this.InstallLogFile = Path.ChangeExtension(this.SourceArchive, ".history.xml");

			if (!Directory.Exists(this.SourceDirectory))
			{
				this.ExtractArchive();
			}
			else if (this.ArchiveDate > Directory.GetLastWriteTime(this.SourceDirectory))
			{
				try
				{
					Directory.Delete(this.SourceDirectory);
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Could not delete the source directory '{0}' for extension '{1}': {2}.",
						this.SourceDirectory, this.Name, ex.Message);
				}

				this.ExtractArchive();
			}

			this.ReadLog();

			string bindir = Path.Combine(this.SourceDirectory, "bin");
			if (Directory.Exists(bindir))
			{
				this.AssembyNames.AddRange(
					Directory.GetFiles(bindir, "*.dll", SearchOption.AllDirectories));
			}

			string assetdir = Path.Combine(this.SourceDirectory, "assets");
			if (Directory.Exists(assetdir))
			{
				this.SourceAssets.AddRange(
					Directory.GetFiles(assetdir, "*.*", SearchOption.AllDirectories));
			}

			string configPath = Path.Combine(this.SourceDirectory, "Extension.config");
			if (File.Exists(configPath))
				this.Config = ProjectConfiguration.Create(configPath);

			this.TargetAssets = this.SourceAssets.Select(this.GetTargetPath).ToList();

			orderedLogs = from i in InstallHistory
						  orderby i.Date
						  select i;
		}

		public string Name { get; private set; }

		public DateTime ArchiveDate { get; private set; }

		public DateTime? LastInstallDate
		{
			get
			{
				return this.orderedLogs.Last(l => l.Result == InstallState.Installed).Date;
			}
		}

		public ProjectConfiguration Config { get; private set; }

		public List<Assembly> Assemblies
		{
			get
			{
				if (assemblies == null)
					LoadAssemblies();

				return assemblies;
			}
		}

		public List<string> AssembyNames { get; private set; }

		public List<string> SourceAssets { get; private set; }

		public List<string> TargetAssets { get; private set; }

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

		public bool IsUpdateAvailable
		{
			get
			{
				DateTime? lastInstallDate = orderedLogs.Last().Date;
				return this.ArchiveDate > lastInstallDate;
			}
		}

		public bool IsMissingResources
		{
			get
			{
				return this.TargetAssets.Any(targetAsset => !File.Exists(targetAsset));
			}
		}

		public void Update(bool forceUpdate = false)
		{
			Uninstall(true, forceUpdate);
			Install();
		}

		public void Install()
		{
			if (this.IsInstalled)
				this.Uninstall();

			InstallLog installLog = new InstallLog(DateTime.Now);

			log.DebugFormat("Installation of extension '{0}' started", this.Name);

			try
			{
				foreach (string file in this.SourceAssets)
				{
					string childPath = file.ToLower().Replace(this.SourceDirectory.ToLower(), string.Empty);
					string targetPath = context.Path.Resolve(childPath);
					string targetDir = Path.GetDirectoryName(targetPath);

					Directory.CreateDirectory(targetDir);

					InstallItem entry = installLog.AddFile(targetPath);
					entry.CrcCode = Crc32.GetHash(file);

					if (File.Exists(targetPath))
					{
						if (!this.HasInstalled(targetPath))
						{
							log.WarnFormat("Extension {1}: skipped installing '{0}' because a file with the same name already exists, and it doesn't originate from this extension.",
								targetPath, this.Name);

							entry.State = InstallState.NotInstalled;
							continue;
						}

						if (Crc32.GetHash(targetPath) != entry.CrcCode)
						{
							log.WarnFormat("Extension {1}: not overwriting previously installed file '{0}' because it has been changed.",
								targetPath, this.Name);

							entry.State = InstallState.NotInstalled;
							continue;
						}
					}

					File.Copy(file, targetPath, true);
					entry.State = InstallState.Installed;
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

		public void Refresh()
		{
			foreach (string sourcePath in this.SourceAssets)
			{
				string targetPath = GetTargetPath(sourcePath);
				if (!File.Exists(targetPath))
				{
					string directoryPath = Path.GetDirectoryName(targetPath);
					if (!Directory.Exists(directoryPath))
						Directory.CreateDirectory(directoryPath);

					File.Copy(sourcePath, targetPath, true);
				}
			}
		}

		public void Uninstall(bool isUpdateUninstall = false, bool deleteChangedFiles = false)
		{
			if (!this.IsInstalled)
				return;

			log.DebugFormat("Uninstalling extension '{0}'", this.Name);

			var installLog = orderedLogs.Last();
			Rollback(installLog, isUpdateUninstall, deleteChangedFiles);
			SaveLog(installLog);
		}

		public void LoadAssemblies()
		{
			if (loaded)
				return;

			assemblies = new List<Assembly>();
			foreach (string assemblyPath in this.AssembyNames)
			{
				assemblies.Add(Assembly.LoadFrom(assemblyPath));
			}

			loaded = true;
		}

		public bool HasInstalled(string itemPath)
		{
			InstallLog firstInstall = orderedLogs.FirstOrDefault(l => l.Items.FirstOrDefault(i => i.Path == itemPath) != null);
			if (firstInstall != null)
			{
				InstallItem item = firstInstall.Items.First(i => i.Path == itemPath);
				return item.State == InstallState.Installed;
			}

			return false;
		}

		public CacheableXmlDocument GetDictionary(string locale)
		{
			SageContext pluginContext = GetExtensionContext();
			string path = pluginContext.Path.GetDictionaryPath(locale);
			if (File.Exists(path))
				return pluginContext.Resources.LoadXml(path);

			return null;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0}", this.Name);
		}

		private SageContext GetExtensionContext()
		{
			return new SageContext(context, this.Config);
		}

		private void Rollback(InstallLog installLog, bool isUpdateRollback = false, bool deleteChangedFiles = false)
		{
			log.DebugFormat("Rolling back log extension '{0}'", this.Name);

			IEnumerable<InstallItem> installedItems = installLog.Items.Where(f => f.State == InstallState.Installed);
			foreach (InstallItem file in installedItems)
			{
				if (!File.Exists(file.Path))
				{
					file.State = InstallState.UnInstalled;
					continue;
				}

				string currentCrc = Crc32.GetHash(file.Path);
				string originalCrc = file.CrcCode;

				if (!deleteChangedFiles && currentCrc != originalCrc)
				{
					bool deleteFile = false;
					if (isUpdateRollback)
					{
						string sourceFile = GetSourcePath(file.Path);
						string updatedCrc = Crc32.GetHash(sourceFile);
						if (updatedCrc == currentCrc)
							deleteFile = true;
					}

					if (!deleteFile)
					{
						log.WarnFormat("The file '{0}' has changed since it was installed, it will not be deleted", file.Path);
						continue;
					}
				}

				try
				{
					File.Delete(file.Path);
					file.State = InstallState.UnInstalled;
				}
				catch (Exception ex)
				{
					log.Error(ex);
				}
			}

			installLog.Result = InstallState.UnInstalled;
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

		private string GetTargetPath(string sourcePath)
		{
			string childPath = sourcePath.ToLower().Replace(this.SourceDirectory.ToLower(), string.Empty);
			return context.Path.Resolve(childPath);
		}

		private string GetSourcePath(string targetPath)
		{
			string childPath = targetPath.ToLower().Replace(context.Path.Resolve("/").ToLower(), string.Empty);
			return Path.Combine(this.SourceDirectory, childPath);
		}
	}
}
