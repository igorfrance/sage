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
namespace Sage.DevTools.Modules
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Extensions;

	using log4net;
	using log4net.Appender;
	using log4net.Repository;
	using log4net.Repository.Hierarchy;

	using Sage.Modules;
	using Sage.Views;

	public class LogViewerModule : IModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LogViewerModule));

		private const string DefaultLoggerName = "MainLogger";
		private const string LineFindExpression = @"\d\d\d\d-\d\d-\d\d \d\d:\d\d:\d\d,\d+ \[{0}\][\s\S]*?(?=(?:\d\d\d\d-\d\d-\d\d \d\d:\d\d:\d\d,\d+)|$)";
		private const string LinePartsExpression = @"(?'Date'(?'YY'\d\d\d\d)-(?'MM'\d\d)-(?'DD'\d\d)) +(?'Time'(?'HH'\d\d):(?'MIN'\d\d):(?'SEC'\d\d),(?'MILLI'\d+)) +\[(?'Thread'\S+)\] +(?'Severity'\S+) +(?'Logger'\S+) - (?'Message'[\s\S]*)";
		private const string TimingExpression = @"\b(\d+)ms\b";
		#region configuration-example

		private const string ConfigurationExample = @"
<appender name=""{0}"" type=""log4net.Appender.RollingFileAppender"">
	<file value=""log/sage.log"" />
	<appendToFile value=""true"" />
	<maximumFileSize value=""2048KB"" />
	<layout type=""log4net.Layout.PatternLayout"">
		<conversionPattern value=""%d [%t] %-5p %c - %m%n"" />
	</layout>
</appender>";

		#endregion

		private static readonly Regex logLineExpr = new Regex(LinePartsExpression);
		private static readonly Regex logTimeExpr = new Regex(TimingExpression);

		private bool IsLoggingEnabled
		{
			get
			{
				return ((Hierarchy) log4net.LogManager.GetRepository()).Root.Level != log4net.Core.Level.Off;
			}
		}

		public ModuleResult ProcessElement(XmlElement moduleElement, ViewConfiguration configuration)
		{
			XmlNamespaceManager nm = XmlNamespaces.Manager;

			bool isPublic = moduleElement.GetBoolean("mod:config/mod:public", nm);
			long threadName = moduleElement.GetLong("mod:config/mod:thread", 0, nm);
			string loggerName = moduleElement.GetString("mod:config/mod:logger", DefaultLoggerName, nm);

			SageContext context = configuration.Context;
			if (!isPublic && !context.IsDeveloperRequest)
			{
				log.Warn("Skipping work for non-developer reqest. To override this set mod:config/mod:public to 'true'.");
				return null;
			}

			if (threadName == 0)
			{
				log.Warn("Thread name parameter is missing. Set mod:config/mod:thread be a non-empty string that parses as long integer.");
				return new ModuleResult(ModuleResultStatus.MissingParameters);
			}

			if (!IsLoggingEnabled)
			{
				log.Warn("Logging is disabled.");
				log.WarnFormat("Make sure to enable log4net logging, and to add an appender with the following configuration:");
				log.WarnFormat(ConfigurationExample, loggerName);
				return new ModuleResult(ModuleResultStatus.NoData);
			}

			FileAppender appender = GetAppender(loggerName) as FileAppender;
			if (appender == null)
			{
				log.WarnFormat("The log appender '{0}' does not exist or is not a FileAppender.", loggerName);
				log.WarnFormat("Either create an appender with that name or specify another using mod:config/mod:logger.");
				log.WarnFormat("Make sure to enable log4net logging, and to add an appender with the following configuration:");
				log.WarnFormat(ConfigurationExample, loggerName);
				return new ModuleResult(ModuleResultStatus.NoData);
			}

			ModuleResult result = new ModuleResult(moduleElement);
			result.AppendDataElement(CreateLogElement(moduleElement.OwnerDocument, appender, threadName));

			return result;
		}

		private static XmlElement CreateLogElement(XmlDocument ownerDoc, FileAppender appender, long threadName)
		{
			XmlElement logNode = ownerDoc.CreateElement("mod:log", XmlNamespaces.ModulesNamespace);
			IEnumerable<string> logLines = GetThreadLogs(appender, threadName.ToString());
			if (logLines != null)
			{
				long requestStart = threadName;
				foreach (string t in logLines)
				{
					Match match;
					if ((match = logLineExpr.Match(t)).Success)
					{
						int yy = int.Parse(match.Groups["YY"].Value);
						int mm = int.Parse(match.Groups["MM"].Value);
						int dd = int.Parse(match.Groups["DD"].Value);
						int hh = int.Parse(match.Groups["HH"].Value);
						int min = int.Parse(match.Groups["MIN"].Value);
						int sec = int.Parse(match.Groups["SEC"].Value);
						int milli = int.Parse(match.Groups["MILLI"].Value);

						DateTime logTime = new DateTime(yy, mm, dd, hh, min, sec, milli);
						long difference = logTime.Ticks - requestStart;

						TimeSpan elapsed = new TimeSpan(difference > 0 ? difference : 0);

						string message = match.Groups["Message"].Value;

						XmlElement lineNode = logNode.AppendElement(ownerDoc.CreateElement("mod:line", XmlNamespaces.ModulesNamespace));
						lineNode.SetAttribute("severity", match.Groups["Severity"].Value);
						lineNode.SetAttribute("date", match.Groups["Date"].Value);
						lineNode.SetAttribute("time", match.Groups["Time"].Value);
						lineNode.SetAttribute("elapsed", Math.Round(elapsed.TotalMilliseconds, 0).ToString());

						Match m;
						if ((m = logTimeExpr.Match(message)).Success)
							lineNode.SetAttribute("duration", m.Groups[0].Value);

						lineNode.SetAttribute("thread", match.Groups["Thread"].Value);
						lineNode.SetAttribute("logger", match.Groups["Logger"].Value);
						lineNode.SetAttribute("message", match.Groups["Message"].Value);
					}
				}
			}

			return logNode;
		}

		private static IEnumerable<string> GetThreadLogs(FileAppender appender, string threadName)
		{
			string contents = GetLogContents(appender);
			List<string> logs = new List<string>();
			Regex expr = new Regex(string.Format(LineFindExpression, threadName), RegexOptions.Multiline);
			foreach (Match match in expr.Matches(contents))
				logs.Add(match.Groups[0].Value);

			return logs;
		}

		private static string GetLogContents(FileAppender appender)
		{
			string contents;
			using (FileStream stream = System.IO.File.Open(appender.File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				byte[] bytes = new byte[stream.Length];
				stream.Lock(0, bytes.Length);
				stream.Read(bytes, 0, bytes.Length);
				stream.Unlock(0, bytes.Length);

				contents = Encoding.UTF8.GetString(bytes);
			}

			return contents;
		}

		private static IAppender GetAppender(string loggerName)
		{
			ILoggerRepository rootRepository = LogManager.GetRepository();
			IAppender[] allAppenders = rootRepository.GetAppenders();
			IAppender appender = allAppenders.FirstOrDefault(t => t.Name == loggerName);

			return appender;
		}
	}
}
