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
	using System.Text.RegularExpressions;

	/// <summary>
	/// Contains various utility methods and properties that don't fit anywhere else.
	/// </summary>
	public static class Util
	{
		/// <summary>
		/// Gets the physical path of the currently executing assembly.
		/// </summary>
		public static string ExecutingAssemblyPath
		{
			get
			{
				return Path.GetDirectoryName(
					Assembly.GetExecutingAssembly()
						.CodeBase
						.Replace("file:///", string.Empty)
						.Replace("/", "\\"));
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
			var files = Directory.GetFiles(ExecutingAssemblyPath, "*.dll", SearchOption.AllDirectories);
			
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
	}
}
