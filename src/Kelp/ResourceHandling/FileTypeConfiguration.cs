﻿/**
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
