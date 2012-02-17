<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:template match="mod:DocumentationBoxes">
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/site:navigation"/>
		<xsl:variable name="sections" select="$navigation/ul/li/ul/li[ul]"/>

		<xsl:for-each select="$sections[position() mod 4 = 1]">
			<xsl:apply-templates select="." mode="documentation-box">
				<xsl:with-param name="step" select="position()"/>
				<xsl:with-param name="count" select="4"/>
				<xsl:with-param name="sections" select="$sections"/>
			</xsl:apply-templates>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="li" mode="documentation-box">
		<xsl:param name="sections"/>
		<xsl:param name="step"/>
		<xsl:param name="count" select="4"/>
		<xsl:param name="min" select="(($step - 1) * $count) + 1"/>
		<xsl:param name="max" select="$min + $count"/>

		<div class="documentation-row">
			<xsl:for-each select="$sections[position() >= $min and position() &lt; $max]">
				<div class="section">
					<h5>
						<xsl:apply-templates select="a | text()"/>
					</h5>
					<div class="content">
						<xsl:apply-templates select="ul"/>
					</div>
				</div>
			</xsl:for-each>
		</div>

	</xsl:template>

</xsl:stylesheet>
