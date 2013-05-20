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

		internal string TemporaryDirectory { get; private set; }

		internal ScriptFileConfiguration Script { get; private set; }

		internal CssFileConfiguration Css { get; private set; }
	}
}
