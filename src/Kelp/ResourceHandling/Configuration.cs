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
