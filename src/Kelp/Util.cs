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
namespace Kelp
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;

	using log4net;

	/// <summary>
	/// Contains various utility methods and properties that don't fit anywhere else.
	/// </summary>
	public static class Util
	{
		private const char Quotation = '"';
		private const char Apostrophe = '\'';
		private const char Escape = '\\';
		private const string Space = "\t\r\n\f ";

		private static readonly ILog log = LogManager.GetLogger(typeof(Util).FullName);

		/// <summary>
		/// Gets the physical path of the currently executing assembly in the .net framework temp directory.
		/// </summary>
		public static string ExecutingAssemblyLocation
		{
			get
			{
				return Assembly.GetExecutingAssembly()
						.Location
						.Replace("file:///", string.Empty)
						.Replace("/", "\\");
			}
		}

		/// <summary>
		/// Gets the physical path of the code currently executing assembly in the directory from which it 
		/// was copied to the .net framework temp directory.
		/// </summary>
		public static string ExecutingAssemblyCodeBase
		{
			get
			{
				return Assembly.GetExecutingAssembly()
						.CodeBase
						.Replace("file:///", string.Empty)
						.Replace("/", "\\");
			}
		}

		/// <summary>
		/// Gets the last modification date of the specified <paramref name="assembly"/>.
		/// </summary>
		/// <value>The last modification date of the specified <paramref name="assembly"/>.</value>
		public static DateTime? GetAssemblyDate(Assembly assembly)
		{
			if (assembly == null)
				return null;

			try
			{
				FileInfo fileInfo = new FileInfo(assembly.Location);
				return fileInfo.LastWriteTime;
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Could not read assembly date for assembly '{0}': {1}", assembly.FullName, ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Gets the list of assemblies that contains the specified <paramref name="source"/> assembly, and any
		/// loadable assemblies that reference it.
		/// </summary>
		/// <param name="source">The source assembly for which the list should be created.</param>
		/// <returns>The list of assemblies that contains the specified <paramref name="source"/> assembly, and any
		/// loadable assemblies that reference it.</returns>
		public static List<Assembly> GetAssociatedAssemblies(Assembly source)
		{
			var result = new List<Assembly> { source };
			var files = Directory.GetFiles(
				Path.GetDirectoryName(Util.ExecutingAssemblyLocation), "*.dll", SearchOption.AllDirectories);
			
			result.AddRange(files
				.Select(Assembly.LoadFrom)
				.Where(asmb => asmb
					.GetReferencedAssemblies()
					.Count(a => a.FullName == source.FullName) != 0));

			return result;
		}

		/// <summary>
		/// Gets a signature string in the format of <c>ClassName.MethodName</c> for the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method">The method whose signature to get</param>
		/// <returns>A signature string in the format of <c>ClassName.MethodName</c> for the specified <paramref name="method"/>.</returns>
		public static string GetMethodSignature(MethodInfo method)
		{
			return string.Concat(method.DeclaringType.FullName, ".", method.Name);
		}

		/// <summary>
		/// Tests the specified <paramref name="value"/> against <paramref name="validExpression"/> and 
		/// returns it, if successful - or returns <paramref name="defaultValue"/> if validation was not
		/// successful.
		/// </summary>
		/// <param name="value">The value to test</param>
		/// <param name="defaultValue">The value to return if the test fails to match</param>
		/// <param name="validExpression">The regular expression text to test against</param>
		/// <param name="ignoreCase">If <c>true</c>, the test will be done case-insensitive.</param>
		/// <returns>Either <paramref name="value"/>, if evaluating <paramref name="validExpression"/> was
		/// successful, or <paramref name="defaultValue"/> if it was not.</returns>
		public static string GetParameterValue(string value, string defaultValue, string validExpression = null, bool ignoreCase = false)
		{
			if (string.IsNullOrWhiteSpace(value))
				return defaultValue ?? value;

			if (string.IsNullOrWhiteSpace(validExpression))
				return value;

			RegexOptions options = RegexOptions.None;
			if (ignoreCase)
				options |= RegexOptions.IgnoreCase;

			if (Regex.IsMatch(value, validExpression, options))
				return value;

			return defaultValue;
		}

		/// <summary>
		/// Splits the specified <paramref name="argumentString"/>using the specified <paramref name="separator"/>,
		/// but taking into account possible quotation marks around individual arguments.
		/// </summary>
		/// <param name="separator">The separator string.</param>
		/// <param name="argumentString">The argument string to process.</param>
		/// <returns>An array or individual arguments, parsed from the specified <paramref name="argumentString"/>.</returns>
		public static IEnumerable<string> SplitArguments(char separator, string argumentString)
		{
			List<string> result = new List<string>();
			StringBuilder argument = new StringBuilder();

			const char Zero = (char) 0;
			char quote = Zero;
			bool endQuote = false;

			for (int i = 0; i <= argumentString.Length; i++)
			{
				char prev = i == 0 ? Zero : argumentString[i - 1];
				char curr = i == argumentString.Length ? Zero : argumentString[i];
				char next = i >= argumentString.Length - 1 ? Zero : argumentString[i + 1];

				// skip over spaces when we are not in a string
				if (Space.IndexOf(curr) != -1 && quote == Zero)
					continue;

				// we reached the end, or a separator not in a string; save the argument
				if (curr == Zero || (curr == separator && (endQuote || quote == Zero)))
				{
					string value = argument.ToString();
					if (endQuote)
						value = value.Substring(1, value.Length - 2);

					result.Add(value);
					argument = new StringBuilder();
					quote = Zero;
					endQuote = false;

					if (i == argumentString.Length)
						break;
				}
				else
				{
					// starting a new argument
					if (argument.Length == 0)
					{
						// beginning quotation
						if (curr == Quotation || curr == Apostrophe)
							quote = curr;
						else
						{
							quote = Zero;
							endQuote = false;
						}
					}
					else if (curr == quote && prev != Escape)
					{
						endQuote = true;
					}

					bool isEscape = false;
					if (curr == Escape)
					{
						isEscape = true;
						int j = i;
						while (j > 0 && argumentString[--j] == Escape)
							isEscape = !isEscape;
					}

					if (!(isEscape && (next == Quotation || next == Apostrophe)))
						argument.Append(curr);
				}
			}

			return result;
		}
	}
}
