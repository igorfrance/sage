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
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Xml;
	using System.Xml.XPath;
	using System.Xml.Xsl;

	/// <summary>
	/// Provides the evaluation context for fast execution and custom 
	/// variables resolution.
	/// </summary>
	public sealed class DynamicContext : XsltContext
	{
		private readonly IDictionary<string, IXsltContextVariable> variables = new Dictionary<string, IXsltContextVariable>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicContext"/> class.
		/// </summary>
		public DynamicContext()
			: base(new NameTable())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicContext"/> 
		/// class with the specified <see cref="NameTable"/>.
		/// </summary>
		/// <param name="table">The NameTable to use.</param>
		public DynamicContext(NameTable table)
			: base(table)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicContext"/> class.
		/// </summary>
		/// <param name="context">A previously filled context with the namespaces to use.</param>
		public DynamicContext(XmlNamespaceManager context)
			: this(context, new NameTable())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicContext"/> class.
		/// </summary>
		/// <param name="context">A previously filled context with the namespaces to use.</param>
		/// <param name="table">The NameTable to use.</param>
		public DynamicContext(XmlNamespaceManager context, NameTable table)
			: base(table)
		{
			object xml = table.Add(XmlNamespaces.XmlNamespace);
			object xmlns = table.Add(XmlNamespaces.XmlNamespacesNamespace);

			if (context != null)
			{
				foreach (string prefix in context)
				{
					string uri = context.LookupNamespace(prefix);
					if (Equals(uri, xml) || Equals(uri, xmlns))
					{
						continue;
					}

					this.AddNamespace(prefix, uri);
				}
			}
		}

		/// <summary>
		/// Same as <see cref="XsltContext"/>.
		/// </summary>
		public override bool Whitespace
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Shortcut method that compiles an expression using an empty navigator.
		/// </summary>
		/// <param name="xpath">The expression to compile</param>
		/// <returns>A compiled <see cref="XPathExpression"/>.</returns>
		public static XPathExpression Compile(string xpath)
		{
			return new XmlDocument().CreateNavigator().Compile(xpath);
		}

		/// <summary>
		/// Adds the variable to the dynamic evaluation context.
		/// </summary>
		/// <param name="name">The name of the variable to add to the context.</param>
		/// <param name="value">The value of the variable to add to the context.</param>
		/// <remarks>
		/// Value type conversion for XPath evaluation is as follows:
		/// <list type="table">
		///		<listheader>
		///			<term>CLR Type</term>
		///			<description>XPath type</description>
		///		</listheader>
		///		<item>
		///			<term>System.String</term>
		///			<description>XPathResultType.String</description>
		///		</item>
		///		<item>
		///			<term>System.Double (or types that can be converted to)</term>
		///			<description>XPathResultType.Number</description>
		///		</item>
		///		<item>
		///			<term>System.Boolean</term>
		///			<description>XPathResultType.Boolean</description>
		///		</item>
		///		<item>
		///			<term>System.Xml.XPath.XPathNavigator</term>
		///			<description>XPathResultType.Navigator</description>
		///		</item>
		///		<item>
		///			<term>System.Xml.XPath.XPathNodeIterator</term>
		///			<description>XPathResultType.NodeSet</description>
		///		</item>
		///		<item>
		///			<term>Others</term>
		///			<description>XPathResultType.Any</description>
		///		</item>
		/// </list>
		/// <note type="note">See the topic "Compile, Select, Evaluate, and Matches with 
		/// XPath and XPathExpressions" in MSDN documentation for additional information.</note>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The <paramref name="value"/> is null.</exception>
		public void AddVariable(string name, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			this.variables[name] = new DynamicVariable(value);
		}

		/// <summary>
		/// Implementation equal to <see cref="XsltContext"/>.
		/// </summary>
		public override int CompareDocument(string baseUri, string nextbaseUri)
		{
			return string.Compare(baseUri, nextbaseUri, false, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Same as <see cref="XmlNamespaceManager"/>.
		/// </summary>
		public override string LookupNamespace(string prefix)
		{
			string key = this.NameTable.Get(prefix);
			return key == null ? null : base.LookupNamespace(key);
		}

		/// <summary>
		/// Same as <see cref="XmlNamespaceManager"/>.
		/// </summary>
		public override string LookupPrefix(string uri)
		{
			string key = this.NameTable.Get(uri);
			return key == null ? null : base.LookupPrefix(key);
		}

		/// <summary>
		/// Same as <see cref="XsltContext"/>.
		/// </summary>
		public override bool PreserveWhitespace(XPathNavigator node)
		{
			return true;
		}

		/// <summary>
		/// See <see cref="XsltContext"/>. Not used in our implementation.
		/// </summary>
		public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] types)
		{
			return null;
		}

		/// <summary>
		/// Resolves the dynamic variables added to the context. See <see cref="XsltContext"/>. 
		/// </summary>
		public override IXsltContextVariable ResolveVariable(string prefix, string name)
		{
			IXsltContextVariable var;
			this.variables.TryGetValue(name, out var);
			return var;
		}

		/// <summary>
		/// Represents a variable during dynamic expression execution.
		/// </summary>
		internal class DynamicVariable : IXsltContextVariable
		{
			private readonly XPathResultType type;
			private readonly object value;

			/// <summary>
			/// Initializes a new instance of the class.
			/// </summary>
			/// <param name="value">The value of the variable.</param>
			public DynamicVariable(object value)
			{
				this.value = value;

				if (value is string)
				{
					this.type = XPathResultType.String;
				}
				else if (value is bool)
				{
					this.type = XPathResultType.Boolean;
				}
				else if (value is XPathNavigator)
				{
					this.type = XPathResultType.Navigator;
				}
				else if (value is XPathNodeIterator)
				{
					this.type = XPathResultType.NodeSet;
				}
				else
				{
					// Try to convert to double (native XPath numeric type)
					if (value is double)
					{
						this.type = XPathResultType.Number;
					}
					else
					{
						if (value is IConvertible)
						{
							try
							{
								this.value = Convert.ToDouble(value);

								// We suceeded, so it's a number.
								this.type = XPathResultType.Number;
							}
							catch (FormatException)
							{
								this.type = XPathResultType.Any;
							}
							catch (OverflowException)
							{
								this.type = XPathResultType.Any;
							}
						}
						else
						{
							this.type = XPathResultType.Any;
						}
					}
				}
			}

			bool IXsltContextVariable.IsLocal
			{
				get
				{
					return false;
				}
			}

			bool IXsltContextVariable.IsParam
			{
				get
				{
					return false;
				}
			}

			XPathResultType IXsltContextVariable.VariableType
			{
				get
				{
					return this.type;
				}
			}

			object IXsltContextVariable.Evaluate(XsltContext context)
			{
				return this.value;
			}
		}
	}
}