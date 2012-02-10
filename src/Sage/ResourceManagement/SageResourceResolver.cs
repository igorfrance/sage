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
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using Mvp.Xml.XInclude;

	using log4net;
	using Sage.Extensibility;

	/// <summary>
	/// Implements a custom resolver for various resource providers.
	/// </summary>
	/// <remarks>
	/// In order to hook into this resolver and provide custom resources that can be referred to in
	/// url's, create a method that matches <see cref="GetResource"/> delegate and tag it with
	/// <see cref="SageResourceProviderAttribute"/>. The name that you specify in the constructor for
	/// <see cref="SageResourceProviderAttribute"/> is the string that follows <c>sageres://</c>.
	/// </remarks>
	[UrlResolver(Scheme = SageResourceResolver.Scheme)]
	public class SageResourceResolver : ISageXmlUrlResolver
	{
		/// <summary>
		/// The scheme associated with this resolver;
		/// </summary>
		public const string Scheme = "sageres";
		private static readonly ILog log = LogManager.GetLogger(typeof(SageResourceResolver).FullName);
		private static readonly Dictionary<string, GetResource> providers;
		private readonly List<string> dependencies = new List<string>();

		static SageResourceResolver()
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			providers = new Dictionary<string, GetResource>();

			foreach (Assembly a in Application.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
							where
								t.IsClass &&
								t.GetMethods(flags).Count(m => m.GetCustomAttributes(typeof(SageResourceProviderAttribute), false).Count() != 0) != 0
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods(flags))
					{
						foreach (SageResourceProviderAttribute attrib in methodInfo.GetCustomAttributes(typeof(SageResourceProviderAttribute), false))
						{
							GetResource del = (GetResource) Delegate.CreateDelegate(typeof(GetResource), methodInfo);
							if (providers.ContainsKey(attrib.ResourceName))
							{
								log.WarnFormat("Overwriting existing resource provider '{0}' for resource name '{1}' with provider '{2}'",
									ResourceManager.GetDelegateSignature(providers[attrib.ResourceName]),
									attrib.ResourceName,
									ResourceManager.GetDelegateSignature(del));
							}

							providers[attrib.ResourceName] = del;
						}
					}
				}
			}
		}

		/// <inheritdoc/>
		public EntityResult GetEntity(UrlResolver parent, SageContext context, string resourceUri)
		{
			string resourceName = GetResourceName(resourceUri);
				XmlReaderSettings settings = CacheableXmlDocument.CreateReaderSettings(parent);

			CacheableXmlDocument resourceDoc;

			// first check if we have a registered provider for the specified resour name
			if (providers.ContainsKey(resourceName))
			{
				log.DebugFormat("Found a specific resource provider for {0}: {1}.{2}",
					resourceUri,
					providers[resourceName].GetType().Name,
					providers[resourceName].Method.Name);

				resourceDoc = providers[resourceName](context, resourceUri);
			}
			else
			{
				string sourcePath = context.Path.Expand(resourceName);
				if (sourcePath == null || !File.Exists(sourcePath))
					throw new FileNotFoundException(string.Format("The specified resource '{0}' doesn't exist.", resourceUri));

				resourceDoc = context.Resources.LoadXml(sourcePath);
			}

			XmlReader reader = XmlReader.Create(new StringReader(resourceDoc.OuterXml), settings, resourceUri);
			return new EntityResult { Entity = reader, Dependencies = resourceDoc.Dependencies };
		}

		private string GetResourceName(string uri)
		{
			return uri.Replace(GetResourcePrefix(), string.Empty).Trim('/', '\\');
		}

		private string GetResourcePrefix()
		{
			return Scheme + "://";
		}
	}
}
