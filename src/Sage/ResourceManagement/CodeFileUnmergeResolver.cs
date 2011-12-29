namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Web.Caching;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Kelp.ResourceHandling;
	using log4net;
	using Sage.Configuration;

	/// <summary>
	/// Implements an <see cref="XmlResolver"/> that reads the contents of special 'global' and 'category' css
	/// and script files, and provides a reader around a document generated with elements representing the 
	/// the included files.
	/// </summary>
	[UrlResolver(Scheme = CodeFileUnmergeResolver.Scheme)]
	public class CodeFileUnmergeResolver : ISageXmlUrlResolver
	{
		public const string Scheme = "kelp";

		internal const string RelativeFilePathInProductionSetup = "{0}?{1}";
		internal const string RelativeFilePathInDevelopmentSetup = "{0}?noprocess=1";

		private static readonly ILog log = LogManager.GetLogger(typeof(CodeFileUnmergeResolver).FullName);

		public EntityResult GetEntity(UrlResolver parent, SageContext context, string uri)
		{
			string sourcePath = uri.Replace(Scheme + "://", string.Empty);
			EntityResult result = null;
			Stopwatch sw = new Stopwatch();
			long time = sw.TimeMilliseconds(() => result = GetClientResourceReader(context, sourcePath));
			log.DebugFormat("Time taken to get resource reader for '{0}': {1}ms", uri, time);
			
			return result;
		}

		private EntityResult GetClientResourceReader(SageContext context, string sourcePath)
		{
			XmlDocument document = new XmlDocument();
			XmlElement rootNode = document.AppendElement(document.CreateElement("kelp:resources", Kelp.XmlNamespaces.KelpNamespace));
			XmlReader reader = new XmlNodeReader(rootNode);

			string sourcePathAbsolute = context.Path.Resolve(sourcePath.StartsWith("/") ? sourcePath : "~/" + sourcePath);
			string sourcePathRelative = context.Path.GetRelativeWebPath(sourcePathAbsolute);
			if (sourcePathAbsolute == null)
				return new EntityResult { Entity = reader, Dependencies = new List<string>() };

			string type = Path.GetExtension(sourcePathAbsolute).Replace(".", string.Empty).ToLower();
			if (!File.Exists(sourcePathAbsolute))
			{
				XmlElement resourceNode = rootNode.AppendElement(document.CreateElement("kelp:resource", Kelp.XmlNamespaces.KelpNamespace));
				resourceNode.SetAttribute("type", type);
				resourceNode.SetAttribute("path", sourcePathAbsolute);
				resourceNode.SetAttribute("src", sourcePathRelative);
				resourceNode.SetAttribute("exists", "false");
				log.ErrorFormat("The resource include file for '{0}' doesn't exist", sourcePathRelative);
				return new EntityResult { Entity = reader, Dependencies = new List<string>() };
			}

			CodeFile file = CodeFile.Create(sourcePathAbsolute, sourcePathRelative, context.MapPath);
			if (context.Config.MergeResources)
			{
				XmlElement resourceNode = rootNode.AppendElement(document.CreateElement("kelp:resource", Kelp.XmlNamespaces.KelpNamespace));
				resourceNode.SetAttribute("src", string.Format(RelativeFilePathInProductionSetup, sourcePath, file.ETag));
				resourceNode.SetAttribute("type", type);
				resourceNode.SetAttribute("exists", "true");
			}
			else
			{
				try
				{
					foreach (string absolutePath in file.References.Keys)
					{
						string relativePath = file.References[absolutePath];
						XmlElement resourceNode = rootNode.AppendElement(document.CreateElement("kelp:resource", Kelp.XmlNamespaces.KelpNamespace));
						resourceNode.SetAttribute("path", absolutePath);
						resourceNode.SetAttribute("type", type);

						if (File.Exists(absolutePath))
						{
							string path = string.Format(RelativeFilePathInDevelopmentSetup, context.Path.GetRelativeWebPath(absolutePath));
							resourceNode.SetAttribute("src", path);
							resourceNode.SetAttribute("exists", "true");
						}
						else
						{
							resourceNode.SetAttribute("src", relativePath);
							resourceNode.SetAttribute("exists", "false");
						}
					}
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Could not process the code file includes: " + ex.Message);
				}
			}

			return new EntityResult { Entity = reader, Dependencies = new List<string>(file.References.Keys) };
		}
	}
}
