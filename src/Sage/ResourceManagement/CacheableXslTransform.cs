namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Xml;
	using System.Xml.Xsl;

	/// <summary>
	/// Similar to <see cref="CacheableXmlDocument"/>, this class wraps an <see cref="XslCompiledTransform"/>
	/// and provides a collection of its file dependencies (other XSLT templates that were included).
	/// </summary>
	public class CacheableXslTransform
	{
		private CacheableXmlDocument document;

		public CacheableXslTransform(CacheableXmlDocument document, UrlResolver resolver)
		{
			this.document = document;

			this.Processor = new XslCompiledTransform();
			this.Processor.Load(document, XsltSettings.TrustedXslt, resolver);
			document.AddDependencies(resolver.Dependencies);
		}

		public XslCompiledTransform Processor { get; private set; }

		/// <summary>
		/// Gets the list of files that this document consists of / depends on.
		/// </summary>
		public ReadOnlyCollection<string> Dependencies
		{
			get
			{
				return this.document.Dependencies;
			}
		}

		public DateTime LastModified
		{
			get
			{
				return this.document.LastModified;
			}
		}
	}
}
