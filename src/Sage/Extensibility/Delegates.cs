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
	/// <seealso cref="ResourceManager.CopyTree"/>
	/// <seealso cref="ResourceManager.RegisterNodeHandler"/>
	public delegate XmlNode ProcessNode(XmlNode node, SageContext context);

	/// <summary>
	/// Defines the signature of a method that can be used to substitute placeholders in element or attribute text during 
	/// copying of an XML document.
	/// </summary>
	/// <param name="variableName">The name of the variable that was matched.</param>
	/// <param name="context">The context under which the method is being executed.</param>
	/// <returns>The text that should be used instead of the original text, or a <c>null</c> if the node should be skipped.</returns>
	/// <seealso cref="ResourceManager.CopyTree"/>
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
