namespace Sage.Configuration
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements the configuration container for configurable properties of a white label category.
	/// </summary>
	public class CategoryConfiguration
	{
		private const string ConfigSchemaPath = "sageresx://sage/resources/schemas/CategoryConfiguration.xsd";

		private CategoryConfiguration(XmlDocument configDocument)
		{
			XmlNamespaceManager nm = XmlNamespaces.Manager;
			XmlElement categoryElement = configDocument.SelectSingleElement("c:configuration/c:category", nm);

			this.Name = categoryElement.GetAttribute("name");
			this.VariablesElement = categoryElement.SelectSingleElement("c:variables", nm);
			this.ConfigurationElement = categoryElement;
		}

		/// <summary>
		/// Gets the name of this category.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; private set; }

		public XmlElement ConfigurationElement { get; private set; }

		/// <summary>
		/// Gets the xml element containing the localization variables for use in internationalization.
		/// </summary>
		public XmlElement VariablesElement { get; private set; }

		public static CategoryConfiguration Create(string configurationPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(configurationPath));

			CacheableXmlDocument schemaDocument = ResourceManager.LoadXmlDocument(configurationPath, null, ConfigSchemaPath);
			return new CategoryConfiguration(schemaDocument);
		}
	}
}