<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:x="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml">
	
	<xsl:variable name="request" select="//sage:request"/>

	<xsl:template match="@data-thread">
		<xsl:attribute name="data-thread">
			<xsl:value-of select="$request/@thread"/>
		</xsl:attribute>
	</xsl:template>
	
	<xsl:template match="@data-session">
		<xsl:attribute name="data-thread">
			<xsl:value-of select="$request/sage:session/@id"/>
		</xsl:attribute>
	</xsl:template>
	
	<xsl:template match="*">
		<xsl:element name="{name()}">
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()">
		<xsl:value-of select="."/>
	</xsl:template>

</xsl:stylesheet>