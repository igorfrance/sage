<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns:site="http://sage.cycle99.com/"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns:set="http://www.cycle99.com/schemas/sage/xslt/extensions/set.xsd"
	xmlns:x="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="x set site mod sage">

	<xsl:template match="mod:BreadCrumb">
		<xsl:variable name="currentHref" select="normalize-space(mod:config/mod:current/text())"/>
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/sage:data/site:navigation"/>
		<xsl:variable name="currentLink" select="$navigation//x:a[@href=$currentHref]"/>
		<xsl:variable name="levels" select="$currentLink/ancestor::x:li"/>
		<div class="breadcrumb">
			<xsl:for-each select="$levels">
				<span class="level">
					<xsl:choose>
						<xsl:when test="x:a/@href = $currentHref">
							<xsl:attribute name="class">level current</xsl:attribute>
							<span class="name">
								<xsl:apply-templates select="x:a/text()" mode="breadcrumb-tree"/>
							</span>
						</xsl:when>
						<xsl:otherwise>
							<span class="name">
								<xsl:apply-templates select="x:a | x:span" mode="breadcrumb-tree"/>
							</span>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="position() != last()">
						<span class="separator">
							<span class="icon">
								<span class="text">::</span>
							</span>
							<span class="children">
								<xsl:apply-templates select="x:ul" mode="breadcrumb-tree">
									<xsl:with-param name="current" select="$currentHref"/>
								</xsl:apply-templates>
							</span>
						</span>
					</xsl:if>
				</span>
			</xsl:for-each>
		</div>
	</xsl:template>

	<xsl:template match="x:ul" mode="breadcrumb-tree">
		<xsl:param name="current"/>
		<ul>
			<xsl:apply-templates select="x:li" mode="breadcrumb-tree">
				<xsl:with-param name="current" select="$current"/>
			</xsl:apply-templates>
		</ul>
	</xsl:template>

	<xsl:template match="x:li" mode="breadcrumb-tree">
		<xsl:param name="current"/>
		<li>
			<xsl:if test="x:a/@href = $current or @class">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
					<xsl:if test="x:a/@href = $current">
						<xsl:text> current</xsl:text>
					</xsl:if>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="@*[name() != 'class']" mode="breadcrumb-tree"/>
			<xsl:if test="count(x:ul) != 0">
				<span class="expander"></span>
			</xsl:if>
			<xsl:apply-templates select="node()" mode="breadcrumb-tree">
				<xsl:with-param name="current" select="$current"/>
			</xsl:apply-templates>
		</li>
	</xsl:template>

	<xsl:template match="x:a" mode="breadcrumb-tree">
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
