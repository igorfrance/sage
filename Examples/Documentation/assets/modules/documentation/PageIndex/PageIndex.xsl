<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns:basic="http://www.cycle99.com/schemas/sage/xslt/extensions/basic.xsd"
	xmlns:string="http://www.cycle99.com/schemas/sage/xslt/extensions/string.xsd"
	xmlns:set="http://www.cycle99.com/schemas/sage/xslt/extensions/set.xsd"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="xhtml mod sage basic string set">

	<xsl:template match="mod:PageIndex">
		<xsl:apply-templates select="." mode="mod:PageIndex"/>
	</xsl:template>

	<xsl:template match="*" mode="mod:PageIndex">
		<xsl:param name="config" select="mod:config"/>
		<xsl:param name="mode" select="$config/mod:mode"/>

		<xsl:choose>
			<xsl:when test="$mode = 'siblings'">
				<xsl:apply-templates select="." mode="mod:PageIndex_RelatedSiblings"/>
			</xsl:when>
			<xsl:when test="$mode = 'children'">
				<xsl:apply-templates select="." mode="mod:PageIndex_RelatedChildren"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="." mode="mod:PageIndex_PageIndex"/>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<xsl:template match="*" mode="mod:PageIndex_RelatedSiblings">
		<xsl:param name="config" select="mod:config"/>
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/sage:data/site:navigation"/>
		<xsl:variable name="currentHref" select="$config/mod:current/text()"/>
		<xsl:variable name="currentLink" select="$navigation//xhtml:a[@href=$currentHref][1]"/>
		<xsl:variable name="siblings" select="$currentLink/parent::node()/parent::node()/xhtml:li/xhtml:a"/>
		<xsl:variable name="parentAxis" select="$currentLink/parent::xhtml:li/parent::xhtml:ul/parent::xhtml:li"/>
		<xsl:variable name="parentName" select="($parentAxis/xhtml:a | $parentAxis/xhtml:span)[1]"/>
		<xsl:if test="count($siblings) > 1">
			<section class="pageindex related {$config/mod:class}">
				<header>
					<xsl:value-of select="$parentName"/>
				</header>
				<article>
					<ul>
						<xsl:for-each select="$siblings">
							<li class="name{basic:iif(@href = $currentHref, ' current', '')}">
								<xsl:choose>
									<xsl:when test="@href = $currentHref">
										<xsl:apply-templates select="text()"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:apply-templates select="."/>
									</xsl:otherwise>
								</xsl:choose>
							</li>
						</xsl:for-each>
					</ul>
				</article>
			</section>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*" mode="mod:PageIndex_RelatedChildren">
		<xsl:param name="config" select="mod:config"/>
		<xsl:variable name="currentHref" select="$config/mod:current/text()"/>
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/sage:data/site:navigation"/>
		<xsl:variable name="currentLink" select="$navigation//xhtml:a[@href=$currentHref][1]"/>
		<xsl:variable name="children" select="$currentLink/parent::node()/xhtml:ul/xhtml:li/xhtml:a"/>

		<xsl:if test="count($children) != 0">
			<section class="related children">
				<header>
					<xsl:apply-templates select="$currentLink/node()"/>
				</header>
				<article>
					<ul>
						<xsl:for-each select="$children">
							<li>
								<xsl:apply-templates select="."/>
							</li>
						</xsl:for-each>
					</ul>
				</article>
			</section>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*" mode="mod:PageIndex_PageIndex">
		<xsl:param name="config" select="mod:config"/>
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/sage:data/site:navigation"/>
		<xsl:variable name="currentHref" select="normalize-space($config/mod:current/text())"/>
		<xsl:variable name="currentLink" select="$navigation//x:a[@href=$currentHref]"/>

		<xsl:variable name="headers" select="//xhtml:*[@data-index='yes']"/>
		<xsl:choose>
			<xsl:when test="count($headers)">
				<section class="pageindex">
					<header>
						On this page
					</header>
					<article>
						<ul>
							<xsl:for-each select="$headers">
								<xsl:variable name="headerid">
									<xsl:apply-templates select="." mode="page-index-generate-id"/>
								</xsl:variable>
								<xsl:variable name="level" select="basic:isnull(@data-level, count(ancestor-or-self::*[@data-index='yes']))"/>

								<xsl:variable name="position" select="position()"/>
								<xsl:variable name="nextHeader" select="$headers[$position + 1]"/>
								<xsl:variable name="nextLevel" select="basic:isnull(
									$nextHeader/@data-level, count($nextHeader/ancestor-or-self::*[@data-index='yes']))"/>

								<xsl:variable name="isListEnd" select="$level &gt; $nextLevel"/>
								<li class="level{$level}{basic:iif($isListEnd, ' listEnd', '')}">
									<a href="{concat($request/sage:address/@url, '#', $headerid)}">
										<xsl:choose>
											<xsl:when test="@data-text">
												<xsl:value-of select="@data-text"/>
											</xsl:when>
											<xsl:when test="xhtml:header">
												<xsl:value-of select="xhtml:header"/>
											</xsl:when>
											<xsl:when test="xhtml:*[string:matches(local-name(), 'h\d')]">
												<xsl:value-of select="xhtml:*[string:matches(local-name(), 'h\d')]"/>
											</xsl:when>
											<xsl:otherwise>
												<xsl:value-of select="."/>
											</xsl:otherwise>
										</xsl:choose>
									</a>
								</li>
							</xsl:for-each>
						</ul>
					</article>
				</section>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="*[@data-index='yes']">
		<xsl:element name="{name()}">
			<xsl:attribute name="id">
				<xsl:apply-templates select="." mode="page-index-generate-id"/>
			</xsl:attribute>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="*[@data-index='yes']" mode="page-index-generate-id">
		<xsl:value-of select="basic:isnull(@id, generate-id(.))"/>
	</xsl:template>


</xsl:stylesheet>
