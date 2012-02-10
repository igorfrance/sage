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
	public delegate CacheableXmlDocument GetResource(SageContext context, string resourceUri);

	/// <summary>
	/// Defines the signature of a method that can be used to handle a single XML node during copying of an XML document.
	/// </summary>
	/// <param name="node">The node being processed.</param>
	/// <param name="context">The context under which the method is being executed.</param>
	/// <returns>The XML node that should be copied in the result document, or a <c>null</c> if the node should be skipped.</returns>
	/// <seealso cref="ResourceManager.CopyNode"/>
	/// <seealso cref="ResourceManager.RegisterNodeHandler"/>
	public delegate XmlNode ProcessNode(XmlNode node, SageContext context);

	/// <summary>
	/// Defines the signature of a method that can be used to substitute placeholders in element or attribute text during 
	/// copying of an XML document.
	/// </summary>
	/// <param name="variableName">The name of the variable that was matched.</param>
	/// <param name="context">The context under which the method is being executed.</param>
	/// <returns>The text that should be used instead of the original text, or a <c>null</c> if the node should be skipped.</returns>
	/// <seealso cref="ResourceManager.CopyNode"/>
	/// <seealso cref="ResourceManager.RegisterTextHandler"/>
	public delegate string ProcessText(string variableName, SageContext context);

	/// <summary>
	/// Defines the signature of a method that can be used to filter the XML document that view be server from the controller
	/// to the view.
	/// </summary>
	/// <param name="controller">The controller calling this method.</param>
	/// <param name="viewContext">The current viewContext associated with this method call.</param>
	/// <param name="viewXml">The view XML document being changed.</param>
	/// <returns>The filtered version of the specified <paramref name="viewXml"/>.</returns>
	/// <seealso cref="ViewXmlFilterAttribute"/>
	/// <seealso cref="SageController.FilterViewXml"/>
	public delegate XmlDocument FilterViewXml(SageController controller, ViewContext viewContext, XmlDocument viewXml);
}
