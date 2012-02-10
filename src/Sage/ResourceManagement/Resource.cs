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
	using System.Diagnostics.Contracts;
	using System.Xml;

	public class Resource
	{
		public Resource(XmlElement configElement)
		{
			Contract.Requires<ArgumentNullException>(configElement != null);

			this.Path = configElement.GetAttribute("path");
			this.Type = (ResourceType) Enum.Parse(typeof(ResourceType), configElement.GetAttribute("type"), true);
			this.Location = (ResourceLocation) Enum.Parse(typeof(ResourceLocation), configElement.GetAttribute("location"), true);
		}

		public ResourceType Type { get; protected set; }

		public ResourceLocation Location { get; protected set; }

		public string Path { get; protected set; }

		public static bool operator ==(Resource left, Resource right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Resource left, Resource right)
		{
			return !Equals(left, right);
		}

		public virtual string GetResolvedWebPath(SageContext context)
		{
			string physicalPath = this.GetResolvedPhysicalPath(context);
			return context.Path.GetRelativeWebPath(physicalPath, true);
		}

		public virtual string GetResolvedPhysicalPath(SageContext context)
		{
			return context.Path.Resolve(this.Path);
		}

		public virtual XmlElement ToXml(XmlDocument ownerDocument, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(ownerDocument != null);
			Contract.Requires<ArgumentNullException>(context != null);

			XmlElement result;
			string webPath = this.GetResolvedWebPath(context);
			if (this.Type == ResourceType.Style)
			{
				result = ownerDocument.CreateElement("xhtml:link", XmlNamespaces.XHtmlNamespace);
				result.SetAttribute("type", "text/css");
				result.SetAttribute("rel", "stylesheet");
				result.SetAttribute("href", webPath);
			}
			else if (this.Type == ResourceType.Script)
			{
				result = ownerDocument.CreateElement("xhtml:script", XmlNamespaces.XHtmlNamespace);
				result.SetAttribute("type", "text/javascript");
				result.SetAttribute("language", "javascript");
				result.SetAttribute("src", webPath);
			}
			else
			{
				string documentPath = context.Path.Resolve(this.Path);
				XmlDocument document = context.Resources.LoadXml(documentPath);

				result = (XmlElement) ownerDocument.ImportNode(document.DocumentElement, true);
			}

			return result;
		}

		public bool Equals(Resource other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return 
				Equals(other.Type, this.Type) && 
				Equals(other.Location, this.Location) &&
				other.Path.Equals(this.Path, StringComparison.InvariantCultureIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Resource);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = this.Type.GetHashCode();
				result = (result * 397) ^ this.Location.GetHashCode();
				result = (result * 397) ^ (this.Path != null ? this.Path.GetHashCode() : 0);
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2})", this.Path, this.Type, this.Location);
		}
	}
}
