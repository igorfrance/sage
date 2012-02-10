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
	using System.Text.RegularExpressions;

	using Sage.Extensibility;

	/// <summary>
	/// Implements a resolver that can be used with embedded resources.
	/// </summary>
	[UrlResolver(Scheme = EmbeddedResourceResolver.Scheme)]
	internal class EmbeddedResourceResolver : ISageXmlUrlResolver
	{
		public const string Scheme = "sageresx";

		public EntityResult GetEntity(UrlResolver parent, SageContext context, string uri)
		{
			Stream reader = GetStreamResourceReader(parent, context, uri);
			if (reader == null)
				throw new ArgumentException(string.Format("The uri '{0}' could not be resolved", uri));

			return new EntityResult { Entity = reader };
		}

		protected Stream GetStreamResourceReader(UrlResolver parent, SageContext context, string resourceUri)
		{
			string resourceName = ConvertPath(resourceUri);

			Stream result = null;
			foreach (Assembly a in Application.RelevantAssemblies)
			{
				string resourcePath = new List<string>(a.GetManifestResourceNames())
					.FirstOrDefault(r => r.ToLower().IndexOf(resourceName) != -1);

				if (!string.IsNullOrEmpty(resourcePath))
				{
					result = a.GetManifestResourceStream(resourcePath);
					break;
				}
			}

			return result;
		}

		private string ConvertPath(string path)
		{
			return Regex.Replace(path, @"^\w+://", string.Empty)
				.TrimEnd('/')
				.Replace(Scheme, string.Empty)
				.Replace("://", string.Empty)
				.Replace('/', '.')
				.Replace('\\', '.')
				.ToLower();
		}
	}
}
