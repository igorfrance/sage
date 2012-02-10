<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:intl="http://www.cycle99.com/projects/sage/internationalization"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns:nav="http://www.cycle99.com/projects/sage/navigation"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="sage mod nav intl">

	<xsl:include href="tree.xsl" />

	<xsl:variable name="request" select="/request" />
	<xsl:variable name="queryparam" select="$request/querystring" />
	<xsl:variable name="environment" select="$request/environment" />
	<xsl:variable name="view" select="$request/data/sage:view" />
	<xsl:variable name="basehref" select="$request/@basehref" />
	<xsl:variable name="category" select="$request/@category" />
	<xsl:variable name="developer" select="$request/@developer" />
	<xsl:variable name="assembly" select="translate($request/assembly/@version,'.','_')" />
	<xsl:variable name="separator" select="'|*#*|'" />

	<xsl:variable name="paths" select="$request/paths"/>

	<xsl:output method="xml" version="1.0" encoding="utf-8"
		indent="yes" omit-xml-declaration="no"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/">
		<html id="t{request/@thread}" class="{$request/@controller} {$request/@action}">
			<head>
				<base href="{request/@basehref}"/>
				<meta charset="utf-8"/>
				<xsl:apply-templates select="." mode="head"/>
			</head>
			<body>
				<xsl:attribute name="class">
					<xsl:if test="$view/sage:body/@class">
						<xsl:value-of select="$view/sage:body/@class" />
					</xsl:if>
					<xsl:if test="$developer = '1'">
						<xsl:text> developer</xsl:text>
					</xsl:if>
				</xsl:attribute>
				<div id="pagewrapper">
					<xsl:apply-templates select="." mode="messages" />
					<xsl:apply-templates select="$view/sage:body/sage:area"/>
				</div>
				<div id="waitingOverlay"></div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="/request" mode="head">
		<title>
			<xsl:apply-templates select="." mode="page-title" />
		</title>
		<xsl:apply-templates select="$view/sage:head/sage:meta/sage:tag"/>
		<xsl:apply-templates select="$view/sage:head/sage:script"/>
		<xsl:apply-templates select="$view/sage:head/sage:style"/>
	</xsl:template>

	<xsl:template match="/request" mode="page-title">
		<xsl:apply-templates select="$view/sage:head/sage:title/node()" mode="page-title"/>
	</xsl:template>

	<xsl:template match="sage:meta/sage:tag">
		<meta name="{@name}">
			<xsl:attribute name="content">
				<xsl:apply-templates select="node()"/>
			</xsl:attribute>
		</meta>
	</xsl:template>

	<xsl:template match="sage:style">
		<style type="text/css">
			<xsl:apply-templates select="node()"/>
		</style>
	</xsl:template>

	<xsl:template match="sage:style[@href]">
		<link type="text/css" rel="stylesheet" href="{@href}"/>
	</xsl:template>

	<xsl:template match="sage:style[@merged]">
		<xsl:for-each select="document(concat('kelp://', @merged))/*/resource">
			<xsl:choose>
				<xsl:when test="@exists = 'false'">
					<xsl:comment>
						File not found: <xsl:value-of select="@path"/>
					</xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<link type="text/css" rel="stylesheet" href="{@src}"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="sage:script">
		<script type="text/javascript" language="javascript">
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</script>
	</xsl:template>

	<xsl:template match="sage:script[@src]">
		<script type="text/javascript" language="javascript">
			<xsl:apply-templates select="@*"/>
		</script>
	</xsl:template>

	<xsl:template match="sage:script[@merged]">
		<xsl:for-each select="document(concat('kelp://', @merged))/*/resource">
			<xsl:choose>
				<xsl:when test="@exists = 'false'">
					<xsl:comment>
						File not found: <xsl:value-of select="@path"/>
					</xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<script type="text/javascript" src="{@src}"></script>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="*" mode="page-title">
		<!-- Make sure that no markup makes it into the title tag. -->
		<xsl:variable name="title-result">
			<xsl:apply-templates select="."/>
		</xsl:variable>
		<xsl:value-of select="$title-result"/>
	</xsl:template>

	<xsl:template match="text()" mode="page-title">
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template match="/" mode="sidebar">
		SIDEBAR
	</xsl:template>

	<xsl:template match="/request" mode="messages">
		<div id="messages" class="{data/messages/@type}" style="display: none">
			<xsl:if test="count(data/messages/message) != 0">
				<xsl:attribute name="style">display: block</xsl:attribute>
			</xsl:if>
			<div class="wrapper">
				<div class="background"></div>
				<div class="content">
					<div class="close"></div>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="/request/data/value">
		<input type="hidden" class="variable" name="{@id}" value="{node()}" />
	</xsl:template>

	<xsl:template match="sage:area">
		<div>
			<xsl:if test="@class">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@name">
				<xsl:attribute name="id">
					<xsl:value-of select="@name"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates />
		</div>
	</xsl:template>

	<xsl:template match="mod:*">
		<xsl:apply-templates select="." mode="tree-headers"/>
		<div class="xmltree" style="border: solid 1px #ccc; max-height: 250px; overflow: auto; margin-bottom: 20px;">
			<xsl:apply-templates select="." mode="tree"/>
		</div>
	</xsl:template>

	<xsl:template match="mod:link">
		<xsl:param name="id"/>
		<xsl:param name="class" select="@class"/>
		<xsl:param name="style" select="@style"/>
		<xsl:param name="text"/>
		<xsl:param name="target" select="@target"/>
		<a href="{@href}">
			<xsl:if test="@url">
				<xsl:attribute name="href">
					<xsl:value-of select="@url"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="$id">
				<xsl:attribute name="id">
					<xsl:value-of select="$id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:attribute name="class">
				<xsl:value-of select="@linkId"/>
				<xsl:if test="$class">
					&#32;<xsl:value-of select="$class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="@openInNewWindow = 'True' or @openInNewWindow = 'true'">
				<xsl:attribute name="target">_blank</xsl:attribute>
			</xsl:if>
			<xsl:if test="$target">
				<xsl:attribute name="target">
					<xsl:value-of select="$target"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="$style">
				<xsl:attribute name="style">
					<xsl:value-of select="$style"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@metricsId">
				<xsl:attribute name="data-cm-id">
					<xsl:value-of select="@metricsId"/>
				</xsl:attribute>
			</xsl:if>
			<span>
				<xsl:choose>
					<xsl:when test="$text">
						<xsl:value-of select="$text"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates />
					</xsl:otherwise>
				</xsl:choose>
				<span class="inner"></span>
			</span>
		</a>
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
		<xsl:value-of select="."/>
	</xsl:template>

</xsl:stylesheet>