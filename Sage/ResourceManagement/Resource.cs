﻿/**
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
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Xml;

	using Kelp;
	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Represents a file resource for use with Sage.
	/// </summary>
	public class Resource : IXmlConvertible
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Resource"/> class, using the specified <paramref name="configElement"/>.
		/// </summary>
		/// <param name="configElement">The configuration element that defines this resource.</param>
		/// <param name="projectId">The identification string of the project this library belongs to.</param>
		public Resource(XmlElement configElement, string projectId)
		{
			Contract.Requires<ArgumentNullException>(configElement != null);

			this.ProjectId = projectId;
			this.Parse(configElement);
		}

		/// <summary>
		/// Gets or sets the list of user agent id's that this resource should be limited to.
		/// </summary>
		public List<string> LimitTo { get; protected set; }

		/// <summary>
		/// Gets or sets the location of this resource.
		/// </summary>
		public ResourceLocation Location { get; protected set; }

		/// <summary>
		/// Gets or sets the optional name of this resource.
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// Gets or sets the path of this resource.
		/// </summary>
		public string Path { get; protected set; }

		/// <summary>
		/// Gets the identification string of the project this library belongs to.
		/// </summary>
		public string ProjectId { get; protected set; }

		/// <summary>
		/// Gets or sets the type of this resource.
		/// </summary>
		public ResourceType Type { get; protected set; }

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
			return object.Equals(left, right);
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
			return !object.Equals(left, right);
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
				object.Equals(other.Type, this.Type) &&
				object.Equals(other.Location, this.Location) &&
				other.Path.Equals(this.Path, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return this.Equals(obj as Resource);
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

		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			this.Path = element.GetAttribute("path");
			this.Name = element.GetAttribute("name");
			this.Type = (ResourceType) Enum.Parse(typeof(ResourceType), element.GetAttribute("type"), true);
			this.Location = (ResourceLocation) Enum.Parse(typeof(ResourceLocation), element.GetAttribute("location"), true);
			this.LimitTo = new List<string>();

			string limitTo = element.GetAttribute("limitTo");
			if (!string.IsNullOrWhiteSpace(limitTo))
			{
				this.LimitTo.AddRange(limitTo.Split(',').Select(s => s.Trim().ToLower()));
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2})", this.Path, this.Type, this.Location);
		}

		/// <summary>
		/// Generates an <see cref="XmlElement" /> that represents the configuration of this instance.
		/// </summary>
		/// <param name="document">The document to use to create the element with.</param>
		/// <returns>An <see cref="XmlElement" /> that represents the configuration of this instance.</returns>
		public XmlElement ToXml(XmlDocument document)
		{
			XmlElement result = document.CreateElement("resource", XmlNamespaces.ProjectConfigurationNamespace);

			result.SetAttribute("name", this.Name);
			result.SetAttribute("path", this.Path);
			result.SetAttribute("type", this.Type.ToString().ToLower());
			result.SetAttribute("location", this.Location.ToString().ToLower());

			if (this.LimitTo.Count != 0)
			{
				result.SetAttribute("limitTo", string.Join(",", this.LimitTo.ToArray()));
			}

			return result;
		}

		/// <summary>
		/// Generates an <see cref="XmlElement" /> that represent a this resource.
		/// </summary>
		/// <param name="ownerDocument">The owner document to use to create the element.</param>
		/// <param name="context">The context under which this method is executed, used to resolve the paths and load resources with.</param>
		/// <returns>
		/// An <see cref="XmlElement" /> that represent this resource.
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
				if (!string.IsNullOrWhiteSpace(this.Name))
					result.OwnerDocument.DocumentElement.SetAttribute("sage:resourceName", XmlNamespaces.SageNamespace, this.Name);
			}

			return result;
		}
	}
}