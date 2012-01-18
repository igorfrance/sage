<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="sage mod">

	<xsl:template match="mod:XmlTree">
		<xsl:param name="config" select="mod:config"/>
		<div class="xmltree">
			<xsl:attribute name="class">
				<xsl:text>xmltree</xsl:text>
				<xsl:if test="$config/mod:wrap = 'true'"> wrap</xsl:if>
				<xsl:if test="$config/mod:namespaces = 'true'"> namespaces</xsl:if>
			</xsl:attribute>
			<xsl:if test="count($config) = 0 or $config/mod:toolbar = 'true'">
				<xsl:apply-templates select="." mode="xmltree-toolbar"/>
			</xsl:if>
			<div class="xmlroot">
				<xsl:apply-templates select="mod:data/node()" mode="xmltree">
					<xsl:with-param name="level" select="1" />
				</xsl:apply-templates>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="mod:XmlTree" mode="xmltree-toolbar">
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

	<xsl:template match="*" mode="xmltree">
		<xsl:param name="level" select="1"/>
		<div class="element">
			<span class="markup">&#160;</span>
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
				<xsl:apply-templates mode="xmltree">
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
		<xsl:variable name="expandNode" select="ancestor::mod:XmlTree/mod:config/mod:expandLevels"/>
		<xsl:variable name="expandLevels">
			<xsl:choose>
				<xsl:when test="$expandNode">
					<xsl:apply-templates select="$expandNode/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="'*'"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
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
		<xsl:variable name="expandNode" select="ancestor::mod:XmlTree/mod:config/mod:expandLevels"/>
		<xsl:variable name="expandLevels">
			<xsl:choose>
				<xsl:when test="$expandNode">
					<xsl:apply-templates select="$expandNode/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="'*'"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="$expandLevels != '*' and $level &gt; $expandLevels">
			<xsl:attribute name="style">display:none</xsl:attribute>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>

