<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:param name="path"/>
	<xsl:param name="expandLevels" select="7"/>

	<xsl:output indent="no"/>

	<xsl:template match="/">
		<xsl:apply-templates select="." mode="xmltree"/>
	</xsl:template>

	<xsl:template match="/" mode="xmltree">
		<html xml:lang="en" lang="en">
			<head>
				<title>Xml Tree View</title>
				<xsl:apply-templates select="." mode="xmltree-styles"/>
			</head>
			<body>
				<xsl:apply-templates select="*" mode="xmlroot"/>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="*" mode="xmlroot">
		<div class="xmltree">
			<xsl:apply-templates select="." mode="xmltree-toolbar"/>
			<xsl:apply-templates select="." mode="tree-start"/>
			<xsl:apply-templates select="." mode="xmltree-script"/>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="xmltree-styles">
		<style>

			.xmltree
				{ font: 11px Verdana; }
			.xmltree *
				{ text-align: left; }
			.xmltree .xmlroot
				{ white-space: nowrap; padding: 10px 0; }
			.xmltree .toolbar
				{ font-size: 10px; padding: 4px 7px; overflow: hidden; background: #F5F5F5; border-radius: 4px; }
			.xmltree .toolbar a
				{ color: #8B8B8B; text-decoration: none; }
			.xmltree .toolbar a:hover
				{ color: #f00; }
			.xmltree .toolbar .left
				{ float: left; }
			.xmltree .toolbar .right
				{ float: right; }
			.xmltree .toolbar .toggler
				{ margin-right: 10px; }
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
				{ font-weight: bold;  }
			.xmltree .text
				{ color: #323232;  }
			.xmltree .markup
				{ color: blue; }
			.xmltree .comment .markup
				{ color: #888888; }
			.xmltree .commentText
				{ color: #888888; }
			.xmltree .pi
				{ color: blue; }
			.xmltree .ns
				{ color: blue; }
			.xmltree .switch
				{ display: inline-block; color: #FF0000; cursor: pointer; width: 8px; text-align: right; font-weight: bold; padding-right: 2px; }
			.xmltree .switch_space
				{ display: inline-block; width: 10px; }

			.xmltree.wrap .element span
				{	float: left; }
			.xmltree.wrap .element
				{ overflow: hidden; }
			.xmltree.wrap .element .children
				{ overflow: hidden; clear: left; }
			.xmltree.namespaces .nsattrib
				{	display: inline; }


			.xmltree .nsattrib
				{ font-weight: normal; display: none; }
			.xmltree .nsattrib .attrName,
			.xmltree .nsattrib .markup,
			.xmltree .nsattrib .attrValue
				{ font-weight: normal; color: #00f; }
			.xmltree .nsattrib a,
			.xmltree .nsattrib a:visited
				{ color: #00f; text-decoration: underline; font-weight: normal; }

		</style>
	</xsl:template>

	<xsl:template match="*" mode="xmltree-script">
		<script><xsl:comment><![CDATA[

			(function initializeTrees()
			{
				function onToggleAllClick()
				{
					var xmltree = findParentByClassName(this, "xmltree");
					var children = findChildrenByClassName(xmltree, "children", true);
					var elements = findChildrenByClassName(xmltree, "element", true);
					var allVisible = true;

					for (var i = 0; i < children.length; i++)
					{
						if (children[i].offsetHeight == 0)
						{
							allVisible = false;
							break;
						}
					}

					for (var i = 0; i < elements.length; i++)
					{
						if (allVisible)
							collapseElement(elements[i]);
						else
							expandElement(elements[i]);
					}
				}

				function onToggleNamespacesClick()
				{
					var xmltree = findParentByClassName(this, "xmltree");
					var toggler = findParentByClassName(this, "toggler");
					var state = findChildrenByClassName(toggler, "state")[0];

					if (containsClass(xmltree, "namespaces"))
					{
						removeClass(xmltree, "namespaces");
						setText(state, "off");
					}
					else
					{
						addClass(xmltree, "namespaces");
						setText(state, "on");
					}
				}

				function onToggleWordWrapClick()
				{
					var xmltree = findParentByClassName(this, "xmltree");
					var toggler = findParentByClassName(this, "toggler");
					var state = findChildrenByClassName(toggler, "state")[0];

					if (containsClass(xmltree, "wrap"))
					{
						removeClass(xmltree, "wrap");
						setText(state, "off");
					}
					else
					{
						addClass(xmltree, "wrap");
						setText(state, "on");
					}
				}

				function onSwitchClick()
				{
					for (var i = 0; i < this.parentNode.childNodes.length; i++)
					{
						var childNode = this.parentNode.childNodes[i];
						var className = childNode.className;
						if (className == "children")
						{
							if (childNode.offsetHeight)
								collapseElement(this.parentNode);
							else
								expandElement(this.parentNode);
						}
					}
				}

				function containsClass(element)
				{
					if (element == null || element.className == null || arguments.length < 2)
						return false;

					for (var i = 1; i < arguments.length; i++)
					{
						var rxp = new RegExp("\\b" + arguments[i] + "\\b");
						if (!element.className.match(rxp))
							return false;
					}

					return true;
				}

				function addClass(element)
				{
					if (element == null || element.className == null || arguments.length < 2)
						return;

					for (var i = 1; i < arguments.length; i++)
					{
						var className = arguments[i];
						if (!containsClass(element, className))
							element.className += " " + className;
					}
				}

				function removeClass(element)
				{
					if (element == null || element.className == null || arguments.length < 2)
						return;

					for (var i = 1; i < arguments.length; i++)
					{
						var className = arguments[i];
						var rxp = new RegExp("(\\b)" + className + "(\\b)", "g");
						element.className = element.className.replace(rxp, "$1$2");
					}
				}

				function getStyle(element, property)
				{
					if (window.getComputedStyle)
						return window.getComputedStyle(element, null)[property];

					return element.currentStyle[property];
				}

				function findParentByClassName(element, className)
				{
					var parent = element;
					while (parent && parent.className && parent.className.indexOf(className) == -1)
					{
						parent = parent.parentNode;
					}

					return parent;
				}

				function findChildrenByClassName(element, className, recursive)
				{
					var result = [];
					for (var i = 0; i < element.childNodes.length; i++)
					{
						if (element.childNodes[i].nodeType != 1)
							continue;

						if (element.childNodes[i].className.indexOf(className) != -1)
							result.push(element.childNodes[i]);

						var children = findChildrenByClassName(element.childNodes[i], className, recursive);
						for (var j = 0; j < children.length; j++)
						{
							result.push(children[j]);
						}
					}

					return result;
				}

				function setText(element, text)
				{
					element.innerText = element.textContent = text;
				}

				function expandElement(element)
				{
					for (var i = 0; i < element.childNodes.length; i++)
					{
						var className = element.childNodes[i].className;
						if (className == "children")
							element.childNodes[i].style.display = "block";
						if (className == "switch")
							setText(element.childNodes[i], "-");
					}
				}

				function collapseElement(element)
				{
					for (var i = 0; i < element.childNodes.length; i++)
					{
						var className = element.childNodes[i].className;
						if (className == "children")
							element.childNodes[i].style.display = "none";
						if (className == "switch")
							setText(element.childNodes[i], "+");
					}
				}

				var divs = document.getElementsByTagName("div");
				for (var i = 0; i < divs.length; i++)
				{
					if (divs[i].className.indexOf("xmltree") != -1)
					{
						var spans = divs[i].getElementsByTagName("span");
						var links = divs[i].getElementsByTagName("a");

						for (var j = 0; j < spans.length; j++)
						{
							if (spans[j].className.indexOf("switch") != -1)
							{
								spans[j].onclick = onSwitchClick;
							}
						}

						for (var j = 0; j < links.length; j++)
						{
							if (links[j].className.indexOf("wrap") != -1)
								links[j].onclick = onToggleWordWrapClick;
							if (links[j].className.indexOf("namespaces") != -1)
								links[j].onclick = onToggleNamespacesClick;
							if (links[j].className.indexOf("toggleall") != -1)
								links[j].onclick = onToggleAllClick;
						}
					}
				}

			})();

		]]></xsl:comment></script>
	</xsl:template>

	<xsl:template match="*" mode="xmltree-toolbar">
		<div class="toolbar">
			<span class="group left">
				<span class="toggler">
					<a href="javascript:;" class="wrap">Wrap: </a>
					<span class="state">off</span>
				</span>
				<span class="toggler">
					<a href="javascript:;" class="namespaces">Namespaces: </a>
					<span class="state">off</span>
				</span>
			</span>
			<span class="group right">
				<a href="javascript:;" class="toggleall">toggle all</a>
			</span>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="tree-start">
		<div class="xmlroot">
			<xsl:apply-templates select="." mode="xmltree">
				<xsl:with-param name="level" select="1" />
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="xmltree">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<span class="switch_space">
				<xsl:text>&#160;</xsl:text>
			</span>
			<span class="markup">
				<xsl:text>&lt;</xsl:text>
			</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="xmltree"/>
			<xsl:apply-templates select="." mode="xnamespace">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">
				<xsl:text>/></xsl:text>
			</span>
		</div>
	</xsl:template>

	<xsl:template match="*[node()]" mode="xmltree">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<span class="markup">&#160;</span>
			<xsl:apply-templates select="." mode="xswitch">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">
				<xsl:text>&lt;</xsl:text>
			</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="xmltree"/>
			<xsl:apply-templates select="." mode="xnamespace">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">
				<xsl:text>&gt;</xsl:text>
			</span>
			<div class="children">
				<xsl:apply-templates select="." mode="xdisplay">
					<xsl:with-param name="level" select="$level" />
				</xsl:apply-templates>
				<xsl:apply-templates mode="xmltree">
					<xsl:with-param name="level" select="$level + 1" />
				</xsl:apply-templates>
				<div>
					<span class="switch_space">&#160;</span>
					<span class="markup">
						<xsl:text>&lt;/</xsl:text>
					</span>
					<span class="nodeName element-{translate(name(), ':', '_')}">
						<xsl:value-of select="name()"/>
					</span>
					<span class="markup">
						<xsl:text>&gt;</xsl:text>
					</span>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*[*]" mode="xmltree" priority="20">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<xsl:apply-templates select="." mode="xswitch">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup"><xsl:text>&lt;</xsl:text></span>
			<span class="nodeName element-{translate(name(), ':', '_')}"><xsl:value-of select="name()"/></span>
			<xsl:apply-templates select="@*" mode="xmltree"/>
			<xsl:apply-templates select="." mode="xnamespace">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">
				<xsl:text>&gt;</xsl:text>
			</span>
			<div class="children">
				<xsl:apply-templates select="." mode="xdisplay">
					<xsl:with-param name="level" select="$level" />
				</xsl:apply-templates>
				<xsl:apply-templates select="node()" mode="xmltree">
					<xsl:with-param name="level" select="$level + 1" />
				</xsl:apply-templates>
				<div>
					<span class="switch_space">
						<xsl:text>&#160;</xsl:text>
					</span>
					<span class="markup">
						<xsl:text>&lt;/</xsl:text>
					</span>
					<span class="nodeName element-{translate(name(), ':', '_')}">
						<xsl:value-of select="name()"/>
					</span>
					<span class="markup">
						<xsl:text>></xsl:text>
					</span>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*[text() and not (comment() or processing-instruction())]" mode="xmltree">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<span class="switch_space">&#160;</span>
			<span class="markup">
				<xsl:text>&lt;</xsl:text>
			</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="xmltree"/>
			<xsl:apply-templates select="." mode="xnamespace">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">></span>
			<span class="text">
				<xsl:value-of select="."/>
			</span>
			<span class="markup">&lt;/</span>
			<span class="nodeName element-{translate(name(), ':', '_')}">
				<xsl:value-of select="name()"/>
			</span>
			<span class="markup">></span>
		</div>
	</xsl:template>

	<xsl:template match="@*" mode="xmltree">
		<span class="markup">&#160;</span>
		<span class="attrName attribute-{name()}">
			<xsl:value-of select="name()"/>
		</span>
		<span class="markup">="</span>
		<span class="attrValue">
			<xsl:value-of select="."/>
		</span>
		<span class="markup">"</span>
	</xsl:template>

	<xsl:template match="@*[contains(name(), ':')]" mode="xmltree">
		<span class="markup">&#160;</span>
		<span class="attrName attribute-{translate(name(), ':', '_')}">
			<xsl:value-of select="name()"/>
		</span>
		<span class="markup">="</span>
		<span class="attrValue">
			<xsl:value-of select="."/>
		</span>
		<span class="markup">"</span>
	</xsl:template>

	<xsl:template match="text()" mode="xmltree">
		<span class="text">
			<xsl:value-of select="."/>
		</span>
	</xsl:template>

	<xsl:template match="comment()" mode="xmltree">
		<xsl:param name="level" select="1"/>
		<div class="comment">
			<xsl:apply-templates select="." mode="xswitch">
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

	<xsl:template match="processing-instruction()" mode="xmltree">
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

	<xsl:template match="processing-instruction('xml')" mode="xmltree">
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

	<xsl:template match="node()" mode="xnamespace">
		<xsl:param name="level" select="1"/>
		<xsl:variable name="thisNamespace" select="namespace-uri(.)"/>
		<xsl:variable name="parentNamespace" select="namespace-uri(parent::*)"/>
		<xsl:if test="$thisNamespace and ($level = 1 or $parentNamespace != $thisNamespace)">
			<span class="nsattrib">
				<span class="markup">&#160;</span>
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
					<a href="{namespace-uri(.)}" target="_blank">
						<xsl:value-of select="namespace-uri(.)"/>
					</a>
				</span>
				<span class="markup">"</span>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="node()" mode="xswitch">
		<xsl:param name="level"/>
		<xsl:choose>
			<xsl:when test="parent::sage:resources or parent::sage:model">
				<span class="switch">+</span>
			</xsl:when>
			<xsl:when test="$expandLevels = '*' or $level &lt;= $expandLevels">
				<span class="switch">-</span>
			</xsl:when>
			<xsl:otherwise>
				<span class="switch">+</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="node()" mode="xdisplay">
		<xsl:param name="level"/>
		<xsl:choose>
			<xsl:when test="$expandLevels != '*' and $level &gt; $expandLevels">
				<xsl:attribute name="style">display:none</xsl:attribute>
			</xsl:when>
			<xsl:when test="parent::sage:resources or parent::sage:model">
				<xsl:attribute name="style">display:none</xsl:attribute>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
