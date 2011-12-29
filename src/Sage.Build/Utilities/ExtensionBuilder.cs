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

	using Sage.Configuration;
	using Sage.ResourceManagement;

	internal class ExtensionBuilder : IUtility
	{
		private NameValueCollection arguments;

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
			//// try get directory with files in there
			//// read project.config
			//// name = project.name ?? dirname
			//// output path = 
			//// create an archive in the output path
				//// add project.config as extension.config
				//// add assets to assets
				//// add all not *.pdb and not project.config or system.config files from bin folder

			string sourcePath = arguments["source"];
			if (!Path.IsPathRooted(sourcePath))
				sourcePath = Path.Combine(Directory.GetCurrentDirectory(), sourcePath);

			string targetName = Path.ChangeExtension(Path.GetFileName(sourcePath), "zip");
			string configPath = Path.Combine(sourcePath, "Project.config");

			string targetPath = arguments["target"] ?? Path.Combine(Path.GetDirectoryName(sourcePath), targetName);
			if (!Path.IsPathRooted(targetPath))
				targetPath = Path.Combine(Directory.GetCurrentDirectory(), targetPath);

			List<string> binaries = new List<string>();

			if (!Directory.Exists(sourcePath))
			{
				throw new FileNotFoundException(
					string.Format("The specified extension source path '{0}' doesn't exist", sourcePath));
			}

			if (File.Exists(configPath))
			{
				ProjectConfiguration config = ProjectConfiguration.Create(configPath);
				if (arguments["target"] == null && !string.IsNullOrWhiteSpace(config.Name))
				{
					targetName = Path.ChangeExtension(config.Name, "zip");
					targetPath = Path.Combine(Path.GetDirectoryName(sourcePath), targetName);
				}

				binaries.AddRange(config.Deliverables);
			}

			if (File.Exists(targetPath))
				File.Delete(targetPath);

			string assetPath = Path.Combine(sourcePath, "Assets");
			string targetDir = Path.GetDirectoryName(targetPath);

			Directory.CreateDirectory(targetDir);

			using (ZipOutputStream zipfile = new ZipOutputStream(File.Create(targetPath)))
			{
				// Disable Zip64 for Mac compatibility
				zipfile.UseZip64 = UseZip64.Off;
				zipfile.SetLevel(9);

				if (Directory.Exists(assetPath))
					PackDirectory(zipfile, assetPath, "Assets");

				foreach (string binary in binaries)
				{
					string binaryPath = Path.Combine(sourcePath, binary);
					if (!File.Exists(binaryPath))
						throw new FileNotFoundException(string.Format("The specified binary deliverable '{0}' was not found.", binaryPath), binaryPath);

					string fileName = Path.GetFileName(binary);
					PackFile(zipfile, binaryPath, Path.Combine("Bin", fileName));
				}

				if (File.Exists(configPath))
					PackFile(zipfile, configPath, "Plugin.config");

				zipfile.IsStreamOwner = true;
				zipfile.Finish();
				zipfile.Close();
			}
		}

		private void PackDirectory(ZipOutputStream zipfile, string directoryPath, string targetName)
		{
			directoryPath = directoryPath.ToLower();
			foreach (string filepath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
			{
				string fileName = filepath.ToLower().Replace(directoryPath, string.Empty).Trim('/', '\\');
				PackFile(zipfile, filepath, Path.Combine(targetName, fileName));
			}
		}

		private void PackFile(ZipOutputStream zipfile, string filePath, string targetName)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			ZipEntry zipentry = new ZipEntry(targetName);
			zipentry.DateTime = fileInfo.LastWriteTime;
			zipentry.Size = fileInfo.Length;

			zipfile.PutNextEntry(zipentry);

			using (FileStream streamReader = File.OpenRead(filePath))
				StreamUtils.Copy(streamReader, zipfile, new byte[4096]);

			zipfile.CloseEntry();
		}
	}
}
