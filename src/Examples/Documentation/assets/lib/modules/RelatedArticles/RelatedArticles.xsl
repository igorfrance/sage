<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="xhtml mod sage">

	<xsl:template match="mod:RelatedArticles">
		<xsl:param name="config" select="mod:config"/>
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/site:navigation"/>
		<xsl:variable name="currentUrl" select="$config/mod:current/text()"/>
		<xsl:variable name="currentDisplay" select="$config/mod:display/text()"/>
		<xsl:variable name="currentLink" select="$navigation//xhtml:a[@href=$currentUrl][1]"/>

		<div class="related-articles">
			<xsl:choose>
				<xsl:when test="$currentDisplay = 'parent'">
					<xsl:apply-templates select="." mode="display-parent">
						<xsl:with-param name="currentLink" select="$currentLink"/>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:when test="$currentDisplay = 'siblings'">
					<xsl:apply-templates select="." mode="display-siblings">
						<xsl:with-param name="currentLink" select="$currentLink"/>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:when test="$currentDisplay = 'children'">
					<xsl:apply-templates select="." mode="display-children">
						<xsl:with-param name="currentLink" select="$currentLink"/>
					</xsl:apply-templates>
				</xsl:when>
			</xsl:choose>
		</div>

	</xsl:template>

	<xsl:template match="mod:RelatedArticles" mode="display-parent">
		<xsl:param name="currentLink"/>
		<xsl:variable name="parent" select="$currentLink/parent::node()/parent::node()/parent::node()/xhtml:a"/>
		<div class="parent">
			<h5>
				<xsl:apply-templates select="$parent"/>
			</h5>
		</div>
	</xsl:template>

	<xsl:template match="mod:RelatedArticles" mode="display-siblings">
		<xsl:param name="currentLink"/>
		<xsl:variable name="siblings" select="$currentLink/parent::node()/parent::node()/xhtml:li/xhtml:a"/>
		<div class="siblibgs">
			<h6>Related articles</h6>
			<ul>
				<xsl:for-each select="$siblings">
					<li>
						<xsl:apply-templates select="."/>
					</li>
				</xsl:for-each>
			</ul>
		</div>
	</xsl:template>

	<xsl:template match="mod:RelatedArticles" mode="display-children">
		<xsl:param name="currentLink"/>
		<xsl:variable name="children" select="$currentLink/parent::node()/xhtml:ul/xhtml:li/xhtml:a"/>
		<div class="children">
			<h3 class="section">Further reading</h3>
			<ul>
				<xsl:for-each select="$children">
					<li>
						<xsl:apply-templates select="."/>
					</li>
				</xsl:for-each>
			</ul>
		</div>
	</xsl:template>

</xsl:stylesheet>
