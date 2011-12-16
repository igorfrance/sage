namespace Sage.BatchGlobalizer
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Xml;

	using Kelp.Core.Extensions;

	internal class Program
	{
		private static string reportPath;

		internal static string ApplicationPath
		{
			get
			{
				return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			}
		}

		internal static string TargetPath
		{
			get;
			private set;
		}

		internal static string Category
		{
			get;
			private set;
		}

		internal static string ReportPath
		{
			get 
			{
				return reportPath ?? ApplicationPath;
			}
		}

		internal static bool MergeAssets
		{
			get;
			private set;
		}

		internal static bool EmitSummary
		{
			get;
			private set;
		}

		private static string ProgramName
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Name.ToLower();
			}
		}
		
		/// <exception cref="Exception"/>
		internal static int Main(string[] args)
		{
			Program.ParseArguments(args);
			if (Program.TargetPath == null || Program.Category == null)
			{
				Program.ShowUsage();
				return 1;
			}

			if (!Directory.Exists(TargetPath))
			{
				Console.WriteLine("The target directory '{0}' doesn't exist.", TargetPath);
				return 1;
			}

			try
			{
				var glob = new BatchGlobalizer(Program.TargetPath, Program.Category);
				var reportDocumentPath = glob.GlobalizeResources();

				if (Program.MergeAssets)
					glob.MergeAssets();

				// Process.Start(reportDocumentPath);
			}
			catch (XmlException ex)
			{
				if (!string.IsNullOrEmpty(ex.SourceUri))
					Console.WriteLine("An XmlException occured while processing file '{0}':", ex.SourceUri);
				else
					Console.WriteLine("An XmlException occured:");

				Console.WriteLine(ex.Message);
				return 1;
			}
			catch (Exception ex)
			{
				Console.WriteLine("An unhandled exception occured:");
				Console.WriteLine(ex.InnermostExceptionMessage());
				return 1;
			}

			return 0;
		}

		private static void ShowUsage()
		{
			Console.WriteLine("Wl Resource Processor");
			Console.WriteLine("Batch-processes all globalizable resources within the specified category,");
			Console.WriteLine("optionally replacing all templates with their internationalized versions.\n");
			Console.WriteLine("Usage: {0} -targetpath:<path> -category:<name> [-summary:<path>] [-merge:<0|1>] [-emitsummary:<0|1>]\n", ProgramName);
			Console.WriteLine("  -targetpath:<path>	 The path to the directory that contains the resources to process.");
			Console.WriteLine("  -category:<name>    The name of the category being processed.");
			Console.WriteLine("  -reportpath:<path>	 The path to the directory in which to save the report files (html and xml).");
			Console.WriteLine("  -emitsummary:<1|0>  Unless set to 0, the program will emit a summary document for each globalized resource.\n");
			Console.WriteLine("  -merge:<1|0>        If set to 1, the original templates will be replaced with their internationalized versions.\n");
		}

		private static void ParseArguments(string[] args)
		{
			string targetPath = null;
			string category = null;
			string reports = null;
			bool mergeAssets = false;
			bool emitSummary = true;

			foreach (string arg in args)
			{
				if (arg.StartsWith("-targetpath:"))
				{
					targetPath = arg.Substring(12).Trim('"');
				}

				if (arg.StartsWith("-category:"))
				{
					category = arg.Substring(10).Trim('"');
				}

				if (arg.StartsWith("-reportpath:"))
				{
					reports = arg.Substring(12).Trim('"');
				}

				if (arg.StartsWith("-emitsummary:"))
				{
					emitSummary = arg.Substring(13) == "1";
				}

				if (arg.StartsWith("-merge:"))
				{
					mergeAssets = arg.Substring(7).ContainsAnyOf("yes", "1", "true");
				}
			}

			reportPath = reports;
			
			Program.TargetPath = targetPath;
			Program.Category = category;
			Program.MergeAssets = mergeAssets;
			Program.EmitSummary = emitSummary;
		}
	}
}
