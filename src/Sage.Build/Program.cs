namespace Sage.Build
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Sage.Build.Utilities;

	internal class Program
	{
		private static readonly Dictionary<string, IUtility> utilities;

		static Program()
		{
			utilities = new Dictionary<string,IUtility>();
			foreach (Type type in typeof(Program).Assembly.GetTypes().Where(t => typeof(IUtility).IsAssignableFrom(t) && !t.IsInterface))
			{
				IUtility instance = (IUtility) type.GetConstructor(new Type[] { }).Invoke(new object[] { });
				utilities[instance.CommandName] = instance;
			}
		}

		internal static string ApplicationPath
		{
			get
			{
				return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			}
		}

		internal static string Name
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Name.ToLower();
			}
		}
		
		internal static int Main(string[] args)
		{
			if (args.Length == 0)
				return ShowUsage();

			string commandName = args[0];
			if (commandName == "help")
			{
				if (args.Length > 1 && utilities.ContainsKey(commandName))
				{
					IUtility utility = utilities[commandName];
					Console.Write(utility.GetUsage());
					return 1;
				}
			}

			if (utilities.ContainsKey(commandName))
			{
				IUtility utility = utilities[commandName];
				if (utility.ParseArguments(args))
				{
					try
					{
						utility.Run();
						return 0;
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
						Console.WriteLine(ex.InnermostExceptionTypeName());
						Console.WriteLine(ex.InnermostExceptionMessage());
						Console.WriteLine(ex.InnermostExceptionStackTrace());
						return 1;
					}
				}

				Console.Write(utility.GetUsage());
				return 1;
			}

			return ShowUsage();
		}

		private static int ShowUsage()
		{
			StringBuilder usage = new StringBuilder();
			usage.AppendLine("Provides several command-line utilities for working with Sage projects");
			usage.AppendLine("The available commands are:");

			foreach (string command in utilities.Keys)
			{
				usage.AppendFormat("  - {0}\n", command);
			}

			usage.AppendLine();
			usage.AppendFormat("Type {0} help <command> to get more information about that command.\n", Program.Name);

			Console.Write(usage.ToString());

			return 1;
		}
	}
}
