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
namespace Sage.Build.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
	using System.Xml;

	using ICSharpCode.SharpZipLib.Core;
	using ICSharpCode.SharpZipLib.Zip;

	using Kelp.Extensions;

	using log4net;
	using Sage.Configuration;
	using Sage.Extensibility;

	//// TODO: Compile any xincluded project.config resources into a single file prior to packaging it
	internal class ExtensionBuilder : IUtility
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionBuilder).FullName);

		private readonly ZipEntryFactory zipEntryFactory = new ZipEntryFactory();
		private NameValueCollection arguments;
		private string sourcePath;
		private string extensionName;

		public string CommandName
		{
			get
			{
				return "extension";
			}
		}

		public bool ParseArguments(string[] args)
		{
			arguments = new NameValueCollection();
			foreach (string arg in args)
			{
				if (arg.StartsWith("-source:"))
					arguments["source"] = arg.Substring(8).Trim('"');

				if (arg.StartsWith("-target:"))
					arguments["target"] = arg.Substring(8).Trim('"');
			}

			return arguments["source"] != null;
		}

		public string GetUsage()
		{
			StringBuilder result = new StringBuilder();
			result.AppendLine("Packs a sage project into a distributable extension package.\n");
			result.AppendFormat("Usage: {0} {1} -source:<path> [-name:<name>]", Program.Name, this.CommandName);
			result.AppendLine("  -source:<path>	 The path to the directory that contains the extension to package.");
			result.AppendLine("    -name:<name>	 The file name of the extension package that will be created."); 
			result.AppendLine("                  If omitted, an attempt will be made to get that name from the"); 
			result.AppendLine("                  Project.config in the source directory, if it exists, and finally"); 
			result.AppendLine("                  if this fails to the name will default to the name of source directory."); 

			return result.ToString();
		}

		public void Run()
		{
			this.sourcePath = this.arguments["source"];
			if (!Path.IsPathRooted(this.sourcePath))
				this.sourcePath = Path.Combine(Directory.GetCurrentDirectory(), this.sourcePath);

			extensionName = Path.GetFileNameWithoutExtension(this.sourcePath);
			string configSourcePath = Path.Combine(this.sourcePath, ProjectConfiguration.ProjectConfigName);
			string systemConfigPath = Path.Combine(Program.ApplicationPath, ProjectConfiguration.SystemConfigName);
			string configTargetPath = Path.Combine(Program.ApplicationPath, ProjectConfiguration.ExtensionConfigName);

			string targetPath = this.arguments["target"] ??
				Path.Combine(Path.GetDirectoryName(this.sourcePath), Path.ChangeExtension(this.extensionName, "zip"));

			if (!Path.IsPathRooted(targetPath))
				targetPath = Path.Combine(Directory.GetCurrentDirectory(), targetPath);

			if (!Directory.Exists(this.sourcePath))
			{
				throw new FileNotFoundException(
					string.Format("The specified extension source path '{0}' doesn't exist", this.sourcePath));
			}

			if (!File.Exists(configSourcePath))
			{
				throw new FileNotFoundException(
					string.Format("The specified project configuration path '{0}' doesn't exist", configSourcePath));
			}

			XmlDocument extensionConfig = CreateExtensionConfigurationDocument(extensionName);
			XmlElement extensionRoot = extensionConfig.DocumentElement;

			ProjectConfiguration config = ProjectConfiguration.Create(configSourcePath, systemConfigPath);
			if (!config.ValidationResult.Success)
				throw config.ValidationResult.Exception;

			XmlElement configRoot = config.ToXml(extensionConfig);
			if (arguments["target"] == null && !string.IsNullOrWhiteSpace(config.Name))
			{
				extensionName = config.Name;
				targetPath = Path.Combine(Path.GetDirectoryName(this.sourcePath), Path.ChangeExtension(config.Name, "zip"));
			}

			log.InfoFormat("Building extension '{0}'.", this.extensionName);
			log.DebugFormat("     (source: {0})", this.sourcePath);
			log.DebugFormat("     (target: {0})", targetPath);

			if (File.Exists(targetPath))
				File.Delete(targetPath);

			string targetDir = Path.GetDirectoryName(targetPath);

			Directory.CreateDirectory(targetDir);
			SageContext context = Program.CreateSageContext("/", this.MapPath, config);

			using (ZipOutputStream zipfile = new ZipOutputStream(File.Create(targetPath)))
			{
				// Disable Zip64 for Mac compatibility
				zipfile.UseZip64 = UseZip64.Off;
				zipfile.SetLevel(9);

				// assets
				string assetSourcePath = context.Path.Resolve(context.Path.AssetPath);
				foreach (string file in config.Package.Assets.GetFiles(context))
				{
					string childPath = file.ReplaceAll(assetSourcePath.EscapeMeta(), string.Empty).Trim('/', '\\');
					this.PackFile(zipfile, file, "assets/" + childPath);
				}

				// binaries
				string binarySourcePath = context.Path.Resolve("~/bin");
				foreach (string file in config.Package.Binaries.GetFiles(context))
				{
					string childPath = file.ReplaceAll(binarySourcePath.EscapeMeta(), string.Empty).Trim('/', '\\');
					this.PackFile(zipfile, file, "bin/" + childPath);
				}

				XmlElement linkingElement = 
					extensionConfig.CreateElement("p:linking", XmlNamespaces.ProjectConfigurationNamespace);

				// libraries, modules, links, metaviews, routes
				this.CopyConfiguration(config.Package.Links, "p:link", "p:link", linkingElement, extensionRoot);
				this.CopyConfiguration(config.Package.Formats, "p:formats", "p:format", linkingElement, extensionRoot);
				if (linkingElement.SelectNodes("*").Count != 0)
					extensionRoot.AppendElement(linkingElement);

				this.CopyConfiguration(config.Package.MetaViews, "p:metaViews", "p:metaView", configRoot, extensionRoot);
				this.CopyConfiguration(config.Package.Routes, "p:routing", "p:route", configRoot, extensionRoot);
				this.CopyConfiguration(config.Package.Libraries, "p:libraries", "p:library", configRoot, extensionRoot);
				this.CopyConfiguration(config.Package.Modules, "p:modules", "p:module", configRoot, extensionRoot);

				extensionConfig.Save(configTargetPath);

				this.PackFile(zipfile, configTargetPath, ProjectConfiguration.ExtensionConfigName);

				zipfile.IsStreamOwner = true;
				zipfile.Finish();
				zipfile.Close();
			}

			log.InfoFormat("Extension build completed");
		}

		private static XmlDocument CreateExtensionConfigurationDocument(string extensionName)
		{
			XmlDocument result = new XmlDocument();
			result.LoadXml(string.Format("<project xmlns='{0}' name='{1}'/>",
				XmlNamespaces.ProjectConfigurationNamespace, extensionName));

			return result;
		}

		private void CopyConfiguration(PackageGroup group, string parentName, string childName, XmlElement source, XmlElement target)
		{
			string aggregateXpath = string.Format("{0}/{1}/@name", parentName, childName);
			List<string> names = group.FilterNames(source.Aggregate(aggregateXpath, XmlNamespaces.Manager));
			if (names.Count != 0)
			{
				XmlNode parentNode = target.AppendElement(parentName.Replace("p:", string.Empty), XmlNamespaces.ProjectConfigurationNamespace);
				foreach (string name in names)
				{
					string xpath = string.Format("{0}/{1}[@name='{2}']", parentName, childName, name);
					parentNode.AppendChild(target.OwnerDocument.ImportNode(
						source.SelectSingleNode(xpath, XmlNamespaces.Manager), true));
				}
			}
		}

		private void PackFile(ZipOutputStream zipfile, string filePath, string targetName)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			ZipEntry zipentry = zipEntryFactory.MakeFileEntry(targetName, true);
			zipentry.DateTime = fileInfo.LastWriteTime;
			zipentry.Size = fileInfo.Length;

			zipfile.PutNextEntry(zipentry);

			using (FileStream streamReader = File.OpenRead(filePath))
				StreamUtils.Copy(streamReader, zipfile, new byte[4096]);

			zipfile.CloseEntry();
		}

		private string MapPath(string path)
		{
			if (path == "/")
				path = "~/";

			string result = path.Replace(
				"~", this.sourcePath).Replace(
				"//", "/").Replace(
				"/", "\\");

			return new FileInfo(result).FullName;
		}
	}
}
