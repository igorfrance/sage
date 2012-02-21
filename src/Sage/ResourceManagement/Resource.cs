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
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Xml;

	using Kelp.Extensions;

	/// <summary>
	/// Represents a file resource for use with Sage.
	/// </summary>
	public class Resource
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Resource"/> class, using the specified <paramref name="configElement"/>.
		/// </summary>
		/// <param name="configElement">The configuration element that defines this resource.</param>
		public Resource(XmlElement configElement)
		{
			Contract.Requires<ArgumentNullException>(configElement != null);

			this.Path = configElement.GetAttribute("path");
			this.Type = (ResourceType) Enum.Parse(typeof(ResourceType), configElement.GetAttribute("type"), true);
			this.Location = (ResourceLocation) Enum.Parse(typeof(ResourceLocation), configElement.GetAttribute("location"), true);
			this.LimitTo = new List<string>();

			string limitTo = configElement.GetAttribute("limitTo");
			if (!string.IsNullOrWhiteSpace(limitTo))
			{
				this.LimitTo.AddRange(limitTo.Split(',').Select(s => s.Trim().ToLower()));
			}
		}

		/// <summary>
		/// Gets or sets the type of this resource.
		/// </summary>
		public ResourceType Type { get; protected set; }

		/// <summary>
		/// Gets or sets the location of this resource.
		/// </summary>
		public ResourceLocation Location { get; protected set; }

		/// <summary>
		/// Gets or sets the path of this resource.
		/// </summary>
		public string Path { get; protected set; }

		/// <summary>
		/// Gets or sets the list of user agent id's that this resource should be limited to.
		/// </summary>
		public List<string> LimitTo { get; protected set; }

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The value to the left of the operator.</param>
		/// <param name="right">The value to the right of the operator.</param>
		/// <returns>
		/// <c>true</c> if the two values are equal; otherwise <c>false</c>.
		/// </returns>
		public static bool operator ==(Resource left, Resource right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The value to the left of the operator.</param>
		/// <param name="right">The value to the right of the operator.</param>
		/// <returns>
		/// <c>true</c> if the two values are NOT equal; otherwise <c>false</c>.
		/// </returns>
		public static bool operator !=(Resource left, Resource right)
		{
			return !Equals(left, right);
		}

		/// <summary>
		/// Determines whether this resource is valid for the specified user agent ID.
		/// </summary>
		/// <param name="userAgentID">The user agent ID.</param>
		/// <returns>
		/// <c>true</c> if this resource is valid for the specified user agent ID; otherwise, <c>false</c>.
		/// </returns>
		public bool IsValidFor(string userAgentID)
		{
			if (string.IsNullOrWhiteSpace(userAgentID))
				return true;

			if (this.LimitTo.Count == 0)
				return true;

			return this.LimitTo.Count(userAgentID.StartsWith) != 0;
		}

		/// <summary>
		/// Gets the resolved web-accessible path of this resource.
		/// </summary>
		/// <param name="context">The context under which this method is executed.</param>
		/// <returns>
		/// The resolved web-accessible path of this resource
		/// </returns>
		public virtual string GetResolvedWebPath(SageContext context)
		{
			string physicalPath = this.GetResolvedPhysicalPath(context);
			return context.Path.GetRelativeWebPath(physicalPath, true);
		}

		/// <summary>
		/// Gets the resolved physical path of this resource.
		/// </summary>
		/// <param name="context">The context under which this method is executed.</param>
		/// <returns>
		/// The resolved physical path of this resource
		/// </returns>
		public virtual string GetResolvedPhysicalPath(SageContext context)
		{
			return context.Path.Resolve(this.Path);
		}

		/// <summary>
		/// Creates an XML element that represent this resource.
		/// </summary>
		/// <param name="ownerDocument">The owner document to use to create the element.</param>
		/// <param name="context">The context under which this method is executed.</param>
		/// <returns>
		/// An XML element that represent this resource.
		/// </returns>
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
			else if (this.Type == ResourceType.Icon)
			{
				result = ownerDocument.CreateElement("xhtml:link", XmlNamespaces.XHtmlNamespace);
				result.SetAttribute("rel", "icon");
				result.SetAttribute("href", webPath);
			}
			else
			{
				string documentPath = context.Path.Resolve(this.Path);
				XmlDocument document = context.Resources.LoadXml(documentPath);

				result = (XmlElement) ownerDocument.ImportNode(document.DocumentElement, true);
			}

			return result;
		}

		/// <summary>
		/// Compares this resource to another resource.
		/// </summary>
		/// <param name="other">The resource to compare this resource with.</param>
		/// <returns>
		/// <c>true</c> if the two values are equal; otherwise <c>false</c>.
		/// </returns>
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

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return Equals(obj as Resource);
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2})", this.Path, this.Type, this.Location);
		}
	}
}
