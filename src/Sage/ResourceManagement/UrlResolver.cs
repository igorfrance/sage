namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using Sage.Configuration;

	using log4net;

	/// <summary>
	/// Implements a generic URL resolver for use with XML and XSL documents, and that any custom resolvers
	/// can be registered with.
	/// </summary>
	public class UrlResolver : XmlUrlResolver
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(UrlResolver).FullName);
		private static readonly Dictionary<string, Type> staticResolvers = new Dictionary<string, Type>();
		private readonly Dictionary<string, ISageXmlUrlResolver> instanceResolvers = new Dictionary<string, ISageXmlUrlResolver>();

		/// <summary>
		/// Local reference to the context under which this instance was created.
		/// </summary>
		private readonly SageContext context;
		private readonly Dictionary<string, ISageXmlUrlResolver> instances;

		private readonly List<string> dependencies = new List<string>();
		private readonly List<string> resolved = new List<string>();

		static UrlResolver()
		{
			foreach (Assembly a in ProjectConfiguration.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
								  where t.IsClass && !t.IsAbstract
										&& typeof(ISageXmlUrlResolver).IsAssignableFrom(t)
										&& t.GetCustomAttributes(typeof(UrlResolverAttribute), false).Count() != 0
								  select t;

				foreach (Type type in types)
				{
					UrlResolverAttribute attrib = ((UrlResolverAttribute[]) type.GetCustomAttributes(typeof(UrlResolverAttribute), false))[0];
					if (staticResolvers.ContainsKey(attrib.Scheme))
					{
						log.WarnFormat("Overwriting existing resolver {0} for scheme '{1}' with resolver {2}",
							staticResolvers[attrib.Scheme].FullName, attrib.Scheme, type.FullName);
					}

					staticResolvers[attrib.Scheme] = type;
				}
			}
		}

		public UrlResolver()
		{
			this.instances = new Dictionary<string, ISageXmlUrlResolver>();
		}

		public UrlResolver(SageContext context)
			: this()
		{
			this.context = context;
		}

		/// <summary>
		/// Lists any file dependencies discovered by the resolvers associates with this instance.
		/// </summary>
		public List<string> Dependencies
		{
			get
			{
				return dependencies;
			}
		}

		public void RegisterResolver(string scheme, ISageXmlUrlResolver resolver)
		{
			if (instanceResolvers.ContainsKey(scheme))
			{
				log.WarnFormat("Overwriting existing resolver {0} for scheme '{1}' with resolver {2}",
					instanceResolvers[scheme].GetType().FullName, scheme, resolver.GetType().FullName);
			}

			instanceResolvers[scheme] = resolver;
		}

		public static string GetScheme(string uriString)
		{
			Uri uri;
			if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
			{
				if (uri.IsAbsoluteUri)
					return uri.Scheme;
			}

			return "file";
		}

		/// <inheritdoc/>
		public override object GetEntity(Uri uri, string role, Type returnObject)
		{
			ISageXmlUrlResolver resolver = GetResolver(uri.Scheme);
			if (resolver != null)
			{
				EntityResult result = resolver.GetEntity(this, context, uri.ToString().Replace("\\", "/"));
				dependencies.AddRange(result.Dependencies);
				resolved.Add(uri.ToString());
				return result.Entity;
			}

			try
			{
				dependencies.Add(uri.LocalPath);
				resolved.Add(uri.ToString());
				return base.GetEntity(uri, role, returnObject);
			}
			catch (NotSupportedException ex)
			{
				throw new NotSupportedException(string.Format("Could not resolve uri '{0}'", uri), ex);
			}
		}

		private ISageXmlUrlResolver GetResolver(string scheme)
		{
			if (instanceResolvers.ContainsKey(scheme))
				return instanceResolvers[scheme];

			if (instances.ContainsKey(scheme))
				return instances[scheme];

			if (staticResolvers.ContainsKey(scheme))
			{
				ConstructorInfo constructor = staticResolvers[scheme].GetConstructor(new Type[] { });
				ISageXmlUrlResolver resolver = (ISageXmlUrlResolver) constructor.Invoke(new object[] { });
				instances[scheme] = resolver;
				return resolver;
			}

			return null;
		}
	}
}
