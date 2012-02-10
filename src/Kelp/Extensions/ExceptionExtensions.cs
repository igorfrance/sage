/**
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
namespace Kelp.Core.Extensions
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
		public static string InnermostExceptionMessage(this Exception instance)
		{
			Exception inner = GetInnermostException(instance);
			return inner.Message;
		}

		/// <summary>
		/// Gets the type name of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.
		/// </summary>
		/// <param name="instance">The <see cref="Exception"/> instance.</param>
		/// <returns>The type name of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.</returns>
		public static string InnermostExceptionTypeName(this Exception instance)
		{
			Exception inner = GetInnermostException(instance);
			return inner.GetType().Name;
		}

		/// <summary>
		/// Gets the stack trace name of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.
		/// </summary>
		/// <param name="instance">The <see cref="Exception"/> instance.</param>
		/// <returns>The stack trace of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.</returns>
		public static string InnermostExceptionStackTrace(this Exception instance)
		{
			Exception inner = GetInnermostException(instance);
			return inner.StackTrace;
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

		private static Exception GetInnermostException(Exception instance)
		{
			Exception result = instance;
			while (result.InnerException != null)
				result = result.InnerException;

			return result;
		}
	}
}
