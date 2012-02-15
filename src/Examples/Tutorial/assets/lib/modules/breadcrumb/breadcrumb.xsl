<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns:site="http://sage.cycle99.com/"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns:set="http://www.cycle99.com/projects/sage/xslt/extensions/set"
	exclude-result-prefixes="set site mod sage">

	<xsl:template match="mod:BreadCrumb">
		<xsl:variable name="currentHref" select="normalize-space(mod:config/mod:current/text())"/>
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/site:navigation"/>
		<xsl:variable name="currentLink" select="$navigation//a[@href=$currentHref]"/>
		<xsl:variable name="levels" select="$currentLink/ancestor::li"/>
		<div class="breadcrumb">
			<xsl:for-each select="$levels">
				<span class="level">
					<xsl:choose>
						<xsl:when test="a/@href = $currentHref">
							<xsl:attribute name="class">level current</xsl:attribute>
							<span class="name">
								<xsl:apply-templates select="a/text()"/>
							</span>
						</xsl:when>
						<xsl:otherwise>
							<span class="name">
								<xsl:apply-templates select="a"/>
							</span>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="position() != last()">
						<span class="separator">
							<span class="icon">
								<span class="text">::</span>
							</span>
							<span class="children">
								<xsl:apply-templates select="ul" mode="breadcrumb-tree">
									<xsl:with-param name="current" select="$currentHref"/>
								</xsl:apply-templates>
							</span>
						</span>
					</xsl:if>
				</span>
			</xsl:for-each>
		</div>
	</xsl:template>

	<xsl:template match="ul" mode="breadcrumb-tree">
		<xsl:param name="current"/>
		<ul>
			<xsl:apply-templates select="li" mode="breadcrumb-tree">
				<xsl:with-param name="current" select="$current"/>
			</xsl:apply-templates>
		</ul>
	</xsl:template>

	<xsl:template match="li" mode="breadcrumb-tree">
		<xsl:param name="current"/>
		<li>
			<xsl:if test="a/@href = $current or @class">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
					<xsl:if test="a/@href = $current">
						<xsl:text> current</xsl:text>
					</xsl:if>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="@*[name() != 'class']" mode="breadcrumb-tree"/>
			<xsl:if test="count(ul) != 0">
				<span class="expander"></span>
			</xsl:if>
			<xsl:apply-templates select="node()" mode="breadcrumb-tree">
				<xsl:with-param name="current" select="$current"/>
			</xsl:apply-templates>
		</li>
	</xsl:template>

	<xsl:template match="a" mode="breadcrumb-tree">
		<xsl:apply-templates select="."/>
	</xsl:template>

	<xsl:template match="*" mode="breadcrumb-tree">
		<xsl:element name="{name()}">
			<xsl:apply-templates select="@*" mode="breadcrumb-tree"/>
			<xsl:apply-templates select="node()" mode="breadcrumb-tree"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*" mode="breadcrumb-tree">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()" mode="breadcrumb-tree">
		<xsl:value-of select="."/>
	</xsl:template>

</xsl:stylesheet>
