namespace Sage.Tools.Utilities
{
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using System.Linq;
	using System.Text;

	using Kelp.Extensions;
	using Kelp.HttpMock;

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
				return "install-extension";
			}
		}

		public bool ParseArguments(string[] args)
		{
			arguments = new NameValueCollection();
			foreach (string arg in args)
			{
				if (arg.StartsWith("-directory:"))
					arguments["directory"] = arg.Substring(11).Trim('"');

				if (arg.StartsWith("-extension:"))
					arguments["extension"] = arg.Substring(11).Trim('"');
			}

			return arguments["directory"] != null && arguments["extension"] != null;
		}

		public string GetUsage()
		{
			StringBuilder result = new StringBuilder();
			result.AppendLine("Builds the the specified Sage extension and installs it into all Sage projects within");
			result.AppendLine("the specified directory that depend on it.");
			result.AppendFormat("Usage: {0} {1} -extension:<path> -directory:<path>\n", Program.Name, this.CommandName);
			result.AppendLine("  -extension:<path>	 The path to the Project.config file of the Sage extension project to build.");
			result.AppendLine("  -directory:<path>	 The path to the directory that in which to look for extensions.");

			return result.ToString();
		}

		public void Run()
		{
			var extensionConfigPath = arguments["extension"].Trim();
			var workingDirectory = arguments["directory"].Trim();

			if (!File.Exists(extensionConfigPath))
			{
				log.ErrorFormat("The extension configuration file '{0}' doesn't exist", extensionConfigPath);
				return;
			}

			if (!Directory.Exists(workingDirectory))
			{
				log.ErrorFormat("Directory '{0}' doesn't exist", workingDirectory);
				return;
			}

			var builder = new ExtensionBuilder();
			var fileInfo = new FileInfo(extensionConfigPath);
			var extensionConfig = ProjectConfiguration.Create(fileInfo.FullName);

			if (string.IsNullOrEmpty(extensionConfig.Name))
			{
				log.ErrorFormat("The extension needs a name, build cancelled");
				return;
			}

			log.InfoFormat("Building extension {0}.", extensionConfig.Name);

			var extensionFile = Path.Combine(fileInfo.DirectoryName, extensionConfig.Name + ".zip");
			builder.BuildExtension(fileInfo.DirectoryName, extensionFile);

			var projectFiles = Directory
				.GetFiles(workingDirectory, "Project.config", SearchOption.AllDirectories)
				.Where(s => s != fileInfo.FullName && !s.Contains(@"\bin\"));

			foreach (var projectFile in projectFiles)
			{
				var projectInfo = new FileInfo(projectFile);
				var projectConfig = ProjectConfiguration.Create(projectInfo.FullName);

				if (projectConfig.Dependencies.Contains(extensionConfig.Name))
				{
					var httpContext = new HttpContextMock("/");
					var projectContext = new SageContext(httpContext, delegate(string path)
					{
						path = path.ReplaceAll("^~?/", string.Empty).ReplaceAll("/", "\\");
						if (!Path.IsPathRooted(path))
							path = Path.Combine(projectInfo.DirectoryName, path);

						return path;

					},	projectConfig);

					string installPath = Path.Combine(projectContext.Path.ExtensionPath, Path.GetFileName(extensionFile));
					File.Copy(extensionFile, installPath, true);
				}
			}
		}
	}
}
