﻿// <auto-generated>Marked as auto-generated so StyleCop will ignore BDD style tests</auto-generated>

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

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
namespace Sage.Test.ResourceManagement
{
	using System;
	using System.IO;
	using System.Xml;

	using Machine.Specifications;
	using Sage.ResourceManagement;

	[Subject(typeof(ResourceManager)), Tags(Categories.ResourceManagement)]
	public class When_registering_node_handler
	{
		static readonly SageContext context = Mother.CreateSageContext("default", "uk");
		static readonly XmlDocument document = new XmlDocument();
		static XmlNode subjectNode;

		Establish ctx = () =>
		{
			document.LoadXml("<document><myelement/></document>");
			subjectNode = document.SelectSingleNode("//myelement");
		};

		It Should_throw_exceptions_with_null_parameters = () => 
		{
			Catch.Exception(() => ResourceManager.RegisterNodeHandler(XmlNodeType.Element, string.Empty, string.Empty, null)).ShouldBeOfType<ArgumentNullException>();
			Catch.Exception(() => ResourceManager.RegisterNodeHandler(XmlNodeType.Element, "node1", string.Empty, null)).ShouldBeOfType<ArgumentNullException>();
		};

		It Should_register_the_handler_correctly = () => 
		{
			ResourceManager.RegisterNodeHandler(XmlNodeType.Element, "myelement", string.Empty, ProcessMyElement);
			ResourceManager.GetNodeHandler(subjectNode).ShouldEqual(ProcessMyElement);
		};

		It Should_use_the_handler_correctly = () => 
		{
			ResourceManager.RegisterNodeHandler(XmlNodeType.Element, "myelement", string.Empty, ProcessMyElement);
			ResourceManager.CopyTree(document, context).InnerText.ShouldEqual("Hello!");
		};

		static XmlNode ProcessMyElement(XmlNode node, SageContext context)
		{
			return node.OwnerDocument.CreateTextNode("Hello!");
		}
	}

	[Subject(typeof(ResourceManager)), Tags(Categories.ResourceManagement)]
	public class When_registering_text_handler
	{
		static readonly SageContext context = Mother.CreateSageContext("default", "uk");
		static XmlDocument document = new XmlDocument();
		static string varName = "myvar";

		Establish ctx = () => document.LoadXml("<document test='I say: ${myvar}'>${myvar}</document>");

		It Should_throw_exceptions_with_null_parameters = () =>
		{
			Catch.Exception(() => ResourceManager.RegisterTextHandler(null, null)).ShouldBeOfType<ArgumentNullException>();
			Catch.Exception(() => ResourceManager.RegisterTextHandler(varName, null)).ShouldBeOfType<ArgumentNullException>();
		};

		It Should_use_the_handler_correctly = () => 
		{
			ResourceManager.RegisterTextHandler(varName, SubstituteText);
			XmlElement result = (XmlElement) ResourceManager.CopyTree(document, context);
			result.InnerText.ShouldEqual("Hello!");
			result.GetAttribute("test").ShouldEqual("I say: Hello!");
		};

		static string SubstituteText(string varName, SageContext context)
		{
			return "Hello!";
		}
	}

	[Subject(typeof(ResourceManager)), Tags(Categories.ResourceManagement)]
	public class When_loading_xml_documents
	{
		static readonly SageContext context = Mother.CreateSageContext("default", "uk");
		static readonly string filePath= context.MapPath("~/assets/default/views/gallery/index.xml");
		static readonly string filePathMissing = context.MapPath("~/assets/file/that/doesnt/exist.xml");

		It Should_throw_ArgumentNullException_with_null_filepath = () =>
			Catch.Exception(() => ResourceManager.LoadXmlDocument(string.Empty, null)).ShouldBeOfType(typeof(ArgumentNullException));

		It Should_correctly_load_xml_document_that_exists = () =>
			ResourceManager.LoadXmlDocument(filePath, null).ShouldBe(typeof(CacheableXmlDocument));

		It Should_throw_IOException_when_attempting_to_load_a_missing_file = () =>
			Catch.Exception(() => ResourceManager.LoadXmlDocument(filePathMissing, null)).ShouldBe(typeof(IOException));

		It Should_load_document_globalized_to_english_if_the_document_is_globalizable_and_language_is_english = () =>
			ResourceManager.LoadXmlDocument(filePath, Mother.CreateSageContext("default", "uk"))
				.DocumentElement.InnerText.Trim().ShouldEqual("Test phrase in english");

		It Should_load_document_globalized_to_german_if_the_document_is_globalizable_and_language_is_german = () =>
			ResourceManager.LoadXmlDocument(filePath, Mother.CreateSageContext("default", "de"))
				.DocumentElement.InnerText.Trim().ShouldEqual("Test phrase in german");

		It Should_load_document_globalized_to_japanese_if_the_document_is_globalizable_and_language_is_japanese = () =>
			ResourceManager.LoadXmlDocument(filePath, Mother.CreateSageContext("default", "jp"))
				.DocumentElement.InnerText.Trim().ShouldEqual("Test phrase in japanese");

		It Should_correctly_load_embedded_xml_document = () =>
			ResourceManager.LoadXmlDocument("sageresx://sage.test/resources/index.xml", null)
				.DocumentElement.InnerText.Trim().ShouldEqual("Embedded resource.");

		It Should_correctly_load_and_globalize_embedded_xml_document = () =>
			ResourceManager.LoadXmlDocument("sageresx://sage.test/resources/index.xml", Mother.CreateSageContext("default", "uk"))
				.DocumentElement.InnerText.Trim().ShouldContain("english");

		It Should_correctly_load_xincluding_xml_document = () =>
			ResourceManager.LoadXmlDocument("sageresx://sage.test/resources/index.xml", null)
				.DocumentElement.SelectSingleNode("included").ShouldNotBeNull();
	}

	[Subject(typeof(ResourceManager)), Tags(Categories.ResourceManagement)]
	public class When_loading_missing_files
	{
		private static SageContext context;
		private static ResourceManager resourceManager;
		private static Exception exception;

		private Establish ctx = () =>
		{
			context = Mother.CreateSageContext("default", "com");
			resourceManager = new ResourceManager(context);
		};

		private Because of = () => exception = Catch.Exception(() => resourceManager.LoadXml("Imaginary/Path.Xml"));
		private It Should_Throw_a_FileNotFoundException = () => exception.ShouldBeOfType<FileNotFoundException>();
	}

	[Subject(typeof(ResourceManager)), Tags(Categories.ResourceManagement)]
	public class When_loading_a_xml_document
	{
		private static SageContext context;
		private static ResourceManager resourceManager;
		private static XmlDocument xmlDocument;

		private Establish ctx = () =>
		{
			context = Mother.CreateSageContext("default", "com");
			resourceManager = new ResourceManager(context);
		};

		private Because of = () => xmlDocument = resourceManager.LoadXml(
			Utilities.ExpandResourcePath("TestSite/assets/default/configuration/dictionary/en.xml"));

		private It Should_Not_be_Null = () => xmlDocument.ShouldNotBeNull();
	}
}