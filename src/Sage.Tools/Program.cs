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
namespace Sage.Tools
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Web;
	using System.Xml;

	using Kelp.Extensions;
	using Kelp.HttpMock;

	using Sage.Configuration;
	using Sage.Tools.Utilities;

	internal class Program
	{
		private static readonly Dictionary<string, IUtility> utilities;

		static Program()
		{
			utilities = new Dictionary<string, IUtility>();
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
							Console.WriteLine("An XmlException occurred while processing file '{0}':", ex.SourceUri);
						else
							Console.WriteLine("An XmlException occurred:");

						Console.WriteLine(ex.Message);
						return 1;
					}
					catch (Exception ex)
					{
						Console.WriteLine("An unhandled exception occurred:");
						Console.WriteLine(ex.RootTypeName());
						Console.WriteLine(ex.RootMessage());
						Console.WriteLine(ex.RootStackTrace());
						return 1;
					}
				}

				Console.Write(utility.GetUsage());
				return 1;
			}

			return ShowUsage();
		}

		internal static string MapPath(string path)
		{
			if (path == "/")
				path = "~/";

			string result = path.Replace(
				"~", ApplicationPath).Replace(
				"//", "/").Replace(
				"/", "\\");

			return new FileInfo(result).FullName;
		}

		internal static HttpContextBase CreateHttpContext(string url)
		{
			return CreateHttpContext(url, "default.aspx");
		}

		internal static HttpContextBase CreateHttpContext(string url, string physicalPath)
		{
			return new HttpContextMock(url, physicalPath, null, null, Program.MapPath);
		}

		internal static SageContext CreateSageContext(string url, Func<string, string> pathMapper, ProjectConfiguration config)
		{
			HttpContextBase httpContext =
				Program.CreateHttpContext(url, "default.aspx");

			SageContext context = new SageContext(httpContext, pathMapper, config);
			return context;
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
