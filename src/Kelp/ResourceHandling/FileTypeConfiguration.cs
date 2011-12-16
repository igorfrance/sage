namespace Kelp.ResourceHandling
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using log4net;

	using Microsoft.Ajax.Utilities;

	internal abstract class FileTypeConfiguration
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FileTypeConfiguration).FullName);
		private const BindingFlags Flags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;

		public bool Enabled { get; protected set; }

		protected abstract List<string> ByteProps { get; }

		protected abstract List<string> BoolProps { get; }

		protected abstract List<string> EnumProps { get; }

		protected void Parse(XmlElement configurationElement, Type self, object target)
		{
			if (configurationElement != null)
			{
				foreach (XmlAttribute attrib in configurationElement.Attributes)
				{
					string attribName = attrib.LocalName;
					string attribValue = attrib.InnerText;
					PropertyInfo property = self.GetProperty(attribName, Flags);
					if (property == null || !property.CanWrite)
						continue;

					byte byteValue;
					if (ByteProps.Contains(attribName, StringComparer.OrdinalIgnoreCase) && Byte.TryParse(attribValue, out byteValue))
					{
						property.SetValue(target, byteValue, null);
					}

					bool boolValue;
					if (BoolProps.Contains(attribName, StringComparer.OrdinalIgnoreCase) && Boolean.TryParse(attribValue, out boolValue))
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
