namespace Sage.DevTools.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web.Mvc;
	using System.Xml;

	using Kelp.Core.Extensions;

	using Sage.Controllers;

	using log4net;
	using log4net.Appender;
	using log4net.Repository;
	using log4net.Repository.Hierarchy;

	using Sage.Configuration;

	/// <summary>
	/// Implements the controller that server the log file views.
	/// </summary>
	[SharedController]
	public class LogViewController : SageController
	{
		private const string MainLogger = "MainLogger";
		private const string LineFindExpression = @"\d\d\d\d-\d\d-\d\d \d\d:\d\d:\d\d,\d+ \[{0}\][\s\S]*?(?=(?:\d\d\d\d-\d\d-\d\d \d\d:\d\d:\d\d,\d+)|$)";
		private const string LinePartsExpression = @"(?'Date'(?'YY'\d\d\d\d)-(?'MM'\d\d)-(?'DD'\d\d)) (?'Time'(?'HH'\d\d):(?'MIN'\d\d):(?'SEC'\d\d),(?'MILLI'\d+)) \[(?'Thread'\S+)\] (?'Severity'\S+) (?'Logger'\S+) - (?'Message'[\s\S]*)";
		private const string TimingExpression = @"\b(\d+)ms\b";

		private static readonly Regex logLineExpr = new Regex(LinePartsExpression);
		private static readonly Regex logTimeExpr = new Regex(TimingExpression);

		private bool IsLoggingEnabled
		{
			get
			{
				return ((Hierarchy)log4net.LogManager.GetRepository()).Root.Level != log4net.Core.Level.Off;
			}
		}

		/// <summary>
		/// Services requests for the log file view.
		/// </summary>
		/// <returns>The result of processing this view.</returns>
		public ActionResult ViewLog()
		{
			if (!Context.IsDeveloperRequest)
				return PageNotFound();

			return View("complete-log");
		}

		/// <summary>
		/// Services requests for the log file view.
		/// </summary>
		/// <returns>The result of processing this view.</returns>
		public ActionResult ViewThread(long? threadID)
		{
			if (!Context.IsDeveloperRequest)
				return PageNotFound();

			if (!IsLoggingEnabled)
			{
				EnableLogging();
				return new ContentResult
				{
					ContentType = "text/plain",
					Content = string.Concat(
						"Because logging was disabled, the current view is empty. Logging has just been enabled\n",
						"and will be available from now, but you need to repeat the original request to see its log output.")
				};
			}

			// this.ViewData["path"] = this.Context.Path.Resolve(

			if (threadID != null)
				this.ViewData["log"] = CreateLogDocumentForThread(threadID.Value);

			return View("thread-log");
		}

		private static FileAppender GetAppender()
		{
			ILoggerRepository rootRepository = LogManager.GetRepository();
			IAppender devAppender = null;
			IAppender[] allAppenders = rootRepository.GetAppenders();

			for (int i = 0; i < allAppenders.Length; i++)
			{
				if (allAppenders[i].Name == MainLogger)
				{
					devAppender = allAppenders[i];
					break;
				}
			}

			if (devAppender == null)
			{
				throw new ConfigurationError(string.Format(
					"To view the log contents, a file appender with name '{0}' needs to be added to the log4net configuration. Also make sure it's conversion pattern is set to %d [%t] %-5p %c - %m%n.", MainLogger));
			}

			if (!(devAppender is FileAppender))
			{
				throw new ConfigurationError(string.Format(
					"The appender with name '{0}' needs to be configured as a {1}. Also make sure it's conversion pattern is set to %d [%t] %-5p %c - %m%n..", MainLogger, typeof(FileAppender).FullName));
			}
			
			return (FileAppender) devAppender;
		}

		private static string GetLogContents()
		{
			FileAppender appender = GetAppender();
			
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

		private static IEnumerable<string> GetThreadLogs(string threadID)
		{
			string contents = GetLogContents();
			List<string> logs = new List<string>();
			Regex expr = new Regex(string.Format(LineFindExpression, threadID), RegexOptions.Multiline);
			foreach (Match match in expr.Matches(contents))
				logs.Add(match.Groups[0].Value);

			return logs;
		}

		private static void EnableLogging()
		{
			((Hierarchy)log4net.LogManager.GetRepository()).Root.Level = log4net.Core.Level.All;
		}

		private static XmlDocument CreateLogDocumentForThread(long threadID)
		{
			XmlDocument doc = new XmlDocument();
			XmlElement logNode = doc.AppendElement(doc.CreateElement("log"));
			IEnumerable<string> logLines = GetThreadLogs(threadID.ToString());
			if (logLines != null)
			{
				Match match;
				long requestStart = threadID;
				foreach (string t in logLines)
				{
					if ((match = logLineExpr.Match(t)).Success)
					{
						int yy = Int32.Parse(match.Groups["YY"].Value);
						int mm = Int32.Parse(match.Groups["MM"].Value);
						int dd = Int32.Parse(match.Groups["DD"].Value);
						int hh = Int32.Parse(match.Groups["HH"].Value);
						int min = Int32.Parse(match.Groups["MIN"].Value);
						int sec = Int32.Parse(match.Groups["SEC"].Value);
						int milli = Int32.Parse(match.Groups["MILLI"].Value);

						DateTime logTime = new DateTime(yy, mm, dd, hh, min, sec, milli);
						long difference = logTime.Ticks - requestStart;

						TimeSpan elapsed = new TimeSpan(difference > 0 ? difference : 0);

						string message = match.Groups["Message"].Value;

						XmlElement lineNode = logNode.AppendElement(doc.CreateElement("line"));
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

			return doc;
		}
	}
}
