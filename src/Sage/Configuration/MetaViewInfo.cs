namespace Sage.Configuration
{
	using System;
	using System.Xml;

	using Sage.Controllers;
	using Sage.ResourceManagement;
	using Sage.Views;

	/// <summary>
	/// Contains information about a meta view.
	/// </summary>
	public class MetaViewInfo
	{
		public MetaViewInfo(XmlElement infoElement)
		{
			if (infoElement == null)
				throw new ArgumentNullException("infoElement");

			this.Name = infoElement.GetAttribute("name");
			this.ViewPath = infoElement.GetAttribute("path");

			if (!string.IsNullOrWhiteSpace(infoElement.GetAttribute("contentType")))
				this.ContentType = infoElement.GetAttribute("contentType");
			else
				this.ContentType = "text/html";

			this.TypeName = infoElement.GetAttribute("type");
		}

		/// <summary>
		/// Gets the name of the meta view.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the path to the meta view file corresponding to this object.
		/// </summary>
		public string ViewPath { get; private set; }

		/// <summary>
		/// Gets or sets the content type associated with the meta view this object represents.
		/// </summary>
		public string ContentType { get; private set; }

		/// <summary>
		/// Gets the name of the type that implements this meta view.
		/// </summary>
		public string TypeName { get; private set; }

		/// <summary>
		/// Gets the XSLT transform associated with the meta view this object represents.
		/// </summary>
		public CacheableXslTransform Transform
		{
			get;
			private set;
		}

		internal bool IsLoaded
		{
			get;
			private set;
		}

		internal void Load(SageContext context)
		{
			if (!string.IsNullOrWhiteSpace(this.ViewPath))
				this.Transform = context.Resources.LoadXsl(context.MapPath(this.ViewPath));

			this.IsLoaded = true;
		}
	}
}