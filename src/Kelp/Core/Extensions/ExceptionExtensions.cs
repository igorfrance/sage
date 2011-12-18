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
