/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides xml utilities
 * @type {Function}
 */
var $xml = new function xml()
{
	var parser;
	var parseErrorNs;
	var parserThrows = false;
	var implementation = document.implementation;
	var domcompatible = window.XMLSerializer != null;
	var xhtmlNamespace = "http://www.w3.org/1999/xhtml";
	var ORDERED_NODE_ITERATOR_TYPE = 5;

	var nodeType =
	{
		ELEMENT: 1,
		ATTRIBUTE: 2,
		TEXT: 3,
		CDATA_SECTION: 4,
		ENTITY_REFERENCE: 5,
		ENTITY: 6,
		PROCESSING_INSTRUCTION: 7,
		COMMENT: 8,
		DOCUMENT: 9,
		DOCUMENT_TYPE: 10,
		DOCUMENT_FRAGMENT: 11,
		NOTATION: 12,
	};

	/**
	 * Returns an XML node or XML document, depending on the type of <c>value</c>.
	 *
	 * <p>If the specified <c>value</c> is a string, a new document will be created and initialized using the <c>value</c>
	 * as it's XML content. The string <c>value</c> should not be blank and it should start with a '&lt;' to even be
	 * considered.</p>
	 * <p>If the speciufied <c>value</c> is an XML node, it will be returned unchanged.</p>
	 * @param {XMLNode|String} value Either the XML node that will be returned, or the string to create a new document with.
	 * @returns {XmlNode} Either the XML node that was specified
	 */
	var xml = function xml(value)
	{
		if ($type.isNode(value))
			return value;

		if ($type.isString(value))
		{
			if (value.trim().indexOf("<") == 0)
			{
				var document = xml.document(value);
				return document;
			}
		}
	};

	xml.text = function xml$text(node, xpath, namespaces)
	{
		$log.assert($type.isNode(node), "Argument 'node' should be a node");

		var selection = xpath ? xml.selectSingleNode(node, xpath, namespaces) : node;
		if (selection)
		{
			if (selection.nodeType == nodeType.DOCUMENT)
				selection = selection.documentElement;

			if (selection.textContent != null)
				return selection.textContent;

			if (selection.text != null)
				return node.text;

			return selection.innerText;
		}
	};

	xml.toString = function xml$toString(node)
	{
		if (arguments.length == 0)
			return this.prototype.toString.apply(this);

		if (domcompatible)
		{
			if (node == document || node.ownerDocument == document)
				return xml.fromCurrentHtml();

			return new XMLSerializer().serializeToString(node);
		}

		return node.xml;
	};

	xml.fromDocument = function xml$fromDocument(document)
	{
		if (document == undefined)
			document = window.document;

		function serialize(node)
		{
			if (node == null || node.nodeType == null)
				return "";

			if (node.nodeType == nodeType.DOCUMENT)
				return serialize(node.documentElement);

			var rslt = [];
			var skipNamespace = false;

			if (node.nodeType == nodeType.ELEMENT)
			{
				var nodeName = node.nodeName;
				if (node.namespaceURI == xhtmlNamespace)
				{
					nodeName = nodeName.toLowerCase();
					skipNamespace = true;
				}

				rslt.push("<");
				rslt.push(nodeName);
				for (var i = 0; i < node.attributes.length; i++)
				{
					var attrName = node.attributes[i].name;
					var attrValue = node.getAttribute(attrName);

					if (attrName == "xmlns" && skipNamespace)
						continue;

					rslt.push(" ");
					rslt.push(node.attributes[i].name);
					rslt.push("=\"");
					rslt.push(xml.escape(attrValue, true));
					rslt.push("\"");
				}

				if (node.childNodes.length == 0)
					rslt.push("/>");
				else
				{
					rslt.push(">");
					for (var i = 0; i < node.childNodes.length; i++)
					{
						var child = node.childNodes[i];
						if (child.nodeType == nodeType.ELEMENT)
						{
							rslt.push(serialize(child));
						}
						if (child.nodeType == nodeType.COMMENT)
						{
							rslt.push("<!--");
							rslt.push(xml.text(child));
							rslt.push("-->");
						}
						if (child.nodeType == nodeType.CDATA_SECTION)
						{
							rslt.push("<![CDATA[");
							rslt.push(xml.text(child));
							rslt.push("]]>");
						}
						if (child.nodeType == nodeType.TEXT)
						{
							rslt.push(xml.escape(xml.text(child)));
						}
					}
					rslt.push("</");
					rslt.push(nodeName);
					rslt.push(">");
				}
			}

			return rslt.join("");
		};

		var result;
		var html = serialize(document);

		if (window.ActiveXObject)
		{
			result = new ActiveXObject("MSXML2.DomDocument");
			result.loadXML(html);
		}
		else
		{
			result = xml.document(html);
		}

		return result;
	}

	xml.toObject = function xml$toObject(node)
	{
		if (node == null || node.nodeType == null)
			return null;

		var result = {};
		if (node.nodeType == nodeType.DOCUMENT)
		{
			var rootName = node.documentElement.nodeName;
			result[rootName] = xml.toObject(node.documentElement);
		}
		else if (node.nodeType == nodeType.ELEMENT)
		{
			for (var i = 0; i < node.attributes.length; i++)
			{
				result[node.attributes[i].name] =
					node.getAttribute(node.attributes[i].name);
			}
			for (var i = 0; i < node.childNodes.length; i++)
			{
				var child = node.childNodes[i];
				if (child.nodeType == nodeType.ELEMENT)
				{
					var parsed = xml.toObject(child);
					if (result[child.nodeName] != null)
					{
						if (!$type.isArray(result[child.nodeName]))
							result[child.nodeName] = [result[child.nodeName]];

						result[child.nodeName].push(parsed);
					}
					else
						result[child.nodeName] = parsed;
				}
				if (child.nodeType == nodeType.TEXT && child.nodeValue.trim() != $string.EMPTY)
					result.$text = child.nodeValue;
			}
		}
		return result;
	};

	xml.resolver = function xml$resolver(namespaces)
	{
		$log.assert($type.isObject(namespaces), "Argument 'namespaces' should be an object");

		return function NsResolver(prefix) /**/
		{
			return namespaces[prefix];
		};
	};

	/**
	 * Selects nodes from <c>subject</c>, using the specified <c>xpath</c> expression.
	 * @param {Node} subject The node from which to select. To select from the current document, omit this argument
	 * completely. Optional.
	 * @param {String} xpath The XPath selection string to use. Required.
	 * @param {Object} namespaces Optional object that defines the namespaces used by the <c>xpath</c> expression.
	 * The property names should be the namespace prefixes and property values the actual namespace URI's.
	 * @returns {Node[]} An array of nodes that were selected by the <c>xpath</c> expression.
	 */
	xml.select = function xml$select(subject, xpath, namespaces)
	{
		if ($type.isString(arguments[0]))
		{
			xpath = arguments[0];
			namespaces = arguments[1];

			subject = xml.fromDocument(document);
		}

		var ownerDocument = subject.nodeType == nodeType.DOCUMENT ? subject : subject.ownerDocument;
		if (window.ActiveXObject)
		{
			var nslist = new Array;
			for (var prefix in namespaces)
				nslist.push('xmlns:{0}="{1}"'.format(prefix, namespaces[prefix]));

			ownerDocument.setProperty("SelectionNamespaces", nslist.join(" "));
			return subject.selectNodes(xpath);
		}
		else
		{
			var nsResolver = namespaces
				? xml.resolver(namespaces)
				: null;

			var result = ownerDocument.evaluate(xpath, subject, nsResolver, ORDERED_NODE_ITERATOR_TYPE, null);
			var node = result.iterateNext();
			var nodeList = new Array;
			while (node)
			{
				nodeList.push(node);
				node = result.iterateNext();
			}
			return nodeList;
		}
	};

	xml.load = function xml$load(url, onload, onerror)
	{
		$log.assert($type.isString(url), "Argument 'url' is required");

		function xml$openXmlDocument$loaded(data, textStatus, request) /**/
		{
			var document = xml.document(request.responseText);
			onload(document);
		}

		$.ajax(url, {
			complete: xml$openXmlDocument$loaded,
			error: onerror
		});
	};

	xml.document = function xml$document(xmltext)
	{
		var document = null;
		if (domcompatible)
		{
			if (xmltext)
			{
				try
				{
					document = getParser().parseFromString(xmltext, "text/xml");
				}
				catch(e)
				{
					throw e;
				}

				if (!parserThrows)
					throwIfParseError(document);
			}
			else
			{
				document = implementation.createDocument($string.EMPTY, $string.EMPTY, null);
			}
		}
		else if (window.ActiveXObject)
		{
			document = new ActiveXObject("MSXML2.DomDocument");
			if (xmltext)
			{
				document.loadXML(xmltext);
				throwIfParseError(document);
			}
		}

		return document;
	};

	xml.processor = function xml$processor(document)
	{
		document = xml(document);
		if (document == null)
			return null;

		var processor = null;
		if (domcompatible)
		{
			processor = new XSLTProcessor();
			processor.importStylesheet(document);
		}
		else
		{
			var ftDocument = xml.document(document.xml, true);
			var template = new ActiveXObject("MSXML2.XslTemplate");
			template.stylesheet = ftDocument;
			processor = template.createProcessor();
		}

		return processor;
	};

	xml.transform = function xml$transform(document, xslProcessor)
	{
		$log.assert($type.isDocument(url), "Argument 'document' should be a document.");
		$log.assert(!$type.isNull(xslProcessor), "Argument 'xslProcessor' is required");

		var result = null;
		if (domcompatible)
		{
			result = xslProcessor.transformToDocument(document);
		}
		else
		{
			result = xml.document();
			xslProcessor.input = document;
			xslProcessor.output = result;
			xslProcessor.transform();
		}

		return result;
	};

	xml.escape = function escape(value, quotes)
	{
		var result = String(value)
			.replace(/&(?!amp;)/g, "&amp;")
			.replace(/</g, "&lt;")
			.replace(/>/g, "&gt;");

		if (quotes)
			result = result.replace(/\"/g, "&quot;");

		return result;
	}

	function throwIfParseError(document)
	{
		var info = { name: "XML Parse Error", error: false, data: { line: 1, column: 0 }};
		if (domcompatible)
		{
			var parseError = document.getElementsByTagNameNS(parseErrorNs, "parsererror")[0];
			if (parseError != null)
			{
				var message = xml.text(parseError).replace(/line(?: number)? (\d+)(?:,)?(?: at)? column (\d+):/i, function replace($0, $1, $2)
				{
					info.line = parseInt($1);
					info.column = parseInt($2);
					return $0 + "\n";
				});

				info.error = true;
				info.message = message;
			}
		}
		else
		{
			if (document.parseError != 0)
			{
				info.error = true;
				info.message = document.parseError.reason;

				if (document.parseError.line)
					info.data.line = document.parseError.line;

				if (document.parseError.column)
					info.data.column = document.parseError.column;
			}
		}

		if (info.error == true)
			throw error(info);
	}

	function getParser()
	{
		if (parser == null)
		{
			parser = new DOMParser();
			try
			{
				parseErrorNs = parser
					.parseFromString("INVALID XML", "text/xml")
					.getElementsByTagName("*")[0]
					.namespaceURI;
			}
			catch(e)
			{
				parserThrows = true;
			}
		}

		return parser;
	}

	xml.nodeType = nodeType;

	return xml;
};
