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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Xml;

	using Kelp.Extensions;
	using Kelp.ResourceHandling;
	using log4net;
	using Sage.Extensibility;

	/// <summary>
	/// Implements an <see cref="XmlResolver"/> that reads the contents of special 'global' and 'category' css
	/// and script files, and provides a reader around a document generated with elements representing the 
	/// the included files.
	/// </summary>
	[UrlResolver(Scheme = CodeFileUnmergeResolver.Scheme)]
	public class CodeFileUnmergeResolver : IUrlResolver
	{
		/// <summary>
		/// The scheme associated with this resolver.
		/// </summary>
		public const string Scheme = "kelp";

		internal const string RelativeFilePathInProductionSetup = "{0}?{1}";
		internal const string RelativeFilePathInDevelopmentSetup = "{0}?noprocess=1";

		private static readonly ILog log = LogManager.GetLogger(typeof(CodeFileUnmergeResolver).FullName);

		/// <summary>
		/// Gets an <see cref="EntityResult"/> that represents the actual resource mapped from the specified <paramref name="uri"/>.
		/// </summary>
		/// <param name="parent">The <see cref="UrlResolver"/> that owns this resolved and calls this method.</param>
		/// <param name="context">The current <see cref="SageContext"/> under which this code is executing.</param>
		/// <param name="uri">The uri to resolve.</param>
		/// <returns>
		/// An object that represents the resource mapped from the specified <paramref name="uri"/>.
		/// </returns>
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
			string sourcePathRelative = context.Path.GetRelativeWebPath(sourcePathAbsolute, true);
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
			if (context.ProjectConfiguration.IsDebugEnabled)
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
							string path = string.Format(RelativeFilePathInDevelopmentSetup, context.Path.GetRelativeWebPath(absolutePath, true));
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
			else
			{
				XmlElement resourceNode = rootNode.AppendElement(document.CreateElement("kelp:resource", Kelp.XmlNamespaces.KelpNamespace));
				resourceNode.SetAttribute("src", string.Format(RelativeFilePathInProductionSetup, sourcePath, file.ETag));
				resourceNode.SetAttribute("type", type);
				resourceNode.SetAttribute("exists", "true");
			}

			return new EntityResult { Entity = reader, Dependencies = new List<string>(file.References.Keys) };
		}
	}
}
