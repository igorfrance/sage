/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

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
		private const BindingFlags AttributeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		static SageResourceResolver()
		{
			providers = new Dictionary<string, GetResource>();

			foreach (Assembly a in Application.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
							where
								t.IsClass &&
								t.GetMethods(AttributeFlags).Count(m => m.GetCustomAttributes(typeof(SageResourceProviderAttribute), false).Count() != 0) != 0
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods(AttributeFlags))
					{
						foreach (SageResourceProviderAttribute attrib in methodInfo.GetCustomAttributes(typeof(SageResourceProviderAttribute), false))
						{
							GetResource del = (GetResource) Delegate.CreateDelegate(typeof(GetResource), methodInfo);
							if (providers.ContainsKey(attrib.ResourceName))
							{
								log.WarnFormat("Overwriting existing resource provider '{0}' for resource name '{1}' with provider '{2}'",
									ResourceManager.GetDelegateSignature(providers[attrib.ResourceName].Method),
									attrib.ResourceName,
									ResourceManager.GetDelegateSignature(del.Method));
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
				GetResource provider = providers[resourceName];
				log.DebugFormat("Found a specific resource provider for {0}: {1}",
					resourceUri,
					ResourceManager.GetDelegateSignature(provider.Method));

				resourceDoc = provider(context, resourceUri);
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
