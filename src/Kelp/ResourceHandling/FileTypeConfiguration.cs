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
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using log4net;

	/// <summary>
	/// Represents the processing configuration for a file type.
	/// </summary>
	public abstract class FileTypeConfiguration
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FileTypeConfiguration).FullName);
		private const BindingFlags Flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;
		private string temporaryDirectory;

		/// <summary>
		/// Gets or sets a value indicating whether minification is enabled for this file type.
		/// </summary>
		/// <value><c>true</c> if minification is enabled; otherwise, <c>false</c>.</value>
		public virtual bool MinificationEnabled { get; set; }

		/// <summary>
		/// Gets or sets the temporary directory in which to save the processed files.
		/// </summary>
		public string TemporaryDirectory
		{
			get
			{
				return temporaryDirectory ?? Configuration.Current.TemporaryDirectory;
			}

			set
			{
				temporaryDirectory = value;
			}
		}

		/// <summary>
		/// Gets configuration options that should be parsed as <see cref="Byte"/> values.
		/// </summary>
		protected abstract List<string> ByteProps { get; }

		/// <summary>
		/// Gets configuration options that should be parsed as <see cref="Boolean"/> values.
		/// </summary>
		protected abstract List<string> BoolProps { get; }

		/// <summary>
		/// Gets configuration options that should be parsed as <see cref="Enum"/> values.
		/// </summary>
		protected abstract List<string> EnumProps { get; }

		/// <summary>
		/// Parses the specified configuration element.
		/// </summary>
		/// <param name="configurationElement">The configuration element to parse.</param>
		/// <param name="self">The (derived) type of the <paramref name="target"/>.</param>
		/// <param name="target">The target instance to parse the configuration into.</param>
		protected void Parse(XmlElement configurationElement, Type self, object target)
		{
			if (configurationElement == null)
				return;

			this.MinificationEnabled = configurationElement.GetAttribute("Enabled") == "true";
			foreach (XmlAttribute attrib in configurationElement.Attributes)
			{
				string attribName = attrib.LocalName;
				string attribValue = attrib.InnerText;
				PropertyInfo property = self.GetProperty(attribName, Flags);
				if (property == null || !property.CanWrite)
					continue;

				byte byteValue;
				if (this.ByteProps.Contains(attribName, StringComparer.OrdinalIgnoreCase) && byte.TryParse(attribValue, out byteValue))
				{
					property.SetValue(target, byteValue, null);
				}

				bool boolValue;
				if (this.BoolProps.Contains(attribName, StringComparer.OrdinalIgnoreCase) && bool.TryParse(attribValue, out boolValue))
				{
					property.SetValue(target, boolValue, null);
				}

				if (this.EnumProps.Contains(attribName, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						property.SetValue(target, Enum.Parse(property.PropertyType, attribValue), null);
					}
					catch (Exception ex)
					{
						log.ErrorFormat("Could not set the enum value '{0}' of property '{1} ({2}): {3}'",
							attribValue, attribName, property.PropertyType.Name, ex.Message);
					}
				}
			}
		}

		/// <summary>
		/// Converts the values of <paramref name="sourceObject"/> to a string.
		/// </summary>
		/// <param name="t">The derived type of the sourceObject.</param>
		/// <param name="sourceObject">The source object to serialize.</param>
		/// <returns>The string representation of the <paramref name="sourceObject"/>.</returns>
		protected string Serialize(Type t, object sourceObject)
		{
			List<string> result = new List<string> { "Enabled=" + this.MinificationEnabled };
			foreach (string prop in BoolProps)
			{
				PropertyInfo property = t.GetProperty(prop, Flags);
				result.Add(prop + "=" + property.GetValue(sourceObject, null));
			}

			foreach (string prop in ByteProps)
			{
				PropertyInfo property = t.GetProperty(prop, Flags);
				result.Add(prop + "=" + property.GetValue(sourceObject, null));
			}

			foreach (string prop in EnumProps)
			{
				PropertyInfo property = t.GetProperty(prop, Flags);
				result.Add(prop + "=" + property.GetValue(sourceObject, null));
			}

			return string.Join("&", result.ToArray());
		}
	}
}
