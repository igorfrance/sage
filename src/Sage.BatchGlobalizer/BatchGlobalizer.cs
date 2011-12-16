namespace Sage.BatchGlobalizer
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Kelp.HttpMock;

	using log4net;

	using Sage.Configuration;
	using Sage.ResourceManagement;
	using Sage.Xml;

	internal class BatchGlobalizer
	{
		internal const string SummaryXmlName = "globalization-summary-{0}.xml";
		internal const string SummaryHtmlName = "globalization-summary-{0}.html";

		private static readonly ILog log = LogManager.GetLogger(typeof(Globalizer).FullName);
		private const string XsltPath = "Resources/Xslt/Summary.xslt";

		private readonly string targetPath;
		private readonly string categoryPath;
		private readonly string category;
		private readonly ProjectConfiguration config;
		private readonly SageContext context;

		private static XsltTemplate processor;

		public BatchGlobalizer(string targetPath, string category)
		{
			HttpContextMock httpContext = new HttpContextMock("whitelabel");

			this.targetPath = targetPath;
			this.category = category;
			this.config = ProjectConfiguration.Current;
			this.context = new SageContext(httpContext, this.category, this.MapPath);
			this.categoryPath = context.Path.GetPhysicalCategoryPath(this.category);
		}

		internal string GlobalizeResources()
		{
			log.InfoFormat("Globalizing resources for category '{0}' in directory '{1}'", this.category, this.categoryPath);

			DateTime started = DateTime.Now;
			XmlDocument summaryDoc = new XmlDocument();
			XmlElement globalization = summaryDoc.AppendElement(summaryDoc.CreateElement("globalization"));

			globalization.SetAttribute("date", started.ToString("dd-MM-yyyy"));
			globalization.SetAttribute("time", started.ToString("hh:mm:ss"));

			XmlElement result = globalization.AppendElement(summaryDoc.CreateElement("result"));
			XmlElement categorySummary = result.AppendElement(summaryDoc.CreateElement("category"));
			categorySummary.SetAttribute("name", this.category);

			CategoryInfo info = config.Categories[this.category];
			XmlElement localesElement = categorySummary.AppendElement(summaryDoc.CreateElement("locales"));
			foreach (string name in info.Locales)
			{
				LocaleInfo locale = config.Locales[name];
				XmlElement categoryNode = localesElement.AppendElement(summaryDoc.CreateElement("locale"));
				categoryNode.SetAttribute("name", name);
				categoryNode.SetAttribute("dictionaries", locale.DictionaryNames.Join(", "));
			}

			log.DebugFormat("Category {0} is configured for {1} locales: {2}", 
				category, info.Locales.Count, string.Join(",", info.Locales.ToArray()));

			XmlElement resourcesElement = categorySummary.AppendElement(summaryDoc.CreateElement("resources"));
			resourcesElement.SetAttribute("path", this.targetPath);

			if (Directory.Exists(this.categoryPath))
			{
				var files = this.GetFiles();
				if (files.Count() == 0)
				{
					log.Info("No globalizable files found.");
				}
				else
				{
					IEnumerable<XmlResource> resources = GetGlobalizableResources();
					foreach (XmlResource resource in resources)
					{
						log.InfoFormat("Processing {0}", resource.Name.Signature);
						GlobalizationSummary summary = resource.Globalize();
						XmlElement summaryElement = summary.ToXml(summaryDoc);
						resourcesElement.AppendChild(summaryElement);
					}
				}
			}
			else
			{
				log.DebugFormat("The target directory '{0}' for category {1} doesn't exist.",
					this.categoryPath, this.category);
			}

			DateTime completed = DateTime.Now;
			TimeSpan elapsed = new TimeSpan(completed.Ticks - started.Ticks);
			globalization.SetAttribute("duration", string.Format("{0}.{1:D3}s", elapsed.Seconds, elapsed.Milliseconds));

			return SummarizeOverview(summaryDoc, Program.ReportPath);
		}

		internal List<string> GetFiles()
		{
			string globDirName = config.PathTemplates.GlobalizedDirectory.Replace("/", string.Empty).Replace("\\", string.Empty);
			string[] skipDirs = new[] { "dictionary", globDirName };
			
			var files = new List<string>(Directory.GetFiles(this.categoryPath, "*.xml", SearchOption.AllDirectories))
				.Where(f => !f.ContainsAnyOf(skipDirs));

			return files.ToList();
		}

		internal void MergeAssets()
		{
			log.InfoFormat("Merging assets within {0}...", this.categoryPath);

			string searchName = Path.GetFileName(config.PathTemplates.GlobalizedDirectory.TrimEnd('/', '\\'));
			string[] directories = Directory.GetDirectories(this.categoryPath, searchName, SearchOption.AllDirectories);
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

		private string SummarizeOverview(XmlDocument resultDoc, string outputPath)
		{
			XmlElement resultElement = resultDoc.SelectSingleElement("/globalization/result");
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
				CacheableXslTransform transform = ResourceManager.LoadXslStylesheet(XsltPath, context);
				processor = new XsltTemplate(transform.Processor);
			}

			StringBuilder text = new StringBuilder();
			StringWriter writer = new StringWriter(text);
			processor.Transform(resultDoc, writer, context);

			string resultPath = Path.Combine(outputPath, string.Format(SummaryXmlName, this.category));
			string reportPath = Path.Combine(outputPath, string.Format(SummaryHtmlName, this.category));
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
					return resultPath;
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
				return resultPath;
			}

			try
			{
				resultDoc.Save(resultPath);
				Console.WriteLine("Saved report XML to {0}", resultPath);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to save the summary xml document to the target location '{0}': {1}", resultPath, ex.Message);
				return resultPath;
			}

			return reportPath;
		}

		private string MapPath(string childPath)
		{
			string result = childPath.Replace(
				"~", this.targetPath).Replace(
				"//", "/").Replace(
				"/", "\\");

			return new FileInfo(result).FullName;
		}

		private IEnumerable<XmlResource> GetGlobalizableResources()
		{
			string globDirName = config.PathTemplates.GlobalizedDirectory.Replace("/", string.Empty).Replace("\\", string.Empty);
			string[] skipDirs = new[] { "dictionary", globDirName };
			
			var files = new List<string>(Directory.GetFiles(this.categoryPath, "*.xml", SearchOption.AllDirectories))
				.Where(f => !f.ContainsAnyOf(skipDirs));

			List<XmlResource> result = new List<XmlResource>();
			foreach (string filePath in files)
			{
				XmlResource resource = new XmlResource(filePath, context);
				XmlDocument sourceDoc = resource.LoadSourceDocument(this.context.Locale);
				if (resource.IsGlobalizable(sourceDoc) && result.Where(r => r.Name.Signature == resource.Name.Signature).Count() == 0)
					result.Add(resource);
			}

			return result;
		}
	}
}
