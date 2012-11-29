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
			result.AppendLine("Scans the specified directory for sage extension projects,");
			result.AppendLine("then builds and installs them as extensions in any project that depend on them.\n");
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
				var fileInfo = new FileInfo(projectFile);
				var projectConfig = ProjectConfiguration.Create(fileInfo.FullName); 
				var projectName = projectConfig.Name ??
					string.Format("Project.{0}.{1}", fileInfo.Directory.Name, projects.Count + 1);

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

			List<string> visitedProjects = new List<string>();

			Action<ProjectInfo> copyDependencies = null;
			copyDependencies = projectInfo =>
			{
				if (visitedProjects.Contains(projectInfo.Name))
					return;

				foreach (string extensionName in projectInfo.Dependencies)
				{
					if (!extensions.ContainsKey(extensionName))
					{
						log.ErrorFormat("Extension {0} (dependency of project {1}) could not be found", extensionName, projectInfo.Name);
						continue;
					}

					string extensionFile = extensions[extensionName];
					string targetPath = Path.Combine(projectInfo.Path, "extensions", Path.GetFileName(extensionFile));
					if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
						Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

					File.Copy(extensionFile, targetPath, true);
					log.InfoFormat("Copied extension {0} to project {1}", extensionName, projectInfo.Name);
					log.DebugFormat("\t({0})", targetPath);

					ProjectInfo info = projects[extensionName];
					copyDependencies(info);
				}

				visitedProjects.Add(projectInfo.Name);
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
