namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Mvp.Xml.XInclude;

	/// <summary>
	/// Provides an xinclude resolver that allows including project specific resources without specifying the full path.
	/// </summary>
	[UrlResolver(Scheme = XmlIncludeResolver.Scheme)]
	public class XmlIncludeResolver : ISageXmlUrlResolver
	{
		/// <summary>
		/// Defines the scheme that this resolver handles.
		/// </summary>
		public const string Scheme = "sagexi";
		private static readonly Regex cleanupName = new Regex(@"^\w+://(.*)$");
		private readonly List<string> dependencies = new List<string>();

		public EntityResult GetEntity(UrlResolver parent, SageContext context, string uri)
		{
			XmlReader reader = GetXmlResourceReader(parent, context, uri);
			return new EntityResult { Entity = reader, Dependencies = dependencies };
		}

		internal static string GetPhysicalXIncludePath(SageContext context, string path, string locale)
		{
			string resourceName = cleanupName.Replace(path, "$1");
			return context.Path.Expand(resourceName);
		}

		protected XmlReader GetXmlResourceReader(UrlResolver parent, SageContext context, string sageResource)
		{
			string sourcePath = GetPhysicalXIncludePath(context, sageResource, context.Locale);
			if (sourcePath == null || !File.Exists(sourcePath))
				throw new FileNotFoundException(string.Format("The specified include file '{0}' could not be found.", sageResource));

			XmlReaderSettings settings = CacheableXmlDocument.CreateReaderSettings(parent);
			XIncludingReader reader = new XIncludingReader(XmlReader.Create(sourcePath, settings));
			dependencies.Add(sourcePath);

			reader.XmlResolver = parent;
			return reader;
		}
	}
}
