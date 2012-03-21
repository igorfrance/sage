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
namespace Kelp.XInclude.XPath
{
	using System.Xml;
	using System.Xml.XPath;

	/// <summary>	
	/// Allows to navigate a subtree of an <see cref="IXPathNavigable"/> source, 
	/// by limiting the scope of the navigator to that received 
	/// at construction time.
	/// </summary>
	public class SubtreeXPathNavigator : XPathNavigator
	{
		private readonly bool fragment;
		private readonly XPathNavigator root;

		private XPathNavigator navigator;

		/// <summary>
		/// Creates SubtreeXPathNavigator over specified XPathNavigator.
		/// </summary>
		/// <param name="navigator">Navigator that determines scope.</param>
		/// <remarks>The incoming navigator is cloned upon construction, 
		/// which isolates the calling code from movements to the 
		/// <see cref="SubtreeXPathNavigator"/>.</remarks>
		public SubtreeXPathNavigator(XPathNavigator navigator)
			: this(navigator, false)
		{
		}

		/// <summary>
		/// Creates SubtreeXPathNavigator over specified XPathNavigator.
		/// </summary>
		/// <param name="navigator">Navigator that determines scope.</param>
		/// <param name="enableFragment">Whether the navigator should be able to 
		/// move among all siblings of the <paramref name="navigator"/> defining the 
		/// scope.</param>
		/// <remarks>The incoming navigator is cloned upon construction, 
		/// which isolates the calling code from movements to the 
		/// <see cref="SubtreeXPathNavigator"/>.</remarks>
		public SubtreeXPathNavigator(XPathNavigator navigator, bool enableFragment)
		{
			this.IsAtRoot = true;
			this.navigator = navigator.Clone();
			this.root = navigator.Clone();
			this.fragment = enableFragment;
		}

		private SubtreeXPathNavigator(XPathNavigator root, XPathNavigator current, bool isAtRoot, bool enableFragment)
		{
			this.root = root.Clone();
			this.navigator = current.Clone();
			this.IsAtRoot = isAtRoot;
			this.fragment = enableFragment;
		}

		/// <summary>
		/// See <see cref="XPathNavigator.BaseURI"/>.
		/// </summary>
		public override string BaseURI
		{
			get
			{
				return this.IsAtRoot ? string.Empty : this.navigator.BaseURI;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.HasAttributes"/>.
		/// </summary>
		public override bool HasAttributes
		{
			get
			{
				return !this.IsAtRoot && this.navigator.HasAttributes;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.HasChildren"/>.
		/// </summary>
		public override bool HasChildren
		{
			get
			{
				return this.IsAtRoot || this.navigator.HasChildren;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.IsEmptyElement"/>.
		/// </summary>
		public override bool IsEmptyElement
		{
			get
			{
				return !this.IsAtRoot && this.navigator.IsEmptyElement;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.LocalName"/>.
		/// </summary>
		public override string LocalName
		{
			get
			{
				return this.IsAtRoot ? string.Empty : this.navigator.LocalName;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.Name"/>.
		/// </summary>
		public override string Name
		{
			get
			{
				return this.IsAtRoot ? string.Empty : this.navigator.Name;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.NameTable"/>.
		/// </summary>
		public override XmlNameTable NameTable
		{
			get
			{
				return this.navigator.NameTable;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.NamespaceURI"/>.
		/// </summary>
		public override string NamespaceURI
		{
			get
			{
				return this.IsAtRoot ? string.Empty : this.navigator.NamespaceURI;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.NodeType"/>.
		/// </summary>
		public override XPathNodeType NodeType
		{
			get
			{
				return this.IsAtRoot ? XPathNodeType.Root : this.navigator.NodeType;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.Prefix"/>.
		/// </summary>
		public override string Prefix
		{
			get
			{
				return this.IsAtRoot ? string.Empty : this.navigator.Prefix;
			}
		}

		/// <summary>
		/// See <see cref="XPathItem.Value"/>.
		/// </summary>
		public override string Value
		{
			get
			{
				return this.IsAtRoot ? string.Empty : this.navigator.Value;
			}
		}

		/// <summary>
		/// See <see cref="XPathNavigator.XmlLang"/>.
		/// </summary>
		public override string XmlLang
		{
			get
			{
				return this.IsAtRoot ? string.Empty : this.navigator.XmlLang;
			}
		}

		/// <summary>
		/// Determines whether the navigator is on the root node (before the first child).
		/// </summary>
		private bool IsAtRoot { get; set; }

		/// <summary>
		/// Determines whether the navigator is at the same position as the "document element".
		/// </summary>
		private bool IsTop
		{
			get
			{
				return this.navigator.IsSamePosition(this.root);
			}
		}

		/// <summary>
		/// Creates new cloned version of the <see cref="SubtreeXPathNavigator"/>.
		/// </summary>
		/// <returns>Cloned copy of the <see cref="SubtreeXPathNavigator"/>.</returns>
		public override XPathNavigator Clone()
		{
			return new SubtreeXPathNavigator(this.root, this.navigator, this.IsAtRoot, this.fragment);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.GetAttribute"/>.
		/// </summary>
		public override string GetAttribute(string localName, string namespaceURI)
		{
			return this.IsAtRoot ? string.Empty : this.navigator.GetAttribute(localName, namespaceURI);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.GetNamespace"/>.
		/// </summary>
		public override string GetNamespace(string localName)
		{
			return this.IsAtRoot ? string.Empty : this.navigator.GetNamespace(localName);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.IsSamePosition"/>.
		/// </summary>
		public override bool IsSamePosition(XPathNavigator other)
		{
			if (other == null || !(other is SubtreeXPathNavigator))
			{
				return false;
			}

			var nav = (SubtreeXPathNavigator)other;
			return nav.IsAtRoot == this.IsAtRoot && nav.navigator.IsSamePosition(this.navigator) && nav.root.IsSamePosition(this.root);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveTo"/>.
		/// </summary>
		public override bool MoveTo(XPathNavigator other)
		{
			if (other == null || !(other is SubtreeXPathNavigator))
			{
				return false;
			}

			return this.navigator.MoveTo(((SubtreeXPathNavigator)other).navigator);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToAttribute"/>.
		/// </summary>
		public override bool MoveToAttribute(string localName, string namespaceURI)
		{
			return !this.IsAtRoot && this.navigator.MoveToAttribute(localName, namespaceURI);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToFirst"/>.
		/// </summary>
		public override bool MoveToFirst()
		{
			if (this.IsAtRoot)
			{
				return false;
			}

			if (this.IsTop)
			{
				if (!this.fragment)
				{
					return false;
				}

				if (this.root.MoveToFirst())
				{
					this.navigator.MoveToFirst();
					return true;
				}
			}

			return this.navigator.MoveToNext();
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToFirstAttribute"/>.
		/// </summary>
		public override bool MoveToFirstAttribute()
		{
			return !this.IsAtRoot && this.navigator.MoveToFirstAttribute();
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToFirstChild"/>.
		/// </summary>
		public override bool MoveToFirstChild()
		{
			if (this.IsAtRoot)
			{
				this.IsAtRoot = false;
				return true;
			}

			return this.navigator.MoveToFirstChild();
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToFirstNamespace(XPathNamespaceScope)"/>.
		/// </summary>
		public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
		{
			return !this.IsAtRoot && this.navigator.MoveToFirstNamespace(namespaceScope);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToId"/>.
		/// </summary>
		public override bool MoveToId(string id)
		{
			return this.navigator.MoveToId(id);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToNamespace"/>.
		/// </summary>
		public override bool MoveToNamespace(string @namespace)
		{
			return !this.IsAtRoot && this.navigator.MoveToNamespace(@namespace);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToNext()"/>.
		/// </summary>
		public override bool MoveToNext()
		{
			if (this.IsAtRoot)
			{
				return false;
			}

			if (this.IsTop)
			{
				if (!this.fragment)
				{
					return false;
				}

				if (this.root.MoveToNext())
				{
					this.navigator.MoveToNext();
					return true;
				}
			}

			return this.navigator.MoveToNext();
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToNextAttribute"/>.
		/// </summary>
		public override bool MoveToNextAttribute()
		{
			return !this.IsAtRoot && this.navigator.MoveToNextAttribute();
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToNextNamespace(XPathNamespaceScope)"/>.
		/// </summary>
		public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
		{
			return !this.IsAtRoot && this.navigator.MoveToNextNamespace(namespaceScope);
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToParent"/>.
		/// </summary>
		public override bool MoveToParent()
		{
			if (this.IsAtRoot)
			{
				return false;
			}

			if (this.IsTop)
			{
				this.IsAtRoot = true;
				return true;
			}

			return this.navigator.MoveToParent();
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToPrevious"/>.
		/// </summary>
		public override bool MoveToPrevious()
		{
			if (this.IsAtRoot)
			{
				return false;
			}

			if (this.IsTop)
			{
				if (!this.fragment)
					return false;
				
				if (this.root.MoveToPrevious())
				{
					this.navigator.MoveToPrevious();
					return true;
				}
			}

			return this.navigator.MoveToPrevious();
		}

		/// <summary>
		/// See <see cref="XPathNavigator.MoveToRoot"/>.
		/// </summary>
		public override void MoveToRoot()
		{
			this.navigator = this.root.Clone();
			this.IsAtRoot = true;
		}
	}
}