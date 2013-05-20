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
namespace Kelp.Extensions
{
	using System;
	using System.Text.RegularExpressions;
	using System.Xml;

	/// <summary>
	/// Provides <see cref="Exception"/> extension methods.
	/// </summary>
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Gets the message of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.
		/// </summary>
		/// <param name="instance">The <see cref="Exception"/> instance.</param>
		/// <returns>The message of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.</returns>
		public static string RootMessage(this Exception instance)
		{
			return Root(instance).Message;
		}

		/// <summary>
		/// Gets the type name of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.
		/// </summary>
		/// <param name="instance">The <see cref="Exception"/> instance.</param>
		/// <returns>The type name of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.</returns>
		public static string RootTypeName(this Exception instance)
		{
			return Root(instance).GetType().Name;
		}

		/// <summary>
		/// Gets the stack trace name of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.
		/// </summary>
		/// <param name="instance">The <see cref="Exception"/> instance.</param>
		/// <returns>The stack trace of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.</returns>
		public static string RootStackTrace(this Exception instance)
		{
			return Root(instance).StackTrace;
		}

		/// <summary>
		/// Gets the exception innermost to the specified <paramref name="instance"/>
		/// </summary>
		/// <param name="instance">The exception that occurred.</param>
		/// <returns>The innermost source of the exception</returns>
		public static Exception Root(this Exception instance)
		{
			Exception result = instance;
			while (result.InnerException != null)
				result = result.InnerException;

			return result;
		}
	}
}
