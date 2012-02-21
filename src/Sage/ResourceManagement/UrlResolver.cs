/**
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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using log4net;
	using Sage.Extensibility;

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
			foreach (Assembly a in Application.RelevantAssemblies)
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

		/// <summary>
		/// Initializes a new instance of the <see cref="UrlResolver"/> class.
		/// </summary>
		public UrlResolver()
		{
			this.instances = new Dictionary<string, ISageXmlUrlResolver>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UrlResolver"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public UrlResolver(SageContext context)
			: this()
		{
			this.context = context;
		}

		/// <summary>
		/// Gets the file dependencies discovered by the resolvers associates with this instance.
		/// </summary>
		public List<string> Dependencies
		{
			get
			{
				return dependencies;
			}
		}

		/// <summary>
		/// Gets the scheme name for the specified <paramref name="uriString"/>.
		/// </summary>
		/// <param name="uriString">The uri for which to get the scheme.</param>
		/// <returns>The scheme name for the specified <paramref name="uriString"/>.</returns>
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

		/// <summary>
		/// Registers the resolver.
		/// </summary>
		/// <param name="scheme">The scheme.</param>
		/// <param name="resolver">The resolver.</param>
		public void RegisterResolver(string scheme, ISageXmlUrlResolver resolver)
		{
			if (instanceResolvers.ContainsKey(scheme))
			{
				log.WarnFormat("Overwriting existing resolver {0} for scheme '{1}' with resolver {2}",
					instanceResolvers[scheme].GetType().FullName, scheme, resolver.GetType().FullName);
			}

			instanceResolvers[scheme] = resolver;
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
