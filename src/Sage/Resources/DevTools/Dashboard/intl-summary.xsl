<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:intl="http://www.cycle99.com/projects/sage/internationalization"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:variable name="url" select="/request/data/value[@id='url']"/>
	<xsl:variable name="error" select="/request/data/value[@id='error']"/>
	<xsl:variable name="summary" select="/request/data/resource"/>
	<xsl:variable name="locales" select="/request/data/locales"/>
	<xsl:variable name="locale" select="/request/data/value[@id='locale']"/>

	<xsl:template match="/">
		<html>
			<head>
				<title>Sage view internationalization summary</title>
				<base href="{request/@basehref}"/>
				<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
				<script type="text/javascript" src="{DevTools}/dashboard/global.js"></script>
				<script type="text/javascript" src="{DevTools}/dashboard/devview.js"></script>
				<link type="text/css" rel="stylesheet" href="{DevTools}/dashboard/devview.css"/>
			</head>
			<body class="intl-summary">
				<h1>Internationalization summary</h1>
				<address>
					<xsl:value-of select="$url"/>
				</address>
				<xsl:if test="string-length($error)">
					<blockquote class="error">
						<xsl:value-of select="$error" />
					</blockquote>
				</xsl:if>
				<div id="resources_phrases">
					<xsl:apply-templates select="$summary/phrases"/>
				</div>
				<div id="resources_files">
					<xsl:apply-templates select="$summary/files"/>
				</div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="phrases">
		<h2>Phrases</h2>
		<table class="phrases">
			<xsl:for-each select="phrase">
				<xsl:variable name="ok" select="count(@*[starts-with(name(), 'locale-') and .= '']) = 0"/>
				<tr>
					<td>
						<xsl:attribute name="class">
							<xsl:text>icon </xsl:text>
							<xsl:choose>
								<xsl:when test="$ok">ok</xsl:when>
								<xsl:otherwise>missing</xsl:otherwise>
							</xsl:choose>
						</xsl:attribute>
					</td>
					<td>
						<xsl:value-of select="@id"/>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>

	<xsl:template match="files">
		<h2>Files</h2>
		<div id="select_locale" class="selectcontrol">
			<span class="selected"><xsl:value-of select="$locale"/></span>
			<ul class="options">
				<xsl:for-each select="locale">
					<li>
						<xsl:value-of select="@name"/>
					</li>
				</xsl:for-each>
			</ul>
		</div>
		<xsl:for-each select="locale">
			<div class="locale {@name}">
				<xsl:if test="@name = $locale">
					<xsl:attribute name="style">display: block</xsl:attribute>
				</xsl:if>
				<h6><xsl:value-of select="@name"/></h6>
				<ul class="files">
					<xsl:apply-templates select="file"/>
				</ul>
			</div>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="file">
		<li>
			<xsl:value-of select="@path"/>
			<xsl:if test="count(file) != 0">
				<ul>
					<xsl:apply-templates select="file"/>
				</ul>
			</xsl:if>
		</li>
	</xsl:template>

</xsl:stylesheet>
