﻿/**
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

	using Kelp.Core.Extensions;
	using Kelp.Extensions;

	using log4net;
	using Sage.Configuration;

	internal class ExtensionBuilder : IUtility
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionBuilder).FullName);

		private readonly ZipEntryFactory zipEntryFactory = new ZipEntryFactory();
		private NameValueCollection arguments;
		private string applicationPath;
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
			result.AppendFormat("Usage: {0} {1} -source:<path>", Program.Name, this.CommandName);
			result.AppendLine("  -source:<path>	 The path to the directory that contains the extension to package.");
			result.AppendLine("    -name:<path>	 The file name of the extension package that will be created."); 
			result.AppendLine("                  If omitted, an attempt will be made to get that name from the"); 
			result.AppendLine("                  Project.config in the source directory, if it exists, and finally"); 
			result.AppendLine("                  if this fails to the name will default to the name of source directory."); 

			return result.ToString();
		}

		public void Run()
		{
			applicationPath = arguments["source"];
			if (!Path.IsPathRooted(applicationPath))
				applicationPath = Path.Combine(Directory.GetCurrentDirectory(), applicationPath);

			extensionName = Path.ChangeExtension(Path.GetFileName(applicationPath), "zip");
			string configPath = Path.Combine(applicationPath, "Project.config");
			string systemConfigPath = Path.Combine(Program.ApplicationPath, "System.config");
			string extensionConfigPath = Path.Combine(Program.ApplicationPath, "Extension.config");

			string targetPath = arguments["target"] ?? Path.Combine(Path.GetDirectoryName(applicationPath), extensionName);
			if (!Path.IsPathRooted(targetPath))
				targetPath = Path.Combine(Directory.GetCurrentDirectory(), targetPath);

			if (!Directory.Exists(applicationPath))
			{
				throw new FileNotFoundException(
					string.Format("The specified extension source path '{0}' doesn't exist", applicationPath));
			}

			if (!File.Exists(configPath))
			{
				throw new FileNotFoundException(
					string.Format("The specified project configuration path '{0}' doesn't exist", configPath));
			}

			XmlNamespaceManager nm = XmlNamespaces.Manager;
			XmlDocument extensionConfig = CreateExtensionConfigurationDocument(extensionName);
			XmlElement extensionRoot = extensionConfig.SelectSingleElement("p:configuration/p:extension", nm);

			ProjectConfiguration config = ProjectConfiguration.Create(configPath, systemConfigPath);
			XmlElement configRoot = config.ConfigurationElement;

			if (arguments["target"] == null && !string.IsNullOrWhiteSpace(config.Name))
			{
				extensionName = Path.ChangeExtension(config.Name, "zip");
				targetPath = Path.Combine(Path.GetDirectoryName(applicationPath), extensionName);
			}

			log.InfoFormat("Building extension '{0}'.", extensionName);
			log.DebugFormat("     (source: {0})", applicationPath);
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
					PackFile(zipfile, file, "assets/" + childPath);
				}

				// binaries
				string binarySourcePath = context.Path.Resolve("~/bin");
				foreach (string file in config.Package.Binaries.GetFiles(context))
				{
					string childPath = file.ReplaceAll(binarySourcePath.EscapeMeta(), string.Empty).Trim('/', '\\');
					PackFile(zipfile, file, "bin/" + childPath);
				}

				// libraries, modules, links, metaviews, routes
				CopyConfiguration(config.Package.MetaViews, "p:metaViews", "p:view", configRoot, extensionRoot);
				CopyConfiguration(config.Package.Routes, "p:routing", "p:route", configRoot, extensionRoot);
				CopyConfiguration(config.Package.Links, "p:links", "p:link", configRoot, extensionRoot);
				CopyConfiguration(config.Package.Libraries, "p:libraries", "p:library", configRoot, extensionRoot);
				CopyConfiguration(config.Package.Modules, "p:modules", "p:module", configRoot, extensionRoot);

				extensionConfig.Save(extensionConfigPath);

				PackFile(zipfile, extensionConfigPath, "Extension.config");

				zipfile.IsStreamOwner = true;
				zipfile.Finish();
				zipfile.Close();
			}

			log.InfoFormat("Extension build completed");
		}

		private static XmlDocument CreateExtensionConfigurationDocument(string extensionName)
		{
			XmlDocument result = new XmlDocument();
			result.LoadXml(string.Format("<configuration xmlns='{0}'><extension name='{1}'></extension></configuration>",
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
				"~", applicationPath).Replace(
				"//", "/").Replace(
				"/", "\\");

			return new FileInfo(result).FullName;
		}
	}
}
