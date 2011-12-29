<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="mod sage">

	<xsl:template match="mod:ViewInspector">
		<xsl:param name="config" select="mod:config"/>
		<xsl:variable name="path">
			<xsl:apply-templates select="$config/mod:path"/>
		</xsl:variable>
		<xsl:variable name="cookies" select="/sage:view/sage:request/sage:cookies"/>
		<div class="view-inspector" data-view="{$path}">
			<xsl:apply-templates select="$config/mod:id" mode="attribute"/>
			<xsl:attribute name="class">
				<xsl:text>view-inspector </xsl:text>
				<xsl:choose>
					<xsl:when test="$cookies/@OR = 'VERTICAL'">vertical </xsl:when>
					<xsl:otherwise>horizontal </xsl:otherwise>
				</xsl:choose>
				<xsl:choose>
					<xsl:when test="$cookies/@LY = 'DOUBLE'">double </xsl:when>
					<xsl:otherwise>single </xsl:otherwise>
				</xsl:choose>
				<xsl:apply-templates select="$config/mod:class" mode="attribute-value"/>
			</xsl:attribute>
			<div class="contentcontainer">
				<div class="contenttoolbar">
					<div class="contenttitle"></div>
					<div class="toolbar right">
						<div class="button reload" data-command="Reload" title="Reload the current page"></div>
					</div>
				</div>
				<div class="contentframe">
					<iframe src="about:blank" frameborder="0" width="100%" height="100%"></iframe>
				</div>
				<div class="contentpreloader"></div>
			</div>
			<div class="toolscontainer">
				<div class="viewtoolbar">
					<div class="toolbar left">
						<xsl:for-each select="mod:data/mod:meta/mod:view">
							<div class="textbutton meta {text()}" title="Show {text()} meta view" data-command="ShowMeta" data-arguments="{text()}">
								<span><xsl:value-of select="text()"/></span>
							</div>
						</xsl:for-each>
						<div class="textbutton log" title="Show request log" data-command="ShowLog" data-arguments="log">
							<span>log</span>
						</div>
					</div>
					<div class="toolbar right">
						<div class="button toggle" title="Toggle the tool frame" data-command="ToggleFrame"></div>
					</div>
				</div>
				<div class="toolframe">
					<iframe src="about:blank" frameborder="0" width="100%" height="100%"></iframe>
				</div>
				<div class="toolpreloader"></div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="mod:ViewInspector/mod:config/*" mode="attribute-value">
		<xsl:apply-templates select="node()"/>
	</xsl:template>

	<xsl:template match="mod:ViewInspector/mod:config/*" mode="attribute">
		<xsl:attribute name="{local-name()}">
			<xsl:apply-templates select="." mode="attribute-value"/>
		</xsl:attribute>
	</xsl:template>

</xsl:stylesheet>
