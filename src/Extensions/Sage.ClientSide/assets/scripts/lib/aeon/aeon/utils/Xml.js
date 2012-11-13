Type.registerNamespace("aeon.utils");

aeon.utils.Xml = function Xml(document)
{
	if (Type.isDocument(document))
		return $xml.serializeToString(document);
};

aeon.utils.Xml.PROGID_DOCUMENT = "MSXML2.DomDocument";
aeon.utils.Xml.PROGID_DOCUMENT_FT = "MSXML2.FreeThreadedDomDocument";
aeon.utils.Xml.PROGID_XSLTEMPLATE = "MSXML2.XslTemplate";

aeon.utils.Xml.outerXml = function Xml$outerXml(xmlNode)
{
	return $xml.serializeToString(xmlNode);
};

aeon.utils.Xml.innerText = function Xml$innerText(node, xpath, namespaces)
{
	$assert.isNode(node);

	var selection = xpath ? $xml.selectSingleNode(node, xpath, namespaces) : node;
	if (selection)
	{
		if (selection.textContent != null)
			return selection.textContent;

		if (selection.text != null)
			return node.text;

		return selection.innerText;
	}
};

aeon.utils.Xml.serializeToString = function Xml$serializeToString(xmlNode)
{
	$assert.isNode(xmlNode);

	if (window.ActiveXObject)
		return xmlNode.xml;
	else
		return (new XMLSerializer()).serializeToString(xmlNode);
};

aeon.utils.Xml.serializeToObject = function Xml$serializeToObject(xmlNode)
{
	if (xmlNode == null || xmlNode.nodeType == null)
		return null;

	var result = {};
	if (xmlNode.nodeType == 9)
	{
		var rootName = xmlNode.documentElement.nodeName;
		result[rootName] = $xml.serializeToObject(xmlNode.documentElement);
	}
	else if (xmlNode.nodeType == 1)
	{
		for (var i = 0; i < xmlNode.attributes.length; i++)
		{
			result[xmlNode.attributes[i].name] =
				xmlNode.getAttribute(xmlNode.attributes[i].name);
		}
		for (var i = 0; i < xmlNode.childNodes.length; i++)
		{
			var child = xmlNode.childNodes[i];
			if (child.nodeType == 1)
			{
				var parsed = $xml.serializeToObject(child);
				if (result[child.nodeName] != null)
				{
					if (!Type.isArray(result[child.nodeName]))
						result[child.nodeName] = [result[child.nodeName]];

					result[child.nodeName].push(parsed);
				}
				else
					result[child.nodeName] = parsed;
			}
			if (child.nodeType == 3 && child.nodeValue.trim() != "")
				result.$text = child.nodeValue;
		}
	}
	return result;
};

aeon.utils.Xml.createNsResolver = function Xml$createNsResolver(namespaces)
{
	$assert.isObject(namespaces);

	return function NsResolver(prefix) /**/
	{
		return namespaces[prefix];
	};
};

aeon.utils.Xml.selectSingleNode = function Xml$selectSingleNode(parent, xPath, namespaces)
{
	$assert.isNode(parent);
	$assert.isString(xPath);

	var doc = parent.nodeType == 9
		? parent
		: parent.ownerDocument;

	if (doc.evaluate)
	{
		var nsResolver = namespaces
			? $xml.createNsResolver(namespaces)
			: null;

		var xPathResult = doc.evaluate(xPath, parent, nsResolver, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
		return xPathResult.singleNodeValue;
	}
	else
	{
		var nslist = new Array;
		for (var prefix in namespaces)
			nslist.push('xmlns:{0}="{1}"'.format(prefix, namespaces[prefix]));

		doc.setProperty("SelectionNamespaces", nslist.join(" "));
		return parent.selectSingleNode(xPath);
	}
};

aeon.utils.Xml.selectNodes = function Xml$selectNodes(parent, xPath, namespaces)
{
	$assert.isNode(parent);
	$assert.isString(xPath);

	var doc = parent.nodeType == 9
		? parent
		: parent.ownerDocument;

	if (doc.evaluate)
	{
		var nsResolver = namespaces
			? $xml.createNsResolver(namespaces)
			: null;

		var xPathResult = doc.evaluate(xPath, parent, nsResolver, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);

		var node = xPathResult.iterateNext();
		var nodeList = new Array;
		while (node)
		{
			nodeList.push(node);
			node = xPathResult.iterateNext();
		}
		return nodeList;
	}
	else
	{
		var nslist = new Array;
		for (var prefix in namespaces)
			nslist.push('xmlns:{0}="{1}"'.format(prefix, namespaces[prefix]));

		doc.setProperty("SelectionNamespaces", nslist.join(" "));
		return parent.selectNodes(xPath);
	}
};

aeon.utils.Xml.loadFile = function Xml$loadFile(url, oncomplete, onerror)
{
	$assert.isString(url);
	$assert.isFunction(oncomplete);

	var request = new aeon.net.WebRequest(url);
	request.addListener("oncomplete", oncomplete);
	if (Type.isFunction(onerror))
		request.addListener("onerror", onerror);

	request.execute();
	return request;
};

aeon.utils.Xml.loadXmlDocument = function Xml$loadXmlDocument(url, onload, onerror)
{
	$assert.isString(url);
	$assert.isFunction(onload);

	function Xml$openXmlDocument$loaded(event) /**/
	{
		var request = event.source;
		var document = $xml.createXmlDocument(request.responseText);
		onload(document);
	}

	$xml.loadFile(url, Xml$openXmlDocument$loaded, onerror);
};

aeon.utils.Xml.createXmlDocument = function Xml$createXmlDocument(xmlText, freeThreaded)
{
	var xmlDocument;

	if (window.ActiveXObject)
	{
		if (freeThreaded)
			xmlDocument = new ActiveXObject(aeon.utils.Xml.PROGID_DOCUMENT_FT);
		else
			xmlDocument = new ActiveXObject(aeon.utils.Xml.PROGID_DOCUMENT);

		if (xmlText)
		{
			xmlDocument.loadXML(xmlText);
			if (xmlDocument.parseError != 0)
			{
				$log.error("Error loading xml text: {0}{1}.".format(
					xmlDocument.parseError.reason,
					xmlDocument.parseError.line
						? " (on line " + xmlDocument.parseError.line + ")"
						: String.EMPTY
				));
			}
		}
	}
	else
	{
		if (xmlText)
		{
			var parser = new DOMParser();
			xmlDocument = parser.parseFromString(xmlText, "text/xml");
			if (xmlDocument.documentElement.localName == "parsererror")
			{
				$log.error($xml.innerText(xmlDocument.documentElement));
			}
		}
		else
		{
			xmlDocument = document.implementation.createDocument(String.EMPTY, String.EMPTY, null);
		}
	}

	return xmlDocument;
};

aeon.utils.Xml.createXslProcessor = function Xml$createXslProcessor(xmlDocument)
{
	$assert.isDocument(xmlDocument);

	var processor = null;
	if (window.ActiveXObject)
	{
		var ftDocument = $xml.createXmlDocument(xmlDocument.xml, true);
		var template = new ActiveXObject($xml.PROGID_XSLTEMPLATE);
		template.stylesheet = ftDocument;
		processor = template.createProcessor();
	}
	else
	{
		processor = new XSLTProcessor();
		processor.importStylesheet(xmlDocument);
	}

	return processor;
};

aeon.utils.Xml.transformToDocument = function Xml$transformToDocument(xmlDocument, xslProcessor)
{
	$assert.isDocument(xmlDocument);
	$assert.isNotNull(xslProcessor);

	var result = null;
	if (window.ActiveXObject)
	{
		result = $xml.createXmlDocument();
		xslProcessor.input = xmlDocument;
		xslProcessor.output = result;
		xslProcessor.transform();
	}
	else
	{
		result = xslProcessor.transformToDocument(xmlDocument);
	}

	return result;
};

aeon.utils.Xml.transformToString = function Xml$transformToString(xmlDocument, xslProcessor)
{
	var resultDoc = $xml.transformToDocument(xmlDocument, xslProcessor);
	return $xml.serializeToString(resultDoc);
};

aeon.utils.Xml.NsResolver = function NsResolver(prefix)
{
	if (this.$ns == null)
		this.$ns = {};

	return this.$ns[prefix];
};

aeon.utils.Xml.NsResolver.prototype.addNamespace = function (prefix, namespace)
{
	$assert.isString(prefix);
	$assert.isString(namespace);

	if (this.$ns == null)
		this.$ns = {};

	this.$ns[prefix] = namespace;
};

/**
 * Global alias to <c>aeon.utils.Xml</c>
 * @type {aeon.utils.Xml}
 */
var $xml = aeon.utils.Xml;
