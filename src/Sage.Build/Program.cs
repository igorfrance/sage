﻿/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Sage.Build
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Web;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Kelp.HttpMock;

	using Sage.Build.Utilities;
	using Sage.Configuration;

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
