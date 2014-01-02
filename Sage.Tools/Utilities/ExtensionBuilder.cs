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
namespace Sage.Tools.Utilities
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

	using NAnt.Core;

	using Sage.Configuration;
	using Sage.Extensibility;

	//// TODO: Compile any xincluded project.config resources into a single file prior to packaging it
	internal class ExtensionBuilder : IUtility
	{
		private const string BuildFileName = "Extension.build";
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionBuilder).FullName);

		private readonly ZipEntryFactory zipEntryFactory = new ZipEntryFactory();
		private NameValueCollection arguments;

		public string CommandName
		{
			get
			{
				return "build-extension";
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
			result.AppendFormat("Usage: {0} {1} -source:<path> [-name:<name>]\n\n", Program.Name, this.CommandName);
			result.AppendLine("  -source:<path>  The path to the directory that contains the extension to pack.");
			result.AppendLine("    -name:<name>  The file name of the extension package that will be created."); 
			result.AppendLine("                  If omitted, an attempt will be made to get that name from the"); 
			result.AppendLine("                  Project.config in the source directory, and if it doesn't exist"); 
			result.AppendLine("                  the name will default to the name of source directory."); 

			return result.ToString();
		}

		public void Run()
		{
			var sourcePath = this.arguments["source"];
			var targetPath = this.arguments["target"];

			if (sourcePath != null)
				sourcePath = sourcePath.Trim();

			if (targetPath != null)
				targetPath = sourcePath.Trim();

			this.BuildExtension(sourcePath, targetPath);
		}

		internal void BuildExtension(string sourcePath, string targetPath = null)
		{
			if (!Path.IsPathRooted(sourcePath))
				sourcePath = Path.Combine(Directory.GetCurrentDirectory(), sourcePath);

			string extensionName = Path.GetFileName(sourcePath);
			string configSourcePath = Path.Combine(sourcePath, ProjectConfiguration.ProjectConfigName);
			string configTargetPath = Path.Combine(Program.ApplicationPath, ProjectConfiguration.ExtensionConfigName);
			string extensionBuildScript = Path.Combine(sourcePath, BuildFileName);
			string extensionPath = targetPath;

			if (string.IsNullOrWhiteSpace(extensionPath))
				extensionPath = Path.Combine(Path.GetDirectoryName(sourcePath), Path.ChangeExtension(extensionName, "zip"));

			if (!Path.IsPathRooted(extensionPath))
				extensionPath = Path.Combine(Directory.GetCurrentDirectory(), extensionPath);

			if (!Directory.Exists(sourcePath))
			{
				throw new FileNotFoundException(
					string.Format("The specified extension source path '{0}' doesn't exist", sourcePath));
			}

			if (!File.Exists(configSourcePath))
			{
				throw new FileNotFoundException(
					string.Format("The specified project configuration path '{0}' doesn't exist", configSourcePath));
			}

			if (File.Exists(extensionBuildScript))
			{
				try
				{
					log.DebugFormat("Running nant on build file '{0}'", extensionBuildScript);
					ConsoleDriver.Main(new[] { "-buildfile:" + extensionBuildScript });

					// nantProject = new Project(extensionBuildScript, Level.Error, 1);
					// nantProject.Run();
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Running nant failed: {0}", ex.Message);
					throw;
				}
			}

			sourcePath = new DirectoryInfo(sourcePath).FullName;

			XmlDocument configurationDocument = CreateExtensionConfigurationDocument(extensionName);
			XmlElement extensionRoot = configurationDocument.DocumentElement;

			ProjectConfiguration config = ProjectConfiguration.Create(configSourcePath);
			if (!config.ValidationResult.Success)
				throw config.ValidationResult.Exception;

			XmlElement configRoot = config.ToXml(configurationDocument);
			if (targetPath == null && !string.IsNullOrWhiteSpace(config.Name))
			{
				extensionName = config.Name;
				extensionPath = Path.Combine(Path.GetDirectoryName(sourcePath), config.Name + ".zip");
				extensionPath = new FileInfo(extensionPath).FullName;
			}

			log.InfoFormat("Building extension '{0}'.", extensionName);
			log.DebugFormat("     (source: {0})", sourcePath);
			log.DebugFormat("     (target: {0})", extensionPath);

			if (File.Exists(extensionPath))
				File.Delete(extensionPath);

			string targetDir = Path.GetDirectoryName(extensionPath);

			Directory.CreateDirectory(targetDir);
			SageContext context = Program.CreateSageContext("/", path =>
			{
				if (path == "/")
					path = "~/";

				string result = path
					.Replace("~", sourcePath)
					.Replace("//", "/")
					.Replace("/", "\\");

				return new FileInfo(result).FullName;
			},
				config);

			using (ZipOutputStream zipfile = new ZipOutputStream(File.Create(extensionPath)))
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
					configurationDocument.CreateElement("p:linking", XmlNamespaces.ProjectConfigurationNamespace);

				// libraries, modules, links, metaviews, routes
				CopyConfiguration(config.Package.Links, "p:link", "p:link", linkingElement, extensionRoot);
				CopyConfiguration(config.Package.Formats, "p:formats", "p:format", linkingElement, extensionRoot);
				if (linkingElement.SelectNodes("*").Count != 0)
					extensionRoot.AppendElement(linkingElement);

				CopyConfiguration(config.Package.MetaViews, "p:metaViews", "p:metaView", configRoot, extensionRoot);
				CopyConfiguration(config.Package.Routes, "p:routing", "p:route", configRoot, extensionRoot);
				CopyConfiguration(config.Package.Libraries, "p:libraries", "p:library", configRoot, extensionRoot);
				CopyConfiguration(config.Package.Modules, "p:modules", "p:module", configRoot, extensionRoot);

				configurationDocument.Save(configTargetPath);

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
			result.LoadXml(string.Format("<extension xmlns='{0}' name='{1}'/>",
				XmlNamespaces.ProjectConfigurationNamespace, extensionName));

			return result;
		}

		private static void CopyConfiguration(PackageGroup group, string parentName, string childName, XmlElement source, XmlElement target)
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
	}
}
