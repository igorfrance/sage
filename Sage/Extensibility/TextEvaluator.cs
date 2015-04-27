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
namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;

	using Kelp;
	using log4net;

	/// <summary>
	/// Implements a text processing system using statically and dynamically registered functions and variables.
	/// </summary>
	public class TextEvaluator
	{
		//// TODO: Introduce strict namespaces on functions and variables
		//// TODO: Support registration of variables with just the namespace, and no name (ns:*)
		private const string Undefined = "undefined";

		private static readonly ILog log = LogManager.GetLogger(typeof(TextEvaluator).FullName);
		private static readonly Dictionary<string, TextFunction> functions = new Dictionary<string, TextFunction>();
		private static readonly Dictionary<string, TextVariable> variables = new Dictionary<string, TextVariable>();

		private static readonly Regex dynamicExpression = new Regex(@"(\\)?\$({([^{}]+)})", RegexOptions.Compiled);
		private static readonly Regex valueExpression = new Regex(@"@@V(?'index'\d+)\b", RegexOptions.Compiled);
		private static readonly Regex variableExpression = new Regex(@"\bvar:(?'name'[\w\.$\-]+)", RegexOptions.Compiled);
		private static readonly Regex functionExpression = new Regex(@"(?'name'[:\w\.$]+)\((?'arguments'[^\(\)]*)\)", RegexOptions.Compiled);

		private readonly SageContext context;

		static TextEvaluator()
		{
			TextEvaluator.DiscoverFunctionsAndVariables();
			Project.AssembliesUpdated += TextEvaluator.OnAssembliesUpdated;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextEvaluator" /> class, using the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The context to use with this instance.</param>
		public TextEvaluator(SageContext context)
		{
			this.context = context;
		}

		/// <summary>
		/// Gets the names of registered functions.
		/// </summary>
		public static ICollection<string> Functions
		{
			get
			{
				return functions.Keys;
			}
		}

		/// <summary>
		/// Gets the names of registered variables.
		/// </summary>
		public static ICollection<string> Variables
		{
			get
			{
				return variables.Keys;
			}
		}

		/// <summary>
		/// Gets the entry with the specified <paramref name="index"/> from the specified <paramref name="parameters"/> list.
		/// </summary>
		/// <param name="parameters">The list parameters.</param>
		/// <param name="index">The index.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The entry with the specified <paramref name="index"/> from the specified <paramref name="parameters"/> list,
		/// or the <paramref name="defaultValue"/> if index is outside of parameters bounds.</returns>
		public static string ExtractParameter(IEnumerable<string> parameters, int index, string defaultValue = null)
		{
			var enumerable = parameters as string[] ?? parameters.ToArray();
			if (parameters == null || index < 0 || index > enumerable.Count() - 1)
				return defaultValue;

			return enumerable.ElementAt(index);
		}

		/// <summary>
		/// Registers a variable <paramref name="handler"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of the variable.</param>
		/// <param name="handler">The function that provides the variable value.</param>
		public static void RegisterVariable(string name, TextVariable handler)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
			Contract.Requires<ArgumentNullException>(handler != null);

			string variableName = name.ToLower();
			if (variables.ContainsKey(variableName))
			{
				if (variables[variableName] == handler)
					return;

				log.WarnFormat("Replacing existing handler '{0}' for variable '{1}' with new handler '{2}'",
					Util.GetMethodSignature(variables[variableName].Method),
					variableName,
					Util.GetMethodSignature(handler.Method));
			}

			variables[variableName] = handler;
		}

		/// <summary>
		/// Registers a function <paramref name="handler"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of the function.</param>
		/// <param name="handler">The function that provides the result value.</param>
		public static void RegisterFunction(string name, TextFunction handler)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
			Contract.Requires<ArgumentNullException>(handler != null);

			string functionName = name.ToLower();
			if (functions.ContainsKey(functionName))
			{
				if (functions[functionName] == handler)
					return;

				log.WarnFormat("Replacing existing handler '{0}' for function '{1}' with new handler '{2}'",
					Util.GetMethodSignature(functions[functionName].Method),
					functionName,
					Util.GetMethodSignature(handler.Method));
			}

			functions[functionName] = handler;
		}

		/// <summary>
		/// Invokes the function with the specified <paramref name="name"/> using the specified
		/// <paramref name="context"/> and <paramref name="arguments"/>.
		/// </summary>
		/// <param name="context">The context to use when invoking the function.</param>
		/// <param name="name">The name of the function to invoke.</param>
		/// <param name="arguments">The arguments to use when invoking the function.</param>
		/// <returns>The result of function invocation.</returns>
		public static string InvokeFunction(SageContext context, string name, string arguments)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));

			if (!functions.ContainsKey(name))
				throw new ArgumentException(string.Format("The function with name '{0}' is undefined.", name), "name");

			string[] args = Util.SplitArguments(',', arguments).ToArray();
			return functions[name](context, args);
		}

		/// <summary>
		/// Invokes the variable with the specified <paramref name="name"/> using the specified
		/// <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The context to use when invoking the variable.</param>
		/// <param name="name">The name of the variable to invoke.</param>
		/// <returns>The result of variable invocation.</returns>
		public static string InvokeVariable(SageContext context, string name)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));

			if (!variables.ContainsKey(name))
				throw new ArgumentException(string.Format("The variable with name '{0}' is undefined.", name), "name");

			return variables[name](context, name);
		}

		/// <summary>
		/// Processes any dynamic expressions embedded in the specified <paramref name="value"/>, using the
		/// specified <paramref name="context"/>.
		/// </summary>
		/// <param name="value">The value to process.</param>
		/// <param name="context">The context to use.</param>
		/// <returns>Processed version of the specified <paramref name="value"/>.</returns>
		public static string Process(string value, SageContext context)
		{
			Match match;

			const string Marker = "^^|_o_|^^";

			while ((match = dynamicExpression.Match(value)).Success)
			{
				string escape = match.Groups[1].Value;
				string wrapped = match.Groups[2].Value;
				string content = match.Groups[3].Value;
				string replacement;

				if (escape != string.Empty)
				{
					replacement = Marker + wrapped;
				}
				else if (variables.ContainsKey(content))
				{
					replacement = TextEvaluator.InvokeVariable(context, content);
				}
				else
				{
					replacement = TextEvaluator.ProcessChunk(context, content);
				}

				value = value.Replace(match.Groups[0].Value, replacement);
			}

			return value.Replace(Marker, "$");
		}

		/// <summary>
		/// Processes any dynamic expressions embedded in the specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to process.</param>
		/// <returns>Processed version of the specified <paramref name="value"/>.</returns>
		public string Process(string value)
		{
			return TextEvaluator.Process(value, context);
		}

		private static string ProcessChunk(SageContext context, string value)
		{
			List<string> values = new List<string>();

			// process all embedded variables, save their values in results and substitute them with placeholders
			string result = variableExpression.Replace(value, delegate(Match varMatch)
			{
				string varName = varMatch.Groups["name"].Value;
				string varValue = variables.ContainsKey(varName)
					? TextEvaluator.InvokeVariable(context, varName)
					: Undefined;

				values.Add(varValue);
				return string.Format("@@V{0}", values.Count - 1);
			});

			// recursively process functions that don't contain any brackets, until there are no more of them left
			Match functMatch, valuesMatch;
			while ((functMatch = functionExpression.Match(result)).Success)
			{
				string functionName = functMatch.Groups["name"].Value;
				string functionArguments = functMatch.Groups["arguments"].Value;

				string functionValue = Undefined;
				if (functions.ContainsKey(functionName))
				{
					while ((valuesMatch = valueExpression.Match(functionArguments)).Success)
					{
						int index = int.Parse(valuesMatch.Groups["index"].Value);
						string resultValue = values.Count > index ? values[index] : Undefined;
						functionArguments = functionArguments.Replace(valuesMatch.Groups[0].Value, resultValue);
					}

					functionValue = TextEvaluator.InvokeFunction(context, functionName, functionArguments);
				}

				values.Add(functionValue);
				result = result.Replace(functMatch.Groups[0].Value, string.Format("@@V{0}", values.Count - 1));
			}

			// convert any remaining placeholders into result values
			while ((valuesMatch = valueExpression.Match(result)).Success)
			{
				int index = int.Parse(valuesMatch.Groups["index"].Value);
				string resultValue = values.Count > index ? values[index] : Undefined;
				result = result.Replace(valuesMatch.Groups[0].Value, resultValue);
			}

			return result;
		}

		private static void DiscoverFunctionsAndVariables()
		{
			const BindingFlags BindingFlags =
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

			foreach (Assembly a in Sage.Project.RelevantAssemblies.ToList())
			{
				var types = from t in a.GetTypes()
							where t.IsClass && !t.IsAbstract
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags))
					{
						foreach (TextFunctionAttribute attrib in methodInfo.GetCustomAttributes(typeof(TextFunctionAttribute), false))
						{
							TextFunction functionHandler = (TextFunction) Delegate.CreateDelegate(typeof(TextFunction), methodInfo);
							TextEvaluator.RegisterFunction(attrib.Name, functionHandler);
						}

						foreach (TextVariableAttribute attrib in methodInfo.GetCustomAttributes(typeof(TextVariableAttribute), false))
						{
							TextVariable variableHandler = (TextVariable) Delegate.CreateDelegate(typeof(TextVariable), methodInfo);
							foreach (string name in attrib.Variables)
							{
								TextEvaluator.RegisterVariable(name, variableHandler);
							}
						}
					}
				}
			}
		}

		private static void OnAssembliesUpdated(object sender, EventArgs arg)
		{
			TextEvaluator.DiscoverFunctionsAndVariables();
		}
	}
}
