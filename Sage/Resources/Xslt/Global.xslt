<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:kelp="http://www.cycle99.com/projects/kelp"
	xmlns:set="http://www.cycle99.com/schemas/sage/xslt/extensions/set.xsd"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="sage mod kelp set xhtml">

	<xsl:include href="sageres://modules.xslt" />
	<xsl:include href="sageresx://sage/resources/xslt/logic.xsl" />

	<xsl:variable name="view" select="/sage:view"/>
	<xsl:variable name="request" select="$view/sage:request"/>
	<xsl:variable name="response" select="$view/sage:response"/>
	<xsl:variable name="address" select="$view/sage:request/sage:address"/>
	<xsl:variable name="useragent" select="$view/sage:request/sage:useragent"/>
	<xsl:variable name="isDebugRequest" select="$view/sage:request/@debug = 1"/>
	<xsl:variable name="isDeveloperRequest" select="$view/sage:request/@developer = 1"/>

	<xsl:output method="xml" version="1.0" standalone="yes" omit-xml-declaration="yes"
		encoding="utf-8" media-type="text/xml" indent="yes"
		doctype-system="about:legacy-compat"/>

	<xsl:template match="/sage:view">
		<xsl:choose>
			<xsl:when test="count($response/sage:model/node()) = 0">
				<html>VOID MODEL</html>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="$response/sage:model/node()"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="sage:error">
		<div class="error">
			<xsl:apply-templates select="node()"/>
		</div>
	</xsl:template>

	<xsl:template match="sage:literal">
		<xsl:choose>
			<xsl:when test="ancestor::sage:literal">
				<xsl:apply-templates select="."/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="node()"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="xhtml:html">
		<html>
			<xsl:apply-templates select="@*"/>
			<xsl:attribute name="class">
				<xsl:value-of select="$view/@controller"/>
				<xsl:text> </xsl:text>
				<xsl:value-of select="$view/@action"/>
				<xsl:if test="string-length(@class)">
					<xsl:text> </xsl:text>
					<xsl:value-of select="@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:attribute name="lang">
				<xsl:value-of select="$request/@language"/>
			</xsl:attribute>
			<xsl:attribute name="data-thread">
				<xsl:value-of select="$request/@thread" />
			</xsl:attribute>
			<xsl:apply-templates select="node()"/>
		</html>
	</xsl:template>

	<xsl:template match="xhtml:head">
		<xsl:variable name="styles" select="$response/sage:resources/sage:head/xhtml:link | xhtml:link"/>
		<xsl:variable name="scripts" select="$response/sage:resources/sage:head/xhtml:script | xhtml:script"/>
		<head>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="*[local-name() != 'script' and local-name() != 'link']"/>
			<xsl:apply-templates select="set:distinct($styles, '@href', true())"/>
			<xsl:apply-templates select="set:distinct($scripts, '@src', true())"/>
			<xsl:apply-templates select="comment()"/>
		</head>
	</xsl:template>

	<xsl:template match="xhtml:body">
		<body>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
			<xsl:apply-templates select="$response/sage:resources/sage:body/xhtml:link"/>
			<xsl:apply-templates select="$response/sage:resources/sage:body/xhtml:script"/>
			<xsl:apply-templates select="." mode="execute-libraries"/>
		</body>
	</xsl:template>

	<xsl:template match="xhtml:script[starts-with(@src, 'kelp://')]">
		<xsl:for-each select="document(@src)/*/kelp:resource">
			<xsl:choose>
				<xsl:when test="@exists = 'false'">
					<xsl:comment> File not found: <xsl:value-of select="@path"/> </xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<script type="text/javascript" src="{@src}"></script>
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

	<xsl:template match="sage:link">
		<a>
			<xsl:apply-templates select="@*[name() != 'ref' and name() != 'values' and name() != 'escape']"/>
			<xsl:apply-templates select="node()"/>
		</a>
	</xsl:template>

	<xsl:template match="sage:resource[@type='script']">
		<script type="text/javascript" src="{@path}"></script>
	</xsl:template>

	<xsl:template match="sage:resource[@type='style']">
		<link type="text/css" rel="stylesheet" href="{@path}" />
	</xsl:template>

	<xsl:template match="xhtml:a/@href[starts-with(., '#')]">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="concat($request/@url, .)"/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="xhtml:a/@href[starts-with(., '~/')] | xhtml:img/@src[starts-with(., '~/')]">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="concat($request/@basehref, substring(., 3))"/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="@xml:base | @xml:space"/>

	<xsl:template match="@href[starts-with(., '~/')] | @src[starts-with(., '~/')]">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="concat(/sage:view/sage:request/sage:path/@applicationPath, substring-after(., '~/'))"/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="xhtml:*">
		<xsl:element name="{local-name()}">
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="*">
		<xsl:element name="{name()}" namespace="{namespace-uri()}">
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*">
		<xsl:attribute name="{name()}" namespace="{namespace-uri()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()">
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template match="comment()">
		<xsl:comment>
			<xsl:value-of select="."/>
		</xsl:comment>
	</xsl:template>

	<xsl:template match="*" mode="copy">
		<xsl:element name="{name()}">
			<xsl:apply-templates select="@*" mode="copy"/>
			<xsl:apply-templates select="node()" mode="copy"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*" mode="copy">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()" mode="copy">
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template match="*" mode="execute-libraries"/>

</xsl:stylesheet>
