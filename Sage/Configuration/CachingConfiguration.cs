namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Xml;
	using Kelp.Extensions;
	using Sage.ResourceManagement;
	using Sage.Views;

	internal class CachingConfiguration
	{
		public CachingConfiguration()
		{
			this.Enabled = false;
			this.Directory = "~/tmp/cache";
			this.Groups = new Dictionary<string, Group>();
		}

		public void Parse(XmlElement configElement)
		{
			this.Enabled = configElement.GetAttribute("enabled").EqualsAnyOf("yes", "true", "1");
			this.Directory = configElement.GetString("p:directory", XmlNamespaces.Manager);

			var groups = configElement.SelectNodes("p:groups/p:group", XmlNamespaces.Manager);
			foreach (XmlElement node in groups)
			{
				var group = new Group(node);
				this.Groups.Add(group.Name, group);
			}
		}

		public bool Enabled { get; set; }

		public string Directory { get; set; }

		public Dictionary<string, Group> Groups
		{
			get;
			private set;
		}

		public class Group
		{
			public Group(XmlElement groupElement)
			{
				this.LoadFilters = new List<Filter>();
				this.SaveFilters = new List<Filter>();

				this.Name = groupElement.GetAttribute("name");
				this.Extension = groupElement.GetAttribute("extension");

				foreach (XmlElement filter in groupElement.SelectNodes("p:loadfilter/*", XmlNamespaces.Manager))
					this.LoadFilters.Add(Filter.Create(filter));

				foreach (XmlElement filter in groupElement.SelectNodes("p:savefilter/*", XmlNamespaces.Manager))
					this.SaveFilters.Add(Filter.Create(filter));
			}

			public string Name { get; private set; }

			public string Extension { get; private set; }

			public List<Filter> LoadFilters { get; private set; } 

			public List<Filter> SaveFilters { get; private set; } 

			public string ApplyLoadFilters(string content, SageContext context)
			{
				return this.ApplyGroup(content, this.LoadFilters, context);
			}

			public string ApplySaveFilters(string content, SageContext context)
			{
				return this.ApplyGroup(content, this.SaveFilters, context);
			}

			private string ApplyGroup(string content, List<Filter> filters, SageContext context)
			{
				foreach (var group in filters)
					content = group.Apply(content, context);

				return content;
			}
		}

		public abstract class Filter
		{
			public abstract string Apply(string content, SageContext context);

			public static Filter Create(XmlElement filterElement)
			{
				switch (filterElement.LocalName)
				{
					case "transform":
						return new TransformFilter(filterElement);

					case "replace":
						return new ReplaceFilter(filterElement);

					default:
						throw new InvalidOperationException("Unexpected filter name: " + filterElement.LocalName);
				}
			}
		}

		public class ReplaceFilter : Filter
		{
			private readonly Regex expression;
			private readonly string replacement;

			public ReplaceFilter(XmlElement filterElement)
			{
				expression = new Regex(filterElement.GetString("p:from", XmlNamespaces.Manager));
				replacement = filterElement.GetString("p:to", XmlNamespaces.Manager);
			}

			public override string Apply(string content, SageContext context)
			{
				return expression.Replace(content, replacement);
			}
		}

		public class TransformFilter : Filter
		{
			private string path;
			private XsltTransform transform;

			public TransformFilter(XmlElement filterElement)
			{
				path = filterElement.InnerText;
			}

			public override string Apply(string content, SageContext context)
			{
				if (transform == null)
					transform = XsltTransform.Create(context, path);

				var result = new StringWriter();
				var document = new CacheableXmlDocument();
				document.LoadXml(content);
				document.DocumentElement.AppendChild(context.ToXml(document));

				transform.Transform(document, result, context);
				return result.ToString();
			}
		}
	}
}
