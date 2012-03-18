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

	internal abstract class FileTypeConfiguration
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FileTypeConfiguration).FullName);
		private const BindingFlags Flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;

		public virtual bool Enabled { get; protected set; }

		protected abstract List<string> ByteProps { get; }

		protected abstract List<string> BoolProps { get; }

		protected abstract List<string> EnumProps { get; }

		protected void Parse(XmlElement configurationElement, Type self, object target)
		{
			if (configurationElement != null)
			{
				this.Enabled = configurationElement.GetAttribute("Enabled") == "true";
				foreach (XmlAttribute attrib in configurationElement.Attributes)
				{
					string attribName = attrib.LocalName;
					string attribValue = attrib.InnerText;
					PropertyInfo property = self.GetProperty(attribName, Flags);
					if (property == null || !property.CanWrite)
						continue;

					byte byteValue;
					if (ByteProps.Contains(attribName, StringComparer.OrdinalIgnoreCase) && byte.TryParse(attribValue, out byteValue))
					{
						property.SetValue(target, byteValue, null);
					}

					bool boolValue;
					if (BoolProps.Contains(attribName, StringComparer.OrdinalIgnoreCase) && bool.TryParse(attribValue, out boolValue))
					{
						property.SetValue(target, boolValue, null);
					}

					if (EnumProps.Contains(attribName, StringComparer.OrdinalIgnoreCase))
					{
						try
						{
							property.SetValue(target, System.Enum.Parse(property.PropertyType, attribValue), null);
						}
						catch (Exception ex)
						{
							log.ErrorFormat("Could not set the enum value '{0}' of property '{1} ({2}): {3}'",
								attribValue, attribName, property.PropertyType.Name, ex.Message);
						}
					}
				}
			}
		}

		protected string Serialize(Type t, object sourceObject)
		{
			List<string> result = new List<string> { "Enabled=" + this.Enabled.ToString() };
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
