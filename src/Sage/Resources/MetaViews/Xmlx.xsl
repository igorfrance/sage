<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:param name="path"/>

 	<xsl:template match="/">
		<xsl:apply-templates select="." mode="tree"/>
	</xsl:template>

	<xsl:template match="/" mode="tree">
		<html xml:lang="en" lang="en">
			<head>
				<title>Xml Tree View</title>
				<xsl:apply-templates select="*" mode="tree-headers"/>
			</head>
			<body>
				<xsl:apply-templates select="*" mode="tree-root"/>
			</body>
		</html>
	</xsl:template>

	<xsl:variable name="expand-levels" select="7"/>

	<xsl:template match="*" mode="tree-headers">
		<style>

			.xmltree *
				{ -moz-box-sizing: border-box; box-sizing: border-box; text-align: left; }
			.xmltree
				{ font: 11px Verdana; }
			.xmltree .treeRoot
				{ white-space: nowrap; }
			.xmltree A, .element A:visited
				{ color: blue; font-weight: normal; }
			.xmltree pre
				{ margin: 0px; }
			.xmltree .wrapctrl
				{ cursor: pointer; color: blue; }
			.xmltree .comment
				{ margin-left: 22px; color: #A7A7A7; }
			.xmltree .element .element
				{ margin-left: 15px; }
			.xmltree .nodeName
				{ color: #CC0000; }
			.xmltree .attrName
				{ color: #CC0000; }
			.xmltree .attrValue
				{ font-weight: bold; }
			.xmltree .--text
				{ font-weight: bold; }
			.xmltree .markup
				{ color: blue; }
			.xmltree .comment .markup
				{ color:#888888; }
			.xmltree .commentText
				{ color:#888888; }
			.xmltree .pi
				{ color: blue; }
			.xmltree .ns
				{ color: blue; }
			.xmltree .switch, .xmltree .switch_space
				{ color: #FF0000; cursor: pointer; font-family: Courier New; }
			.xmltree .controls
				{ font-size: 10px; font-family: verdana, padding: 2px; background-color: #e2e2e2; color: black;
				  margin-bottom: 20px; padding: 2px; height: 16px; }

			.xmltree .nsattrib
				{ font-weight: normal; display: none; }
			.xmltree .nsattrib .attrName,
			.xmltree .nsattrib .markup,
			.xmltree .nsattrib .attrValue
				{ font-weight: normal; color: #00f; }

		</style>
		<script>
			<xsl:comment>
				<![CDATA[

			var namespacesVisible = false;

			function switchNode()
			{
				var parent = this.parentNode;
				var children = parent.getElementsByTagName("DIV")[0];
				if (children.offsetHeight)
					toggleOff(parent);
				else
					toggleOn(parent);
			}

			function toggleOn(node)
			{
				for (var i = 0; i < node.childNodes.length; i++)
				{
					var c = node.childNodes[i];
					if (!c.tagName)
						continue;

					if (c.className.indexOf("children") != -1)
						c.style.display = "block";

					if (c.className.indexOf("switch") != -1)
						c.innerText = c.textContent = "-";
				}
			}

			function toggleOff(node)
			{
				for (var i = 0; i < node.childNodes.length; i++)
				{
					var c = node.childNodes[i];
					if (!c.tagName)
						continue;

					if (c.className.indexOf("children") != -1)
						c.style.display = "none";

					if (c.className.indexOf("switch") != -1)
						c.innerText = c.textContent = "+";
				}
			}

			function expandAllNodes()
			{
				var nodes = document.getElementsByTagName("div");
				for (var i = 0; i < nodes.length; i++)
				{
					if (nodes[i].className.indexOf("element") != -1)
					{
						toggleOn(nodes[i]);
					}
				}
			}

			function toggleNamespaceAttributes()
			{
				var nodes = document.getElementsByTagName("span");
				namespacesVisible = !namespacesVisible;
				for (var i = 0; i < nodes.length; i++)
				{
					if (nodes[i].className.indexOf("nsattrib") != -1)
					{
						nodes[i].style.display = namespacesVisible ? "inline" : "none";
					}
				}
			}

			function toggleWordWrap(on)
			{
				if (on != undefined)
					var wordwrap = on;
				else
					var wordwrap = window.wordwrap = !window.wordwrap;

				var whitespace = wordwrap ? "normal" : "nowrap";

				var elements = document.getElementsByTagName("DIV");
				for (var i = 0; i < elements.length; i++)
				{
					if (elements[i].className == "treeRoot")
					{
						elements[i].style.whiteSpace = whitespace;
						elements[i].style.display = "block";
					}
				}

				document.cookie = "wordwrap=" + wordwrap;

				var wordWrapOn = document.getElementById("wordWrapOn");
				var wordWrapOff = document.getElementById("wordWrapOff");
				if (wordWrapOn && wordWrapOff)
				{
					if (wordwrap)
					{
						document.getElementById("wordWrapOn").style.fontWeight = "bold";
						document.getElementById("wordWrapOff").style.fontWeight = "normal";
					}
					else
					{
						document.getElementById("wordWrapOn").style.fontWeight = "normal";
						document.getElementById("wordWrapOff").style.fontWeight = "bold";
					}
				}
			}

			function wrap_on()  { toggleWordWrap(true);  }
			function wrap_off() { toggleWordWrap(false); }

			window_onload = function ()
			{
				window.wordwrap = false;
				var cookie = String(document.cookie);
				if (cookie.match(/wordwrap=(true|false)/))
					window.wordwrap = RegExp.$1 == "true" ? true : false;

				toggleWordWrap(window.wordwrap);
				var wordWrapOn = document.getElementById("wordWrapOn");
				var wordWrapOff = document.getElementById("wordWrapOff");
				var expandAll = document.getElementById("expandAll");
				var toggleNamespaces = document.getElementById("toggleNamespaces");
				if (wordWrapOn)
					wordWrapOn.onclick = wrap_on;
				if (wordWrapOff)
					wordWrapOff.onclick = wrap_off;
				if (expandAll)
					expandAll.onclick = expandAllNodes;
				if (toggleNamespaces)
					toggleNamespaces.onclick = toggleNamespaceAttributes;

				var spans = document.getElementsByTagName("SPAN");
				for (var i = 0; i < spans.length; i++)
				{
					if (spans[i].className == "switch")
						spans[i].onclick = switchNode;
				}
			}

			if (window.util)
				evt.addHandler(window, "onload", window_onload);
			else
				window.onload = window_onload;

		]]>
			</xsl:comment>
		</script>
	</xsl:template>

	<xsl:template match="*" mode="tree-root">
		<div class="xmltree">
			<div class="controls">
				<span style="float: left">
					[<span class="wrapctrl" id="wordWrapOn">wrap</span>]
					[<span class="wrapctrl" id="wordWrapOff">no wrap</span>]
				</span>
				<span style="float: right">
					[<a href="javascript:;" id="toggleNamespaces">toggle namespaces</a>]
					[<a href="javascript:;" id="expandAll">expand all</a>]
					[<a href="javascript:location.reload()">refresh</a>]
					[<a href="javascript:history.back()">back</a>]
				</span>
			</div>
			<div class="treeRoot">
				<xsl:apply-templates select="." mode="tree-start"/>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="tree-start">
		<div class="xmltree xmlroot">
			<xsl:apply-templates select="." mode="tree">
				<xsl:with-param name="level" select="1" />
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="tree">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<xsl:text>&#160;</xsl:text>
			<span class="switch_space">&#160;</span>
			<span class="markup">&lt;</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="tree"/>
			<xsl:apply-templates select="." mode="namespace"/>
			<span class="markup">/></span>
		</div>
	</xsl:template>

	<xsl:template match="*[node()]" mode="tree">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<xsl:text>&#160;</xsl:text>
			<xsl:apply-templates select="." mode="switch">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">&lt;</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="tree"/>
			<xsl:apply-templates select="." mode="namespace"/>
			<span class="markup">></span>
			<div class="children">
				<xsl:apply-templates select="." mode="display">
					<xsl:with-param name="level" select="$level" />
				</xsl:apply-templates>
				<xsl:apply-templates mode="tree">
					<xsl:with-param name="level" select="$level + 1" />
				</xsl:apply-templates>
				<div>
					<span class="switch_space">&#160;</span>
					<span class="markup">&lt;/</span>
					<span class="nodeName element-{translate(name(), ':', '_')}">
						<xsl:value-of select="name()"/>
					</span>
					<span class="markup">></span>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*[*]" mode="tree" priority="20">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<xsl:text>&#160;</xsl:text>
			<xsl:apply-templates select="." mode="switch">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">&lt;</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="tree"/>
			<xsl:apply-templates select="." mode="namespace"/>
			<span class="markup">></span>
			<div class="children">
				<xsl:apply-templates select="." mode="display">
					<xsl:with-param name="level" select="$level" />
				</xsl:apply-templates>
				<xsl:apply-templates mode="tree">
					<xsl:with-param name="level" select="$level + 1" />
				</xsl:apply-templates>
				<div>
					<span class="switch_space">&#160;</span>
					<span class="markup">&lt;/</span>
					<span class="nodeName element-{translate(name(), ':', '_')}">
						<xsl:value-of select="name()"/>
					</span>
					<span class="markup">></span>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*[text() and not (comment() or processing-instruction())]" mode="tree">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<span class="switch_space">&#160;</span>
			<span class="markup">&lt;</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="tree"/>
			<xsl:apply-templates select="." mode="namespace"/>
			<span class="markup">></span>
			<span class="--text">
				<xsl:value-of select="."/>
			</span>
			<span class="markup">&lt;/</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<span class="markup">></span>
		</div>
	</xsl:template>

	<xsl:template match="@*" mode="tree">
		<xsl:text>&#160;</xsl:text>
		<span class="attrName attribute-{translate(name(), ':', '_')}">
			<xsl:value-of select="name()"/>
		</span>
		<span class="markup">="</span>
		<span class="attrValue">
			<xsl:value-of select="."/>
		</span>
		<span class="markup">"</span>
	</xsl:template>

	<xsl:template match="text()" mode="tree">
		<span class="--text">
			<xsl:value-of select="."/>
		</span>
	</xsl:template>

	<xsl:template match="comment()" mode="tree">
		<xsl:param name="level" select="1"/>
		<div class="comment">
			<xsl:apply-templates select="." mode="switch">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">&lt;&#33;&#45;&#45;</span>
			<div class="commentText">
				<pre>
					<xsl:value-of select="."/>
				</pre>
			</div>
			<span class="switch_space">&#160;</span>
			<span class="markup">&#45;&#45;&gt;</span>
		</div>
	</xsl:template>

	<xsl:template match="processing-instruction()" mode="tree">
		<div class="element">
			<span class="switch_space">&#160;</span>
			<span class="markup">&lt;?</span>
			<span class="pi">
				<xsl:value-of select="name(.)"/>
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of select="."/>
			</span>
			<span class="markup">?></span>
		</div>
	</xsl:template>

	<xsl:template match="processing-instruction('xml')" mode="tree">
		<div class="element">
			<span class="switch_space">&#160;</span>
			<span class="markup">&lt;?</span>
			<span class="pi">
				<xsl:text>xml </xsl:text>
				<xsl:for-each select="@*">
					<xsl:value-of select="name(.)"/>
					<xsl:text>="</xsl:text>
					<xsl:value-of select="."/>
					<xsl:text>" </xsl:text>
				</xsl:for-each>
			</span>
			<span class="markup">?></span>
		</div>
	</xsl:template>

	<xsl:template match="node()" mode="namespace">
		<xsl:if test="namespace-uri(.)">
			<span class="nsattrib">
				<xsl:text>&#160;</xsl:text>
				<span class="attrName">
					<xsl:choose>
						<xsl:when test="contains(name(), ':')">
							<xsl:text>xmlns:</xsl:text>
							<xsl:value-of select="substring-before(name(), ':')"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>xmlns</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</span>
				<span class="markup">="</span>
				<span class="attrValue">
					<xsl:value-of select="namespace-uri(.)"/>
				</span>
				<span class="markup">"</span>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="@href" mode="tree">
		<xsl:text>&#160;</xsl:text>
		<span class="attrName attribute-href">
			<xsl:value-of select="name(.)"/>
		</span>
		<span class="markup">="</span>
		<span class="attrValue">
			<xsl:choose>
				<xsl:when test="string-length($path) and (name(parent::node()) = 'xsl:include' or name(parent::node()) = 'xsl:import')">
					<a href="?{$path}{.}">
						<xsl:value-of select="."/>
					</a>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="."/>
				</xsl:otherwise>
			</xsl:choose>
		</span>
		<span class="markup">"</span>
	</xsl:template>

	<xsl:template match="node()" mode="switch">
		<xsl:param name="level"/>
		<xsl:choose>
			<xsl:when test="$level &lt;= $expand-levels">
				<span class="switch">-</span>
			</xsl:when>
			<xsl:otherwise>
				<span class="switch">+</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="node()" mode="display">
		<xsl:param name="level"/>
		<xsl:if test="$level &gt; $expand-levels">
			<xsl:attribute name="style">display:none</xsl:attribute>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
