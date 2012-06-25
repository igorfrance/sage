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
 * 
 * Original source for XPointer released under BSD licence, hence the disclaimer:
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */
namespace Kelp.XInclude.Common
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Xml;
	using System.Xml.XPath;

	using Kelp.Properties;

	/// <summary>
	/// Constructs <see cref="XmlNodeList"/> instances from 
	/// <see cref="XPathNodeIterator"/> objects.
	/// </summary>
	/// <remarks>See http://weblogs.asp.net/cazzu/archive/2004/04/14/113479.aspx. 
	/// <para>Author: Daniel Cazzulino, <a href="http://clariusconsulting.net/kzu">blog</a></para>
	/// <para>Contributors: Oleg Tkachenko, http://www.xmllab.net</para>
	/// </remarks>
	public static class XmlNodeListFactory
	{
		/// <summary>
		/// Creates an instance of a <see cref="XmlNodeList"/> that allows 
		/// enumerating <see cref="XmlNode"/> elements in the iterator.
		/// </summary>
		/// <param name="iterator">The result of a previous node selection 
		/// through an <see cref="XPathNavigator"/> query.</param>
		/// <returns>An initialized list ready to be enumerated.</returns>
		/// <remarks>The underlying XML store used to issue the query must be 
		/// an object inheriting <see cref="XmlNode"/>, such as 
		/// <see cref="XmlDocument"/>.</remarks>
		public static XmlNodeList CreateNodeList(XPathNodeIterator iterator)
		{
			return new XmlNodeListIterator(iterator);
		}

		private class XmlNodeListIterator : XmlNodeList
		{
			private readonly XPathNodeIterator iterator;
			private readonly IList<XmlNode> nodes = new List<XmlNode>();

			public XmlNodeListIterator(XPathNodeIterator iterator)
			{
				this.iterator = iterator.Clone();
			}

			public override int Count
			{
				get
				{
					if (!this.Done)
					{
						this.ReadToEnd();
					}

					return this.nodes.Count;
				}
			}

			/// <summary>
			/// Current count of nodes in the iterator (read so far).
			/// </summary>
			private int CurrentPosition
			{
				get
				{
					return this.nodes.Count;
				}
			}

			/// <summary>
			/// Flags that the iterator has been consumed.
			/// </summary>
			private bool Done { get; set; }

			public override IEnumerator GetEnumerator()
			{
				return new XmlNodeListEnumerator(this);
			}

			public override XmlNode Item(int index)
			{
				if (index >= this.nodes.Count)
				{
					this.ReadTo(index);
				}

				// Compatible behavior with .NET
				if (index >= this.nodes.Count || index < 0)
				{
					return null;
				}

				return this.nodes[index];
			}

			/// <summary>
			/// Reads up to the specified index, or until the 
			/// iterator is consumed.
			/// </summary>
			private void ReadTo(int to)
			{
				while (this.nodes.Count <= to)
				{
					if (this.iterator.MoveNext())
					{
						var node = this.iterator.Current as IHasXmlNode;

						// Check IHasXmlNode interface.
						if (node == null)
						{
							throw new ArgumentException(Resources.XmlNodeListFactory_IHasXmlNodeMissing);
						}

						this.nodes.Add(node.GetNode());
					}
					else
					{
						this.Done = true;
						return;
					}
				}
			}

			/// <summary>
			/// Reads the entire iterator.
			/// </summary>
			private void ReadToEnd()
			{
				while (this.iterator.MoveNext())
				{
					var node = this.iterator.Current as IHasXmlNode;

					// Check IHasXmlNode interface.
					if (node == null)
					{
						throw new ArgumentException(Resources.XmlNodeListFactory_IHasXmlNodeMissing);
					}

					this.nodes.Add(node.GetNode());
				}

				this.Done = true;
			}

			private class XmlNodeListEnumerator : IEnumerator
			{
				private readonly XmlNodeListIterator iterator;
				private int position = -1;

				public XmlNodeListEnumerator(XmlNodeListIterator iterator)
				{
					this.iterator = iterator;
				}

				object IEnumerator.Current
				{
					get
					{
						return this.iterator[this.position];
					}
				}

				bool IEnumerator.MoveNext()
				{
					this.position++;
					this.iterator.ReadTo(this.position);

					// If we reached the end and our index is still 
					// bigger, there're no more items.
					if (this.iterator.Done && this.position >= this.iterator.CurrentPosition)
					{
						return false;
					}

					return true;
				}

				void IEnumerator.Reset()
				{
					this.position = -1;
				}
			}
		}
	}
}