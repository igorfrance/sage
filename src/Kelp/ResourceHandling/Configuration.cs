namespace Kelp.ResourceHandling
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Xml;

	using Microsoft.Ajax.Utilities;

	/// <summary>
	/// Contains the per-file-extension configuration settings for Kelp.
	/// </summary>
	internal class Configuration
	{
		private static readonly object sync = new object();
		private static volatile Configuration current;

		internal Configuration()
		{
			this.TemporaryDirectory = "~/tmp";
			this.Script = new ScriptFileConfiguration();
			this.Css = new CssFileConfiguration();
		}

		internal Configuration(XmlElement configElement)
		{
			this.TemporaryDirectory = configElement.GetAttribute("tempDir");
			this.Script = new ScriptFileConfiguration(configElement.SelectSingleNode("script") as XmlElement);
			this.Css = new CssFileConfiguration(configElement.SelectSingleNode("css") as XmlElement);
		}

		/// <summary>
		/// Gets the current <see cref="Configuration"/> instance.
		/// </summary>
		internal static Configuration Current
		{
			get
			{
				if (current == null)
				{
					lock (sync)
					{
						if (current == null)
						{
							current = (Configuration)
								ConfigurationManager.GetSection(ConfigurationHandler.SectionName);
						}
					}
				}

				return current ?? (current = new Configuration());
			}
		}

		internal string TemporaryDirectory
		{
			get;
			private set;
		}

		internal ScriptFileConfiguration Script { get; private set; }

		internal CssFileConfiguration Css { get; private set; }
	}
}
