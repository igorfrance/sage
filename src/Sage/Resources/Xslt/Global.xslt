<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:kelp="http://www.cycle99.com/projects/kelp"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="sage mod kelp xhtml">

	<xsl:include href="sage://resources/modules.xslt" />
	<xsl:include href="sageresx://sage/resources/xslt/tree.xsl" />
	<xsl:include href="sageresx://sage/resources/xslt/logic.xsl" />

	<xsl:variable name="view" select="/sage:view"/>
	<xsl:variable name="request" select="$view/sage:request"/>
	<xsl:variable name="response" select="$view/sage:response"/>
	<xsl:variable name="address" select="$view/sage:request/sage:address"/>
	<xsl:variable name="useragent" select="$view/sage:request/sage:useragent"/>

	<xsl:output method="xml" version="1.0" standalone="yes" omit-xml-declaration="yes"
		encoding="utf-8" media-type="text/xml" indent="yes"
		doctype-system="about:legacy-compat"/>

	<xsl:template match="sage:view">
		<xsl:apply-templates select="sage:response/sage:model/node()"/>
	</xsl:template>

	<xsl:template match="sage:basehref">
		<base href="{/sage:view/sage:request/@basehref}"/>
	</xsl:template>

	<xsl:template match="xhtml:html">
		<html>
			<xsl:apply-templates select="@*"/>
			<xsl:if test="$request/@developer = 1">
				<xsl:attribute name="data-developer">1</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="node()"/>
		</html>
	</xsl:template>

	<xsl:template match="xhtml:script[starts-with(@src, 'kelp://')]">
		<xsl:for-each select="document(@src)/*/kelp:resource">
			<xsl:choose>
				<xsl:when test="@exists = 'false'">
					<xsl:comment>
						File not found: <xsl:value-of select="@path"/>
					</xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<script type="text/javascript" language="javascript" src="{@src}"></script>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="xhtml:link[starts-with(@href, 'kelp://')]">
		<xsl:for-each select="document(@href)/*/kelp:resource">
			<xsl:choose>
				<xsl:when test="@exists = 'false'">
					<xsl:comment>
						File not found: <xsl:value-of select="@path"/>
					</xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<link type="text/css" rel="stylesheet" href="{@src}" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="*">
		<xsl:element name="{name()}" namespace="{namespace-uri()}">
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
		<xsl:value-of select="normalize-space(.)"/>
	</xsl:template>

</xsl:stylesheet>
