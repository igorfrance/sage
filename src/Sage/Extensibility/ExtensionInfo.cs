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
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using ICSharpCode.SharpZipLib.Core;
	using ICSharpCode.SharpZipLib.Zip;
	using Kelp.Core.Extensions;
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
				return this.ArchiveDate > orderedLogs.Last().Date;
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
			Uninstall(forceUpdate);
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
					File.Copy(sourcePath, targetPath, true);
			}
		}

		public void Uninstall(bool deleteChangedFiles = false)
		{
			if (!this.IsInstalled)
				return;

			log.DebugFormat("Uninstalling extension '{0}'", this.Name);

			var installLog = orderedLogs.Last();
			Rollback(installLog, deleteChangedFiles);
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

		public override string ToString()
		{
			return string.Format("{0}", this.Name);
		}

		private SageContext GetExtensionContext()
		{
			return new SageContext(context, this.Config);
		}

		private void Rollback(InstallLog installLog, bool deleteChangedFiles = false)
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

				if (!deleteChangedFiles && Crc32.GetHash(file.Path) != file.CrcCode)
				{
					log.WarnFormat("The file '{0}' has changed since it was installed, it will not be deleted", file.Path);
					continue;
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

		private string GetTargetPath(string assetPath)
		{
			string childPath = assetPath.ToLower().Replace(this.SourceDirectory.ToLower(), string.Empty);
			return context.Path.Resolve(childPath);
		}
	}
}
