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
namespace Kelp.XInclude.XPath
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Xml;
	using System.Xml.XPath;

	using Kelp.Properties;
	using Kelp.XInclude.Common;

	/// <summary>
	/// Implements a cache of XPath queries, for faster execution.
	/// </summary>
	/// <remarks>
	/// Discussed at http://weblogs.asp.net/cazzu/archive/2004/04/02/106667.aspx
	/// <para>Author: Daniel Cazzulino, <a href="http://clariusconsulting.net/kzu">blog</a></para>
	/// </remarks>
	public static class XPathCache
	{
		static XPathCache()
		{
			Cache = new Dictionary<string, XPathExpression>();
		}

		/// <summary>
		/// Initially a simple hashtable. In the future should 
		/// implement sliding expiration of unused expressions.
		/// </summary>
		private static IDictionary<string, XPathExpression> Cache { get; set; }

		/// <summary>
		/// Evaluates the given expression and returns the typed result.
		/// </summary>
		public static object Evaluate(
			string expression, XPathNavigator source, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			expr.SetContext(PrepareContext(source, null, null, variables));
			return source.Evaluate(expr);
		}

		/// <summary>
		/// Evaluates the given expression and returns the typed result.
		/// </summary>
		public static object Evaluate(
			string expression, XPathNavigator source, XmlNamespaceManager context, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			expr.SetContext(PrepareContext(source, context, null, variables));
			return source.Evaluate(expr);
		}

		/// <summary>
		/// Evaluates the given expression and returns the typed result.
		/// </summary>
		public static object Evaluate(
			string expression, XPathNavigator source, XmlPrefix[] prefixes, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			expr.SetContext(PrepareContext(source, null, prefixes, variables));
			return source.Evaluate(expr);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator Select(
			string expression, XPathNavigator source, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			expr.SetContext(PrepareContext(source, null, null, variables));
			return source.Select(expr);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator Select(
			string expression, XPathNavigator source, XmlNamespaceManager context, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			expr.SetContext(PrepareContext(source, context, null, variables));
			return source.Select(expr);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator Select(
			string expression, XPathNavigator source, XmlPrefix[] prefixes, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			expr.SetContext(PrepareContext(source, null, prefixes, variables));
			return source.Select(expr);
		}

		/// <summary>
		/// Selects a list of nodes matching the XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodes(
			string expression, XmlNode source, params XPathVariable[] variables)
		{
			XPathNodeIterator it = Select(expression, source.CreateNavigator(), variables);
			return XmlNodeListFactory.CreateNodeList(it);
		}

		/// <summary>
		/// Selects a list of nodes matching the XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodes(
			string expression, XmlNode source, XmlNamespaceManager context, params XPathVariable[] variables)
		{
			XPathNodeIterator it = Select(expression, source.CreateNavigator(), context, variables);
			return XmlNodeListFactory.CreateNodeList(it);
		}

		/// <summary>
		/// Selects a list of nodes matching the XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodes(string expression, XmlNode source, XmlPrefix[] prefixes, params XPathVariable[] variables)
		{
			XPathNodeIterator it = Select(expression, source.CreateNavigator(), prefixes, variables);
			return XmlNodeListFactory.CreateNodeList(it);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodesSorted(
			string expression, XmlNode source, object sortExpression, IComparer comparer, params XPathVariable[] variables)
		{
			return
				XmlNodeListFactory.CreateNodeList(SelectSorted(expression, source.CreateNavigator(), sortExpression, comparer, variables));
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodesSorted(
			string expression, XmlNode source, object sortExpression, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType, params XPathVariable[] variables)
		{
			return
				XmlNodeListFactory.CreateNodeList(
					SelectSorted(expression, source.CreateNavigator(), sortExpression, order, caseOrder, lang, dataType, variables));
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodesSorted(
			string expression, XmlNode source, object sortExpression, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType, XmlNamespaceManager context, params XPathVariable[] variables)
		{
			return
				XmlNodeListFactory.CreateNodeList(
					SelectSorted(expression, source.CreateNavigator(), sortExpression, order, caseOrder, lang, dataType, context, variables));
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodesSorted(
			string expression, XmlNode source, object sortExpression, IComparer comparer, XmlNamespaceManager context, params XPathVariable[] variables)
		{
			return
				XmlNodeListFactory.CreateNodeList(
					SelectSorted(expression, source.CreateNavigator(), sortExpression, comparer, context, variables));
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodesSorted(
			string expression, XmlNode source, object sortExpression, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType, XmlPrefix[] prefixes, params XPathVariable[] variables)
		{
			return
				XmlNodeListFactory.CreateNodeList(
					SelectSorted(expression, source.CreateNavigator(), sortExpression, order, caseOrder, lang, dataType, prefixes, variables));
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XmlNodeList SelectNodesSorted(
			string expression, XmlNode source, object sortExpression, IComparer comparer, XmlPrefix[] prefixes, params XPathVariable[] variables)
		{
			return
				XmlNodeListFactory.CreateNodeList(
					SelectSorted(expression, source.CreateNavigator(), sortExpression, comparer, prefixes, variables));
		}

		/// <summary>
		/// Selects the first XmlNode that matches the XPath expression.
		/// </summary>
		public static XmlNode SelectSingleNode(
			string expression, XmlNode source, params XPathVariable[] variables)
		{
			foreach (XmlNode node in SelectNodes(expression, source, variables))
			{
				return node;
			}

			return null;
		}

		/// <summary>
		/// Selects the first XmlNode that matches the XPath expression.
		/// </summary>
		public static XmlNode SelectSingleNode(
			string expression, XmlNode source, XmlNamespaceManager context, params XPathVariable[] variables)
		{
			foreach (XmlNode node in SelectNodes(expression, source, context, variables))
			{
				return node;
			}

			return null;
		}

		/// <summary>
		/// Selects the first XmlNode that matches the XPath expression.
		/// </summary>
		public static XmlNode SelectSingleNode(
			string expression, XmlNode source, XmlPrefix[] prefixes, params XPathVariable[] variables)
		{
			foreach (XmlNode node in SelectNodes(expression, source, prefixes, variables))
			{
				return node;
			}

			return null;
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator SelectSorted(
			string expression, XPathNavigator source, object sortExpression, IComparer comparer, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			expr.SetContext(PrepareContext(source, null, null, variables));
			PrepareSort(expr, source, sortExpression, comparer);
			return source.Select(expr);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator SelectSorted(
			string expression, XPathNavigator source, object sortExpression, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			expr.SetContext(PrepareContext(source, null, null, variables));
			PrepareSort(expr, source, sortExpression, order, caseOrder, lang, dataType);
			return source.Select(expr);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator SelectSorted(
			string expression, XPathNavigator source, object sortExpression, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType, XmlNamespaceManager context, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			XmlNamespaceManager ctx = PrepareContext(source, context, null, variables);
			expr.SetContext(ctx);
			PrepareSort(expr, source, sortExpression, order, caseOrder, lang, dataType, ctx);
			return source.Select(expr);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator SelectSorted(
			string expression, XPathNavigator source, object sortExpression, IComparer comparer, XmlNamespaceManager context, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			XmlNamespaceManager ctx = PrepareContext(source, context, null, variables);
			expr.SetContext(ctx);
			PrepareSort(expr, source, sortExpression, comparer, ctx);
			return source.Select(expr);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator SelectSorted(
			string expression, XPathNavigator source, object sortExpression, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType, XmlPrefix[] prefixes, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			XmlNamespaceManager ctx = PrepareContext(source, null, prefixes, variables);
			expr.SetContext(ctx);
			PrepareSort(expr, source, sortExpression, order, caseOrder, lang, dataType, ctx);
			return source.Select(expr);
		}

		/// <summary>
		/// Selects a node set using the specified XPath expression.
		/// </summary>
		public static XPathNodeIterator SelectSorted(
			string expression, XPathNavigator source, object sortExpression, IComparer comparer, XmlPrefix[] prefixes, params XPathVariable[] variables)
		{
			XPathExpression expr = GetCompiledExpression(expression, source);
			XmlNamespaceManager ctx = PrepareContext(source, null, prefixes, variables);
			expr.SetContext(ctx);
			PrepareSort(expr, source, sortExpression, comparer, ctx);
			return source.Select(expr);
		}

		/// <summary>
		/// Sets up the context for expression execution.
		/// </summary>
		private static XmlNamespaceManager PrepareContext(
			XPathNavigator source, XmlNamespaceManager context, IEnumerable<XmlPrefix> prefixes, IEnumerable<XPathVariable> variables)
		{
			XmlNamespaceManager ctx = context;

			// If we have variables, we need the dynamic context. 
			if (variables != null)
			{
				DynamicContext dyn = ctx != null ? new DynamicContext(ctx) : new DynamicContext();

				// Add the variables we received.
				foreach (XPathVariable var in variables)
				{
					dyn.AddVariable(var.Name, var.Value);
				}

				ctx = dyn;
			}

			// If prefixes were added, append them to context.
			if (prefixes != null)
			{
				if (ctx == null)
				{
					ctx = new XmlNamespaceManager(source.NameTable);
				}

				foreach (XmlPrefix prefix in prefixes)
				{
					ctx.AddNamespace(prefix.Prefix, prefix.NamespaceURI);
				}
			}

			return ctx;
		}

		private static void PrepareSort(
			XPathExpression expression, XPathNavigator source, object sortExpression, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType)
		{
			if (sortExpression is string)
			{
				expression.AddSort(GetCompiledExpression((string)sortExpression, source), order, caseOrder, lang, dataType);
			}
			else if (sortExpression is XPathExpression)
			{
				expression.AddSort(sortExpression, order, caseOrder, lang, dataType);
			}
			else
			{
				throw new XPathException(Resources.XPathCache_BadSortObject, null);
			}
		}

		private static void PrepareSort(
			XPathExpression expression, XPathNavigator source, object sortExpression, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType, XmlNamespaceManager context)
		{
			XPathExpression se;

			if (sortExpression is string)
			{
				se = GetCompiledExpression((string)sortExpression, source);
			}
			else if (sortExpression is XPathExpression)
			{
				se = (XPathExpression)sortExpression;
			}
			else
			{
				throw new XPathException(Resources.XPathCache_BadSortObject, null);
			}

			se.SetContext(context);
			expression.AddSort(se, order, caseOrder, lang, dataType);
		}

		private static void PrepareSort(
			XPathExpression expression, XPathNavigator source, object sortExpression, IComparer comparer)
		{
			if (sortExpression is string)
			{
				expression.AddSort(GetCompiledExpression((string)sortExpression, source), comparer);
			}
			else if (sortExpression is XPathExpression)
			{
				expression.AddSort(sortExpression, comparer);
			}
			else
			{
				throw new XPathException(Resources.XPathCache_BadSortObject, null);
			}
		}

		private static void PrepareSort(
			XPathExpression expression, XPathNavigator source, object sortExpression, IComparer comparer, XmlNamespaceManager context)
		{
			XPathExpression se;

			if (sortExpression is string)
			{
				se = GetCompiledExpression((string)sortExpression, source);
			}
			else if (sortExpression is XPathExpression)
			{
				se = (XPathExpression)sortExpression;
			}
			else
			{
				throw new XPathException(Resources.XPathCache_BadSortObject, null);
			}

			se.SetContext(context);
			expression.AddSort(se, comparer);
		}

		/// <summary>
		/// Retrieves a cached compiled expression, or a newly compiled one.
		/// </summary>
		private static XPathExpression GetCompiledExpression(
			string expression, XPathNavigator source)
		{
			XPathExpression expr;

			if (!Cache.TryGetValue(expression, out expr))
			{
				// No double checks. At most we will compile twice. No big deal.			  
				expr = source.Compile(expression);
				Cache[expression] = expr;
			}

			return expr.Clone();
		}
	}
}