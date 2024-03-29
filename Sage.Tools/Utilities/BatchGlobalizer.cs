﻿/**
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
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Extensions;
	using Kelp.HttpMock;
	using log4net;
	using Sage.Configuration;
	using Sage.ResourceManagement;
	using Sage.Views;

	internal class BatchGlobalizer : IUtility
	{
		internal const string SummaryXmlName = "internationalization-summary-{0}.xml";
		internal const string SummaryHtmlName = "internationalization-summary-{0}.html";

		private const string XsltPath = "Resources/Xslt/Summary.xslt";
		private static readonly ILog log = LogManager.GetLogger(typeof(Internationalizer).FullName);
		private static XsltTransform processor;

		private string categoryPath;
		private SageContext context;
		private NameValueCollection arguments;

		public string CommandName
		{
			get
			{
				return "globalize";
			}
		}

		public string GetUsage()
		{
			StringBuilder result = new StringBuilder();
			result.AppendLine("Batch-processes all globalizable resources within the specified category,");
			result.AppendLine("optionally replacing them with their internationalized versions.\n");
			result.AppendFormat("Usage: {0} {1} -targetpath:<path> -category:<name> [-summary:<path>] [-merge:<0|1>] [-emitsummary:<0|1>]\n", Program.Name, this.CommandName);
			result.AppendLine("  -targetpath:<path>	 The path to the directory that contains the resources to process.");
			result.AppendLine("  -category:<name>    The name of the category being processed.");
			result.AppendLine("  -reportpath:<path>	 The path to the directory in which to save the report files (html and xml).");
			result.AppendLine("  -emitsummary:<1|0>  Unless set to 0, the program will emit a summary document for each globalized resource.\n");
			result.AppendLine("  -merge:<1|0>        If set to 1, the original templates will be replaced with their internationalized versions.\n");

			return result.ToString();
		}

		public bool ParseArguments(string[] args)
		{
			arguments = new NameValueCollection();
			foreach (string arg in args)
			{
				if (arg.StartsWith("-targetpath:"))
				{
					arguments["targetPath"] = arg.Substring(12).Trim('"');
				}

				if (arg.StartsWith("-category:"))
				{
					arguments["category"] = arg.Substring(10).Trim('"');
				}

				if (arg.StartsWith("-reportpath:"))
				{
					arguments["reportpath"] = arg.Substring(12).Trim('"');
				}

				if (arg.StartsWith("-emitsummary:"))
				{
					arguments["emitSummary"] = arg.Substring(13) == "1" ? "1" : "0";
				}

				if (arg.StartsWith("-merge:"))
				{
					arguments["mergeAssets"] = arg.Substring(7).ContainsAnyOf("yes", "1", "true") ? "1" : "0";
				}
			}

			if (arguments["targetPath"] != null && arguments["category"] != null)
			{
				HttpContextMock httpContext = new HttpContextMock("sage");

				context = new SageContext(httpContext, arguments["category"], this.MapPath);
				categoryPath = context.Path.GetPhysicalCategoryPath(arguments["category"]);

				return true;
			}

			return false;
		}

		public void Run()
		{
			if (!Directory.Exists(arguments["targetPath"]))
			{
				throw new ArgumentException(string.Format("The target directory '{0}' doesn't exist.", arguments["targetPath"]));
			}

			this.GlobalizeResources();
			if (arguments["mergeAssets"] == "1")
				this.MergeAssets();
		}

		internal void GlobalizeResources()
		{
			log.InfoFormat("Globalizing resources for category '{0}' in directory '{1}'", 
				arguments["category"], categoryPath);

			DateTime started = DateTime.Now;
			XmlDocument summaryDoc = new XmlDocument();
			XmlElement internationalization = summaryDoc.AppendElement(summaryDoc.CreateElement("internationalization"));

			internationalization.SetAttribute("date", started.ToString("dd-MM-yyyy"));
			internationalization.SetAttribute("time", started.ToString("hh:mm:ss"));

			XmlElement result = internationalization.AppendElement(summaryDoc.CreateElement("result"));
			XmlElement categorySummary = result.AppendElement(summaryDoc.CreateElement("category"));
			categorySummary.SetAttribute("name", arguments["category"]);

			CategoryInfo info = context.ProjectConfiguration.Categories[arguments["category"]];
			XmlElement localesElement = categorySummary.AppendElement(summaryDoc.CreateElement("locales"));
			foreach (string name in info.Locales)
			{
				LocaleInfo locale = context.ProjectConfiguration.Locales[name];
				XmlElement categoryNode = localesElement.AppendElement(summaryDoc.CreateElement("locale"));
				categoryNode.SetAttribute("name", name);
				categoryNode.SetAttribute("dictionaries", string.Join(", ", locale.DictionaryNames));
			}

			log.DebugFormat("Category {0} is configured for {1} locales: {2}",
				arguments["category"], info.Locales.Count, string.Join(",", info.Locales.ToArray()));

			XmlElement resourcesElement = categorySummary.AppendElement(summaryDoc.CreateElement("resources"));
			resourcesElement.SetAttribute("path", arguments["targetPath"]);

			if (Directory.Exists(categoryPath))
			{
				var files = this.GetFiles();
				if (files.Count() == 0)
				{
					log.Info("No globalizable files found.");
				}
				else
				{
					IEnumerable<XmlResource> resources = this.GetGlobalizableResources();
					foreach (XmlResource resource in resources)
					{
						log.InfoFormat("Processing {0}", resource.Name.Signature);
						InternationalizationSummary summary = resource.Globalize();
						XmlElement summaryElement = summary.ToXml(summaryDoc);
						resourcesElement.AppendChild(summaryElement);
					}
				}
			}
			else
			{
				log.DebugFormat("The target directory '{0}' for category {1} doesn't exist.",
					categoryPath, arguments["category"]);
			}

			DateTime completed = DateTime.Now;
			TimeSpan elapsed = new TimeSpan(completed.Ticks - started.Ticks);
			internationalization.SetAttribute("duration", string.Format("{0}.{1:D3}s", elapsed.Seconds, elapsed.Milliseconds));

			if (!string.IsNullOrWhiteSpace(arguments["reportpath"]))
				this.SummarizeOverview(summaryDoc, arguments["reportpath"]);
		}

		internal List<string> GetFiles()
		{
			string globDirName = context.ProjectConfiguration.PathTemplates.GlobalizedDirectory.Replace("/", string.Empty).Replace("\\", string.Empty);
			string[] skipDirs = new[] { "dictionary", globDirName };
			
			var files = new List<string>(Directory.GetFiles(categoryPath, "*.xml", SearchOption.AllDirectories))
				.Where(f => !f.ContainsAnyOf(skipDirs));

			return files.ToList();
		}

		internal void MergeAssets()
		{
			log.InfoFormat("Merging assets within {0}...", categoryPath);

			string searchName = Path.GetFileName(context.ProjectConfiguration.PathTemplates.GlobalizedDirectory.TrimEnd('/', '\\'));
			string[] directories = Directory.GetDirectories(categoryPath, searchName, SearchOption.AllDirectories);
			foreach (string folder in directories)
			{
				string targetDir = Path.GetDirectoryName(folder);
				string[] files = Directory.GetFiles(folder);
				foreach (string sourceFile in files)
				{
					string targetFile = Path.Combine(targetDir, Path.GetFileName(sourceFile));
					log.DebugFormat("Merging {0} to {1}", sourceFile, targetFile);
					if (File.Exists(targetFile))
						File.Delete(targetFile);

					File.Move(sourceFile, targetFile);
				}

				log.DebugFormat("Deleting target directory {0}", folder);
				Directory.Delete(folder);
			}

			log.InfoFormat("Done");
		}

		private void SummarizeOverview(XmlDocument resultDoc, string outputPath)
		{
			XmlElement resultElement = resultDoc.SelectSingleElement("/internationalization/result");
			foreach (XmlElement categoryElem in resultElement.SelectNodes("category"))
			{
				XmlElement phrasesElement = categoryElem.AppendElement(resultDoc.CreateElement("phrases"));
				foreach (XmlElement resourceNode in categoryElem.SelectNodes("resources/resource"))
				{
					string resourceName = string.Concat(
						Regex.Replace(resourceNode.GetAttribute("path"), @"^.*?([^\\]+\\[^\\]+)$", "$1"), "/",
						resourceNode.GetAttribute("name")).Replace("\\", "/");

					foreach (XmlElement phraseNode in resourceNode.SelectNodes("phrases/phrase"))
					{
						string phraseID = phraseNode.GetAttribute("id");
						XmlElement targetPhrase = phrasesElement.SelectSingleElement(string.Format("phrase[@id='{0}']", phraseID));
						if (targetPhrase == null)
						{
							targetPhrase = phrasesElement.AppendElement(resultDoc.CreateElement("phrase"));
							foreach (XmlAttribute attrNode in phraseNode.Attributes)
							{
								targetPhrase.SetAttribute(attrNode.Name, attrNode.Value);
							}
						}

						if (targetPhrase.SelectSingleNode(string.Format("resource[text()='{0}']", resourceName)) == null)
							targetPhrase.AppendElement(resultDoc.CreateElement("resource")).InnerText = resourceName;
					}
				}
			}

			if (processor == null)
			{
				processor = XsltTransform.Create(context, XsltPath);
			}

			StringBuilder text = new StringBuilder();
			StringWriter writer = new StringWriter(text);
			processor.Transform(resultDoc, writer, context);

			string resultPath = Path.Combine(outputPath, string.Format(SummaryXmlName, arguments["category"]));
			string reportPath = Path.Combine(outputPath, string.Format(SummaryHtmlName, arguments["category"]));
			if (!Directory.Exists(outputPath))
			{
				try
				{
					Directory.CreateDirectory(outputPath);
					Console.WriteLine("Created report directory at {0}", outputPath);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed to create report directory location '{0}': {1}", outputPath, ex.Message);
					return;
				}
			}

			try
			{
				File.WriteAllText(reportPath, text.ToString());
				Console.WriteLine("Saved report HTML to {0}", reportPath);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to write the result summary to the target location '{0}': {1}", reportPath, ex.Message);
				return;
			}

			try
			{
				resultDoc.Save(resultPath);
				Console.WriteLine("Saved report XML to {0}", resultPath);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to save the summary xml document to the target location '{0}': {1}", resultPath, ex.Message);
				return;
			}

			return;
		}

		private string MapPath(string childPath)
		{
			string result = childPath.Replace(
				"~", arguments["targetPath"]).Replace(
				"//", "/").Replace(
				"/", "\\");

			return new FileInfo(result).FullName;
		}

		private IEnumerable<XmlResource> GetGlobalizableResources()
		{
			string globDirName = context.ProjectConfiguration.PathTemplates.GlobalizedDirectory.Replace("/", string.Empty).Replace("\\", string.Empty);
			string[] skipDirs = new[] { "dictionary", globDirName };
			
			var files = new List<string>(Directory.GetFiles(categoryPath, "*.xml", SearchOption.AllDirectories))
				.Where(f => !f.ContainsAnyOf(skipDirs));

			List<XmlResource> result = new List<XmlResource>();
			foreach (string filePath in files)
			{
				XmlResource resource = new XmlResource(filePath, context);
				XmlDocument sourceDoc = resource.LoadSourceDocument(context.Locale);
				if (resource.IsGlobalizable(sourceDoc) && result.Count(r => r.Name.Signature == resource.Name.Signature) == 0)
					result.Add(resource);
			}

			return result;
		}
	}
}
