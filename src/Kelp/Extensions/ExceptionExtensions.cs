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
		/// Saves the current <paramref name="instance"/> as an <see cref="XmlElement"/>, using the specified 
		/// <paramref name="ownerDocument"/> to create the element.
		/// </summary>
		/// <param name="instance">The exception instance.</param>
		/// <param name="ownerDocument">The document to use to create the element.</param>
		/// <returns>An XML representation of the current exception.</returns>
		public static XmlElement ToXml(this Exception instance, XmlDocument ownerDocument)
		{
			XmlElement exceptionElement = ownerDocument.CreateElement("exception");
			exceptionElement.SetAttribute("type", instance.GetType().ToString());
			exceptionElement.SetAttribute("message", instance.Message);
			exceptionElement.SetAttribute("htmlDescription", instance.Message
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
				.Replace("\t", "&#160;&#160;&#160;&#160;")
				.Replace("\n", "<br/>"));

			XmlElement straceNode = (XmlElement) exceptionElement.AppendChild(ownerDocument.CreateElement("stacktrace"));
			string[] stackTrace = instance.StackTrace != null
				? instance.StackTrace.Split(new[] { '\n' })
				: new[] { string.Empty };

			if (instance.GetType() == typeof(XmlException))
			{
				exceptionElement.SetAttribute("sourceuri", ((XmlException) instance).SourceUri);
				exceptionElement.SetAttribute("linenumber", ((XmlException) instance).LineNumber.ToString());
				exceptionElement.SetAttribute("lineposition", ((XmlException) instance).LinePosition.ToString());
			}

			foreach (string t in stackTrace)
			{
				XmlElement frameNode = (XmlElement) straceNode.AppendChild(ownerDocument.CreateElement("frame"));
				Match match;
				if ((match = Regex.Match(t, "^\\s*at (.*) in (.*):line (\\d+)[\\s\\r]*$")).Success)
				{
					frameNode.SetAttribute("text", match.Groups[1].Value);
					frameNode.SetAttribute("file", match.Groups[2].Value);
					frameNode.SetAttribute("line", match.Groups[3].Value);
				}
				else
				{
					frameNode.SetAttribute("text", Regex.Replace(t, "^\\s*at ", string.Empty));
				}
			}

			return exceptionElement;
		}

		/// <summary>
		/// Gets the exception innermost tp the specified <paramref name="instance"/>
		/// </summary>
		/// <param name="instance">The exception that occured.</param>
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
