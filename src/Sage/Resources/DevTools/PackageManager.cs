namespace Sage.Resources.DevTools
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;

	using Sage.ResourceManagement;

	internal class PackageManager
	{
		internal const string DevToolsResourcePath = "sage/resources/devtools/";

		internal static List<string> GetPackageResources()
		{
			return GetPackageResources(null);
		}

		internal static List<string> GetPackageResources(string prefixDir)
		{
			Assembly source = Assembly.GetExecutingAssembly();
			List<string> items = new List<string>(source.GetManifestResourceNames());
			List<string> result = new List<string>();
			
			foreach (var item in items)
			{
				string path = item.Replace(".", "/");
				if (path.ToLower().StartsWith(DevToolsResourcePath))
				{
					if (string.IsNullOrEmpty(prefixDir))
					{
						result.Add(item);
					}
					else
					{
						result.Add(Path.Combine(prefixDir, PackageManager.GetTargetPath(item)).Replace("\\", "/"));
					}
				}
			}

			return result;
		}

		internal static void InstallResourcesTo(string targetDir, string webPath)
		{
			List<string> files = GetPackageResources();
			foreach (string file in files)
			{
				string targetName = GetTargetPath(file);
				string fileExt = Path.GetExtension(targetName);
				string filePath = Path.Combine(targetDir, targetName);
				string fileDir = Path.GetDirectoryName(filePath);

				Directory.CreateDirectory(fileDir);

				if (fileExt == ".xsl" || fileExt == ".xslt")
				{
					string content = ResourceManager.GetEmbeddedResourceAsString(file);
					File.WriteAllText(filePath, content.Replace("{DevTools}", webPath.Trim('/')));
				}
				else
				{
					using (Stream fileStream = ResourceManager.GetEmbeddedResourceAsStream(file))
					{
						byte[] fileContent = new byte[fileStream.Length];
						fileStream.Read(fileContent, 0, fileContent.Length);

						File.WriteAllBytes(filePath, fileContent);
					}
				}
			}
		}

		internal static List<string> GetMissingResources(string targetDir)
		{
			List<string> resources = GetPackageResources(targetDir);
			List<string> missing = new List<string>();
			foreach (var targetPath in resources)
			{
				if (!File.Exists(targetPath))
					missing.Add(targetPath);
			}

			return missing;
		}

		internal static string GetTargetPath(string embeddedPath)
		{
			string path = embeddedPath.Substring(
				0, embeddedPath.Length - (embeddedPath.Length - embeddedPath.LastIndexOf("."))).Replace(".", "/");

			string ext = embeddedPath.Substring(
				embeddedPath.LastIndexOf("."), embeddedPath.Length - embeddedPath.LastIndexOf("."));

			return string.Concat(path.Replace(DevToolsResourcePath, string.Empty), ext);
		}
	}
}
