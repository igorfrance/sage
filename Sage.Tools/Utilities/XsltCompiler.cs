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
namespace Sage.Tools.Utilities
{
	using System;
	using System.IO;
	using System.Text;
	using System.Xml;

	using Sage.Configuration;
	using Sage.Modules;

	internal class XsltCompiler : IUtility
	{
		private string input;
		private string output;
		private string configPath;

		private const string MODULES_XSLT = "MODULES_XSLT";

		public string CommandName
		{
			get
			{
				return "xmldoc";
			}
		}

		public bool ParseArguments(string[] args)
		{
			foreach (string arg in args)
			{
				if (arg.StartsWith("-config:"))
				{
					configPath = arg.Substring(8).Trim('\'', '"');
				}
				if (arg.StartsWith("-input:"))
				{
					input = arg.Substring(7).Trim('\'', '"');
				}
				if (arg.StartsWith("-output:"))
				{
					output = arg.Substring(8).Trim('\'', '"');
				}
			}

			if (string.IsNullOrWhiteSpace(configPath))
				return false;

			if (string.IsNullOrWhiteSpace(input))
				return false;

			if (string.IsNullOrWhiteSpace(output))
				return false;

			configPath = Program.MapPath(configPath);
			output = Program.MapPath(output);

			if (input != MODULES_XSLT)
				input = Program.MapPath(input);

			if (!File.Exists(configPath))
			{
				Console.WriteLine("The path {0} is not a valid project configuration path", configPath);
				return false;
			}

			if (input != MODULES_XSLT && !File.Exists(input))
			{
				Console.WriteLine("The path {0} is not a valid xslt stylesheet path", input);
				return false;
			}

			return true;
		}

		public string GetUsage()
		{
			var result = new StringBuilder();
			result.AppendLine("Loads and sage-resolves xml resources.\n");
			result.AppendFormat("Usage: {0} {1} -config:<config> -input:<input> -output:<output>\n", Program.Name, this.CommandName);
			result.AppendLine("-compile: <compile> Type of compilation to perform. Values: modules");
			result.AppendLine("  -input: <input> The path of the xml resource to open.");
			result.AppendLine("          If this value equals constant value MODULES_XSLT, the special");
			result.AppendLine("          built-in function will be called returning the complete combined module xslt markup.");
			result.AppendLine("          combined module xslt markup.");
			result.AppendLine(" -output: <output> The path to save the processed and resolved document to");

			return result.ToString();
		}

		public void Run()
		{
			Console.WriteLine("Creating project configuration...");
			var config = ProjectConfiguration.Create(this.configPath);

			Console.WriteLine("Creating sage context...");
			var context = Program.CreateSageContext("/", config);

			XmlDocument resultXml;

			if (input == MODULES_XSLT)
			{
				Console.WriteLine("Combining the module XSLT...");
				resultXml = ModuleConfiguration.CombineModuleXslt(context);
			}
			else
			{
				Console.WriteLine("Loading xml from '{0}'...", input);
				resultXml = context.Resources.LoadXml(input);
			}

			Console.WriteLine("Saving xml to '{0}'...", output);
			resultXml.Save(output);
		}
	}
}
