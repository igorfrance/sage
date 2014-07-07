<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:tpl="http://www.cycle99.com/schemas/sage/extensions/templating.xsd">

	<xsl:variable name="templates" select="document('sageres:///assets/configuration/templates/global.html')/tpl:dictionary"/>

	<xsl:template match="*[@tpl:name]">
		<xsl:variable name="groupName" select="substring-before(@tpl:name, '.')"/>
		<xsl:variable name="templateName" select="substring-after(@tpl:name, '.')"/>
		<xsl:variable name="template" select="$templates[@name=$groupName]/tpl:template[@name=$templateName]"/>
		<xsl:choose>
			<xsl:when test="count($template) != 0">
				<xsl:apply-templates select="$template" mode="apply-template">
					<xsl:with-param name="current" select="current()"/>
				</xsl:apply-templates>
			</xsl:when>
			<xsl:otherwise>
				<xsl:if test="$isDebugRequest">
					<strong class="error">Template '<xsl:value-of select="@tpl:name"/>' doesn't exist.</strong>
				</xsl:if>
				<xsl:apply-templates select="." mode="copy"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@tpl:name"/>

	<xsl:template match="tpl:template" mode="apply-template">
		<xsl:param name="current" select="."/>
		<xsl:param name="template" select="*[name() = name($current)]"/>
		<xsl:element name="{name($current)}" namespace="{namespace-uri($current)}">
			<xsl:apply-templates select="$template/@*" mode="apply-template"/>
			<xsl:apply-templates select="$current/@*" mode="apply-template"/>
			<xsl:apply-templates select="$template/node()" mode="apply-template">
				<xsl:with-param name="current" select="$current"/>
			</xsl:apply-templates>
		</xsl:element>
	</xsl:template>

	<xsl:template match="tpl:content" mode="apply-template">
		<xsl:param name="current" select="."/>
		<xsl:apply-templates select="$current/node()" mode="apply-template"/>
	</xsl:template>

	<xsl:template match="*" mode="apply-template">
		<xsl:element name="{name()}" namespace="{namespace-uri()}">
			<xsl:apply-templates select="@*" mode="apply-template"/>
			<xsl:apply-templates select="node()" mode="apply-template"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@tpl:name" mode="apply-template"/>

	<xsl:template match="@*" mode="apply-template">
		<xsl:attribute name="{name()}" namespace="{namespace-uri()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()" mode="apply-template">
		<xsl:param name="current" select="."/>
		<xsl:value-of select="."/>
	</xsl:template>


</xsl:stylesheet>
