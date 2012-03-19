<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns:site="http://sage.cycle99.com/"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd">

	<xsl:template match="mod:Navigation">
		<xsl:variable name="currentHref" select="normalize-space(mod:config/mod:current/text())"/>
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/site:navigation"/>

		<div class="navigation">
			<xsl:apply-templates select="$navigation/ul/li/*[position() > 1]" mode="navigation-tree">
				<xsl:with-param name="current" select="currentHref"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<xsl:template match="li" mode="navigation-tree">
		<xsl:param name="current"/>
		<li>
			<xsl:if test="a/@href = $current or count(ul) != 0 or @class">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
					<xsl:if test="a/@href = $current">
						<xsl:text> current</xsl:text>
					</xsl:if>
					<xsl:if test="count(ul) != 0">
						<xsl:text> expandable</xsl:text>
					</xsl:if>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="@*[name() != 'class']" mode="navigation-tree"/>
			<xsl:apply-templates select="node()" mode="navigation-tree">
				<xsl:with-param name="current" select="$current"/>
			</xsl:apply-templates>
		</li>
	</xsl:template>

	<xsl:template match="*" mode="navigation-tree">
		<xsl:element name="{name()}">
			<xsl:apply-templates select="@*" mode="navigation-tree"/>
			<xsl:apply-templates select="node()" mode="navigation-tree"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*" mode="navigation-tree">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()" mode="navigation-tree">
		<xsl:value-of select="."/>
	</xsl:template>

</xsl:stylesheet>
