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
namespace Sage.Extensibility
{
	using System.Web.Mvc;
	using System.Xml;

	using Sage.Controllers;
	using Sage.ResourceManagement;

	/// <summary>
	/// Defines the signature of the delegate that can resolve, read and retreive specified resources.
	/// </summary>
	/// <param name="context">The context under which the code is executing.</param>
	/// <param name="resourceUri">The URI of the resource to open.</param>
	/// <returns>A <see cref="CacheableXmlDocument"/> associated with the <paramref name="resourceUri"/>.</returns>
	public delegate CacheableXmlDocument XmlProvider(SageContext context, string resourceUri);

	/// <summary>
	/// Defines the signature of a method that can be used to handle a single XML node during copying of an XML document.
	/// </summary>
	/// <param name="context">The context under which the method is being executed.</param>
	/// <param name="node">The node being processed.</param>
	/// <returns>The XML node that should be copied in the result document, or a <c>null</c> if the node should be skipped.</returns>
	public delegate XmlNode NodeHandler(SageContext context, XmlNode node);

	/// <summary>
	/// Defines the signature of a method that can be used to generate variable values.
	/// </summary>
	/// <param name="context">The context under which the method is being executed.</param>
	/// <param name="variableName">The name of the variable.</param>
	/// <returns>The variable value.</returns>
	public delegate string TextVariable(SageContext context, string variableName);

	/// <summary>
	/// Defines the signature of a method that can be used to further filter and/or modify the XML document that is 
	/// prepared by the controller for the XSLT transformation.
	/// </summary>
	/// <param name="controller">The controller calling this method.</param>
	/// <param name="viewContext">The current viewContext associated with this method call.</param>
	/// <param name="viewXml">The view XML document being changed.</param>
	/// <returns>The filtered version of the specified <paramref name="viewXml"/>.</returns>
	/// <seealso cref="ViewXmlFilterAttribute"/>
	/// <seealso cref="SageController.FilterViewXml"/>
	public delegate XmlDocument ViewXmlFilter(SageController controller, ViewContext viewContext, XmlDocument viewXml);

	/// <summary>
	/// Defines the signature of a method that can be used to process a function-like expression in text.
	/// </summary>
	/// <param name="context">The <see cref="SageContext"/> in which the method is executing.</param>
	/// <param name="arguments">The array of function arguments.</param>
	/// <returns>The result of evaluating this function.</returns>
	/// <seealso cref="TextFunctionAttribute"/>
	public delegate string TextFunction(SageContext context, params string[] arguments);
}
