<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:basic="http://www.cycle99.com/schemas/sage/xslt/extensions/basic.xsd"
	xmlns:string="http://www.cycle99.com/schemas/sage/xslt/extensions/string.xsd"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="sage mod basic">

	<xsl:template match="mod:XmlTree">
		<xsl:param name="config" select="mod:config"/>
		<xsl:variable name="showWrapped" select="boolean($config/mod:show/@wrapped)"/>
		<xsl:variable name="showNamespaces" select="boolean($config/mod:show/@namespaces)"/>
		<xsl:variable name="showToolbar" select="boolean($config/mod:show/@toolbar)"/>
		<xsl:variable name="showHighlighted" select="count($config/mod:highlight/*) != 0"/>
		<xsl:variable name="phrases" select="$config/mod:text/mod:phrase"/>
		<div class="xmltree{basic:iif($showToolbar, ' with', '')}">
			<xsl:if test="$showToolbar">
				<div class="toolbar">
					<span class="group commands">
						<span class="command toggleall" title="{$phrases[@id='ToggleAll']}">
							<a href="javascript:;">
								<span><xsl:value-of select="$phrases[@id='ToggleAll']"/></span>
							</a>
						</span>
					</span>
					<span class="group switches">
						<span class="switch wrap {basic:iif($showWrapped, 'on', 'off')}" title="{$phrases[@id='ToggleWrapping']}">
							<a href="javascript:;">
								<span><xsl:value-of select="$phrases[@id='ToggleWrapping']"/></span>
							</a>
							<span class="state">&#160;
								<span class="on"></span>
								<span class="off"></span>
							</span>
						</span>
						<span class="switch namespaces {basic:iif($showNamespaces, 'on', 'off')}" title="{$phrases[@id='ToggleNamespaces']}">
							<a href="javascript:;">
								<span><xsl:value-of select="$phrases[@id='ToggleNamespaces']"/></span>
							</a>
							<span class="state">&#160;
								<span class="on"></span>
								<span class="off"></span>
							</span>
						</span>
						<xsl:if test="$showHighlighted">
							<span class="switch highlight {basic:iif($showHighlighted, 'on', 'off')}" title="{$phrases[@id='ToggleHighlighting']}">
								<a href="javascript:;">
									<span><xsl:value-of select="$phrases[@id='ToggleHighlighting']"/></span>
								</a>
								<span class="state">&#160;
									<span class="on"></span>
									<span class="off"></span>
								</span>
							</span>
						</xsl:if>
					</span>
				</div>
			</xsl:if>
			<div class="xmlroot">
				<xsl:attribute name="class">
					<xsl:text>xmlroot</xsl:text>
					<xsl:if test="$showWrapped"> wrap</xsl:if>
					<xsl:if test="$showNamespaces"> namespaces</xsl:if>
					<xsl:if test="$showHighlighted"> highlight</xsl:if>
				</xsl:attribute>
				<!--
					some kind of character that won't be stripped by the xmlwriter;
					this is needed to in order to force
					browsers to ignore whitespace in the following blocks
				-->&#8203;
				<xsl:apply-templates select="mod:data/node()" mode="xmltree">
					<xsl:with-param name="level" select="1" />
				</xsl:apply-templates>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="sage:literal" mode="xmltree" priority="100">
		<xsl:param name="level" select="1"/>
		<xsl:choose>
			<xsl:when test="ancestor::sage:literal">
				<div>
					<xsl:apply-templates select="." mode="class-attribute"/>
					<span class="switch_space">
						<xsl:text>&#160;</xsl:text>
					</span>
					<span class="markup open">
						<xsl:text>&lt;</xsl:text>
					</span>
					<span>
						<xsl:apply-templates select="." mode="xmltree-class"/>
						<xsl:value-of select="name()"/>
					</span>
					<xsl:apply-templates select="@*" mode="xmltree"/>
					<xsl:apply-templates select="." mode="xnamespace">
						<xsl:with-param name="level" select="$level" />
					</xsl:apply-templates>
					<span class="markup close">
						<xsl:text>/></xsl:text>
					</span>
				</div>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="node()" mode="xmltree"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="*" mode="xmltree">
		<xsl:param name="level" select="1"/>
		<div>
			<xsl:apply-templates select="." mode="class-attribute"/>
			<span class="switch_space">
				<xsl:text>&#160;</xsl:text>
			</span>
			<span class="markup open">
				<xsl:text>&lt;</xsl:text>
			</span>
			<span>
				<xsl:apply-templates select="." mode="xmltree-class"/>
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="xmltree"/>
			<xsl:apply-templates select="." mode="xnamespace">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup close">
				<xsl:text>/></xsl:text>
			</span>
		</div>
	</xsl:template>

	<xsl:template match="*[node()]" mode="xmltree">
		<xsl:param name="level" select="1"/>
		<div>
			<xsl:apply-templates select="." mode="class-attribute">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="xswitch">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup open">
				<xsl:text>&lt;</xsl:text>
			</span>
			<span>
				<xsl:apply-templates select="." mode="xmltree-class"/>
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="xmltree"/>
			<xsl:apply-templates select="." mode="xnamespace">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup close">
				<xsl:text>&gt;</xsl:text>
			</span>
			<div class="children">
				<xsl:apply-templates mode="xmltree">
					<xsl:with-param name="level" select="$level + 1" />
				</xsl:apply-templates>
			</div>
			<div class="close">
				<span class="switch_space">&#160;</span>
				<span class="markup open">
					<xsl:text>&lt;/</xsl:text>
				</span>
				<span>
					<xsl:apply-templates select="." mode="xmltree-class"/>
					<xsl:value-of select="name()"/>
				</span>
				<span class="markup close">
					<xsl:text>&gt;</xsl:text>
				</span>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*[*]" mode="xmltree" priority="20">
		<xsl:param name="level" select="1"/>
		<div>
			<xsl:apply-templates select="." mode="class-attribute">
					<xsl:with-param name="level" select="$level" />
				</xsl:apply-templates>
			<xsl:apply-templates select="." mode="xswitch">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup open"><xsl:text>&lt;</xsl:text></span>
			<span>
				<xsl:apply-templates select="." mode="xmltree-class"/>
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="xmltree"/>
			<xsl:apply-templates select="." mode="xnamespace">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">></span>
			<div class="children">
				<xsl:apply-templates mode="xmltree">
					<xsl:with-param name="level" select="$level + 1" />
				</xsl:apply-templates>
				<div class="close">
					<span class="switch_space">
						<xsl:text>&#160;</xsl:text>
					</span>
					<span class="markup">
						<xsl:text>&lt;/</xsl:text>
					</span>
					<span>
						<xsl:apply-templates select="." mode="xmltree-class"/>
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
		<xsl:variable name="wrapIndex" select="basic:isnull(ancestor::mod:XmlTree/mod:config/mod:wrapChars, 110)"/>
		<xsl:variable name="collapsible" select="string-length(text()) > $wrapIndex"/>
		<xsl:variable name="preserve"
			select="count(ancestor::mod:XmlTree/mod:config/mod:preserve/mod:element[text() = name(current())])"/>
		<div>
			<xsl:apply-templates select="." mode="class-attribute"/>
			<xsl:choose>
				<xsl:when test="$collapsible">
					<xsl:apply-templates select="." mode="xswitch">
						<xsl:with-param name="level" select="$level" />
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<span class="switch_space">&#160;</span>
				</xsl:otherwise>
			</xsl:choose>
			<span class="markup">
				<xsl:text>&lt;</xsl:text>
			</span>
			<span>
				<xsl:apply-templates select="." mode="xmltree-class"/>
				<xsl:value-of select="name()"/>
			</span>
			<xsl:apply-templates select="@*" mode="xmltree"/>
			<xsl:apply-templates select="." mode="xnamespace">
				<xsl:with-param name="level" select="$level" />
			</xsl:apply-templates>
			<span class="markup">></span>
			<div class="children{basic:iif($collapsible, '', ' inline')}">
				<span class="text">
					<xsl:apply-templates select="." mode="class-attribute">
						<xsl:with-param name="class">text</xsl:with-param>
					</xsl:apply-templates>
					<xsl:choose>
						<xsl:when test="$preserve = 0">
							<xsl:value-of select="."/>
						</xsl:when>
						<xsl:otherwise>
							<pre>
								<xsl:value-of select="string:trimSourceCode(.)"/>
								<br clear="all"/>
							</pre>
						</xsl:otherwise>
					</xsl:choose>

				</span>
				<div class="close">
					<xsl:if test="$collapsible">
						<span class="switch_space">&#160;</span>
					</xsl:if>
					<span class="markup">&lt;/</span>
					<span>
						<xsl:apply-templates select="." mode="xmltree-class"/>
						<xsl:value-of select="name()"/>
					</span>
					<span class="markup">></span>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="@*" mode="xmltree">
		<span>
			<xsl:apply-templates select="." mode="xmltree-class"/>
			<span class="markup">&#160;</span>
			<span class="attrName">
				<xsl:value-of select="name()"/>
			</span>
			<span class="markup">="</span>
			<span class="attrValue">
				<xsl:value-of select="."/>
			</span>
			<span class="markup">"</span>
		</span>
	</xsl:template>

	<xsl:template match="text()" mode="xmltree">
		<span class="text">
			<xsl:apply-templates select="." mode="class-attribute"/>
			<xsl:value-of select="."/>
		</span>
	</xsl:template>

	<xsl:template match="comment()" mode="xmltree">
		<xsl:param name="level" select="1"/>
		<xsl:variable name="wrapIndex" select="basic:isnull(ancestor::mod:XmlTree/mod:config/mod:wrapChars, 110)"/>
		<xsl:variable name="collapsible" select="string-length(.) > $wrapIndex or contains(., '&#xA;')"/>
		<xsl:variable name="collapsed" select="basic:iif(count(ancestor::mod:XmlTree/mod:config/mod:collapseComments) = 0, 0, 1)"/>
		<xsl:variable name="text" select="string:replace(string:trim(.), '^[\s\t]+', '', 2)"/>
		<div class="comment{basic:iif($collapsible, ' collapsible', '')}{basic:iif($collapsed, ' collapsed', '')}">
			<xsl:choose>
				<xsl:when test="$collapsible">
					<xsl:apply-templates select="." mode="xswitch">
						<xsl:with-param name="level" select="$level" />
						<xsl:with-param name="state" select="basic:iif($collapsed, 0, -1)" />
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<span class="switch_space">&#160;</span>
				</xsl:otherwise>
			</xsl:choose>
			<span class="markup open">&lt;&#33;&#45;&#45;</span>
			<pre data-title="{$text}"><xsl:value-of select="$text"/></pre>
			<span class="markup close">&#45;&#45;&gt;</span>
		</div>
	</xsl:template>

	<xsl:template match="processing-instruction()" mode="xmltree">
		<div class="element">
			<span class="switch_space">&#160;</span>
			<span class="markup open">&lt;?</span>
			<span class="pi">
				<xsl:value-of select="name(.)"/>
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of select="."/>
			</span>
			<span class="markup close">?></span>
		</div>
	</xsl:template>

	<xsl:template match="processing-instruction('xml')" mode="xmltree">
		<div class="element">
			<span class="switch_space">&#160;</span>
			<span class="markup open">&lt;?</span>
			<span class="pi">
				<xsl:text>xml </xsl:text>
				<xsl:for-each select="@*">
					<xsl:value-of select="name(.)"/>
					<xsl:text>="</xsl:text>
					<xsl:value-of select="."/>
					<xsl:text>" </xsl:text>
				</xsl:for-each>
			</span>
			<span class="markup close">?></span>
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
		<xsl:param name="state"/>
		<xsl:variable name="text">
			<xsl:choose>
				<xsl:when test="$state = 1">-</xsl:when>
				<xsl:when test="$state = 0">+</xsl:when>
				<xsl:otherwise>
					<xsl:variable name="expandLevels" select="basic:isnull(ancestor::mod:XmlTree/mod:config/mod:expandLevels/text(), '*')"/>
					<xsl:choose>
						<xsl:when test="$expandLevels = '*' or $level &lt;= number($expandLevels)">-</xsl:when>
						<xsl:otherwise>+</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<span class="switch"><xsl:value-of select="$text"/></span>
	</xsl:template>

	<xsl:template match="*" mode="xmltree-class">
		<xsl:attribute name="class">
			<xsl:text>nodeName </xsl:text>
			<xsl:text>element-</xsl:text>
			<xsl:value-of select="translate(name(), ':', '_')"/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="@*" mode="xmltree-class">
		<xsl:variable name="hlAttributes" select="ancestor::mod:XmlTree/mod:config/mod:highlight/mod:attribute"/>
		<xsl:variable name="hlNamespaces" select="ancestor::mod:XmlTree/mod:config/mod:highlight/mod:namespace"/>
		<xsl:attribute name="class">
			<xsl:text>attribute attribute-</xsl:text>
			<xsl:value-of select="translate(name(), ':', '_')"/>
			<xsl:if test="
				count($hlAttributes[.= name(current())]) != 0 or
				count($hlNamespaces[.= namespace-uri(current())]) != 0">
					<xsl:text> highlighted</xsl:text>
			</xsl:if>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="*" mode="class-attribute">
		<xsl:param name="class" select="'element'"/>
		<xsl:param name="level"/>
		<xsl:variable name="expandLevels" select="basic:isnull(ancestor::mod:XmlTree/mod:config/mod:expandLevels/text(), '*')"/>
		<xsl:variable name="hlElements" select="ancestor::mod:XmlTree/mod:config/mod:highlight/mod:element"/>
		<xsl:variable name="hlNamespaces" select="ancestor::mod:XmlTree/mod:config/mod:highlight/mod:namespace"/>
		<xsl:attribute name="class">
			<xsl:value-of select="$class"/>
			<xsl:if test="count(*) != 0 or count(comment()) != 0 or string-length(text()) > 110">
				<xsl:text> ce </xsl:text>
			</xsl:if>
			<xsl:text> </xsl:text>
			<xsl:if test="
				count($hlElements[.= name(current())]) != 0 or
				count($hlNamespaces[.= namespace-uri(current())]) != 0">
					<xsl:text> highlighted</xsl:text>
			</xsl:if>
			<xsl:if test="$expandLevels != '*' and $level &gt; number($expandLevels)">
				<xsl:text> collapsed</xsl:text>
			</xsl:if>
		</xsl:attribute>
	</xsl:template>

	<xsl:template name="split">
		<xsl:param name="text" select="."/>
		<xsl:param name="index" select="1"/>
		<xsl:param name="charcount" select="20"/>
		<xsl:param name="wrapelem" select="'div'"/>
		<xsl:variable name="fragment" select="substring($text, $index, $charcount)"/>
		<xsl:element name="{$wrapelem}">
			<xsl:value-of select="$fragment"/>
		</xsl:element>
		<xsl:if test="string-length(.) > ($index + $charcount)">
			<xsl:call-template name="split">
				<xsl:with-param name="text" select="$text"/>
				<xsl:with-param name="index" select="$index + $charcount"/>
				<xsl:with-param name="charcount" select="$charcount"/>
				<xsl:with-param name="wrapelem" select="$wrapelem"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>

