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
	using System.Xml;

	using Microsoft.Ajax.Utilities;

	/// <summary>
	/// Represents the processing configuration for css files.
	/// </summary>
	public class CssFileConfiguration : FileTypeConfiguration
	{
		private readonly List<string> byteProps = new List<string> { "IndentSize" };
		private readonly List<string> boolProps = new List<string>
		{
			"MinifyExpressions", "TermSemicolons"
		};

		private readonly List<string> enumProps = new List<string>
		{
			"ColorNames", "CommentMode"
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="CssFileConfiguration" /> class.
		/// </summary>
		public CssFileConfiguration()
		{
			this.Settings = new CssSettings
			{
				MinifyExpressions = true
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CssFileConfiguration" /> class,
		/// using the specified <paramref name="configurationElement"/>
		/// </summary>
		/// <param name="configurationElement">The configuration element.</param>
		public CssFileConfiguration(XmlElement configurationElement)
			: this()
		{
			this.Parse(configurationElement, typeof(CssSettings), this.Settings);
		}

		/// <summary>
		/// Gets the <see cref="CssSettings"/> associated with this instance.
		/// </summary>
		public CssSettings Settings { get; private set; }

		/// <inheritdoc/>
		protected override List<string> BoolProps
		{
			get { return boolProps; }
		}

		/// <inheritdoc/>
		protected override List<string> ByteProps
		{
			get { return byteProps; }
		}

		/// <inheritdoc/>
		protected override List<string> EnumProps
		{
			get { return enumProps; }
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Serialize(typeof(CssSettings), this.Settings);
		}
	}
}
