﻿/**
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
namespace Sage.DevTools.Modules
{
	using System;
	using System.Xml;

	using Kelp.Extensions;

	using Sage.Configuration;
	using Sage.Modules;
	using Sage.Views;

	public class DeveloperToolbarModule : IModule
	{
		public ModuleResult ProcessElement(XmlElement moduleElement, ViewConfiguration configuration)
		{
			SageContext context = configuration.Context;
			if (!context.IsDeveloperRequest)
				return null;

			ModuleResult result = new ModuleResult(moduleElement);
			XmlDocument ownerDoc = moduleElement.OwnerDocument;
			XmlElement dataElement = ownerDoc.CreateElement("mod:data", XmlNamespaces.ModulesNamespace);
			XmlElement metaElement = dataElement.AppendElement("mod:meta", XmlNamespaces.ModulesNamespace);

			foreach (MetaViewInfo info in context.ProjectConfiguration.MetaViews.Values)
			{
				XmlElement viewElement = metaElement.AppendElement("mod:view", XmlNamespaces.ModulesNamespace);
				viewElement.SetAttribute("name", info.Name);
				viewElement.InnerText = info.Description;
			}

			result.AppendDataNode(dataElement);
			return result;
		}
	}
}