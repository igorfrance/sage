namespace Sage.Tools.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Linq;
	using System.Text;

	using Kelp.Extensions;

	using log4net;
	using Sage.Configuration;

	internal class ExtensionInstaller : IUtility
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionInstaller).FullName);
		private NameValueCollection arguments;

		public string CommandName
		{
			get
			{
				return "refresh-extensions";
			}
		}

		public bool ParseArguments(string[] args)
		{
			arguments = new NameValueCollection();
			foreach (string arg in args)
			{
				if (arg.StartsWith("-directory:"))
					arguments["directory"] = arg.Substring(11).Trim('"');
			}

			return arguments["directory"] != null;
		}

		public string GetUsage()
		{
			StringBuilder result = new StringBuilder();
			result.AppendLine("Looks for sage extension projects at any level within the specified directory, builds and installs them in dependent projects.\n");
			result.AppendFormat("Usage: {0} {1} -directory:<path>\n", Program.Name, this.CommandName);
			result.AppendLine("  -directory:<path>	 The path to the directory that in which to look for extensions.");

			return result.ToString();
		}

		public void Run()
		{
			var workingDirectory = arguments["directory"];

			if (!Directory.Exists(workingDirectory))
			{
				log.ErrorFormat("Directory '{0}' doesn't exist", workingDirectory);
				return;
			}

			log.InfoFormat("Building all extension projects in {0}.", workingDirectory);

			var temporaryDir = Path.Combine(workingDirectory, "temp");
			Directory.CreateDirectory(temporaryDir);

			var projectFiles = Directory
				.GetFiles(workingDirectory, "Project.config", SearchOption.AllDirectories)
				.Where(s => !s.Contains(@"\bin\"));

			var extensions = new Dictionary<string, string>();
			var projects = new Dictionary<string, ProjectInfo>();
			var builder = new ExtensionBuilder();
			foreach (var projectFile in projectFiles)
			{
				var absolutePath = new FileInfo(projectFile).FullName;
				var projectConfig = ProjectConfiguration.Create(absolutePath);
				var projectName = projectConfig.Name; 
				var projectPath = Path.GetDirectoryName(projectFile);

				if (projectConfig.Type == ProjectType.ExtensionProject)
				{
					var targetPath = Path.Combine(temporaryDir, projectName + ".zip");
					builder.BuildExtension(projectPath, targetPath);
					extensions.Add(projectName, targetPath);
				}

				projects.Add(projectName, new ProjectInfo 
				{ 
					Name = projectName, 
					Path = projectPath,
					Dependencies = projectConfig.Dependencies
				});
			}

			Action<ProjectInfo> copyDependencies = null;
			copyDependencies = pi =>
			{
				foreach (string extensionName in pi.Dependencies)
				{
					if (!extensions.ContainsKey(extensionName))
					{
						log.ErrorFormat("Extension {0} (dependency of project {1}) could not be found", extensionName, pi.Name);
						continue;
					}

					string extensionFile = extensions[extensionName];
					string targetPath = Path.Combine(pi.Path, "extensions");
					if (!Directory.Exists(targetPath))
						Directory.CreateDirectory(targetPath);

					File.Copy(extensionFile, targetPath, true);
					log.InfoFormat("Copied extension {0} to project {1} ({2}).", extensionName, pi.Name, targetPath);

					ProjectInfo info = projects[extensionName];
					copyDependencies(info);
				}
			};

			projects.Values.Each(copyDependencies);
		}

		private class ProjectInfo
		{
			public string Name { get; set; }

			public string Path { get; set; }

			public List<string> Dependencies { get; set; }
		}
	}
}
