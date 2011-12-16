namespace Kelp.ResourceHandling
{
	using System;
	using System.Configuration;
	using System.Xml;

	internal class ConfigurationHandler : IConfigurationSectionHandler
	{
		public const string SectionName = "resourceHandling";

		public object Create(object parent, object configContext, XmlNode section)
		{
			return new Configuration(section as XmlElement);
		}
	}	
}
