namespace Sage.Extensibility
{
	using System;
	using System.IO;
	using System.Xml;

	using Kelp.Extensions;
	using log4net;

	/// <summary>
	/// Provides IO functions that can be used within Sage markup.
	/// </summary>
	/// TODO: Review the usability and necessity of this group of extensions
	public class IOFunctions
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(IOFunctions).FullName);

		[ContextFunction(Name = "io:read")]
		internal static string ReadFile(SageContext context, params string[] arguments)
		{
			if (arguments.Length == 0)
			{
				log.ErrorFormat("Invalid call to read file; file path argument missing.");
				return string.Empty;
			}

			var filePath = context.MapPath(context.Path.Expand(arguments[0]));
			var convert = Convert.None;
			var pretty = false;

			if (arguments.Length >= 2)
			{
				if (arguments[1] == "xml-json")
					convert = Convert.XmlToJson;
				else
					log.ErrorFormat("The convert argument value '{0}' is not valid. Valid values are '{1}'.", arguments[1], "xml-json");
			}

			if (arguments.Length >= 3)
			{
				pretty = arguments[2].EqualsAnyOf("pretty", "true", "yes", "1");
			}

			if (!File.Exists(filePath))
			{
				log.WarnFormat("File '{0}' (mapped to '{1}') doesn't exist.", arguments[1], filePath);
				return string.Empty;
			}

			var content = File.ReadAllText(filePath);
			if (convert == Convert.XmlToJson)
			{
				var document = new XmlDocument();
				document.LoadXml(content);
				content = document.DocumentElement.ToJson(pretty);
			}

			return content;
		}

		internal enum Convert
		{
			None = 0,
			XmlToJson = 1,
		}
	}
}
