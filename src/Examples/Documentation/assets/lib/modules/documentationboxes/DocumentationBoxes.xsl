<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns:basic="http://www.cycle99.com/projects/sage/xslt/extensions/basic"
	xmlns:x="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="x mod sage basic">

	<xsl:template match="mod:DocumentationBoxes">
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/site:navigation"/>
		<xsl:variable name="sections" select="$navigation/x:ul/x:li/x:ul/x:li[x:ul]"/>

		<xsl:for-each select="$sections[position() mod 4 = 1]">
			<xsl:apply-templates select="." mode="documentation-box">
				<xsl:with-param name="step" select="position()"/>
				<xsl:with-param name="count" select="4"/>
				<xsl:with-param name="sections" select="$sections"/>
			</xsl:apply-templates>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="x:li" mode="documentation-box">
		<xsl:param name="sections"/>
		<xsl:param name="step"/>
		<xsl:param name="count" select="4"/>
		<xsl:param name="min" select="(($step - 1) * $count) + 1"/>
		<xsl:param name="max" select="$min + $count"/>

		<div class="documentation-row">
			<xsl:for-each select="$sections[position() >= $min and position() &lt; $max]">
				<div class="section">
					<h5>
						<xsl:apply-templates select="x:a | x:span" mode="documentation-tree"/>
					</h5>
					<div class="content">
						<xsl:apply-templates select="x:ul" mode="documentation-tree"/>
					</div>
				</div>
			</xsl:for-each>
		</div>

	</xsl:template>

	<xsl:template match="x:span" mode="documentation-tree">
		<xsl:element name="{name()}">
			<xsl:apply-templates select="@*" mode="documentation-tree"/>
			<xsl:apply-templates select="node()" mode="documentation-tree"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="*" mode="documentation-tree">
		<xsl:element name="{name()}">
			<xsl:apply-templates select="@*" mode="documentation-tree"/>
			<xsl:apply-templates select="node()" mode="documentation-tree"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*" mode="documentation-tree">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()" mode="documentation-tree">
		<xsl:value-of select="basic:isnull(parent::node()/@data-shortTitle, .)"/>
	</xsl:template>

</xsl:stylesheet>
