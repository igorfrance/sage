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

	<xsl:template match="mod:Sidebar">
		<xsl:apply-templates select="." mode="mod:Sidebar"/>
	</xsl:template>

	<xsl:template match="*" mode="mod:Sidebar">
		<xsl:param name="config" select="mod:config"/>

		<section class="sidebar">
			<header>
				<a href="{/sage:view/sage:request/@basehref}">
					<span>Sage</span>
				</a>
			</header>
			<article>
				<xsl:apply-templates select="." mode="mod:Sidebar_Contents"/>
				<!-- <xsl:apply-templates select="." mode="mod:PageIndex_RelatedSiblings"/> -->
				<!-- <xsl:apply-templates select="." mode="mod:PageIndex_RelatedChildren"/> -->
				<xsl:apply-templates select="." mode="mod:PageIndex_PageIndex"/>
			</article>
		</section>

	</xsl:template>

	<xsl:template match="*" mode="mod:Sidebar_Contents">
		<xsl:param name="config" select="mod:config"/>
		<xsl:variable name="currentHref" select="normalize-space($config/mod:current/text())"/>
		<xsl:variable name="navigation" select="ancestor::sage:response/sage:resources/sage:data/site:navigation"/>
		<xsl:variable name="currentLink" select="$navigation//x:a[@href=$currentHref]"/>
		<xsl:variable name="siblings" select="$currentLink/parent::node()/parent::node()/xhtml:li/xhtml:a"/>
		<xsl:variable name="children" select="$currentLink/parent::node()/xhtml:ul/xhtml:li/xhtml:a"/>
		<xsl:variable name="levels" select="$currentLink/ancestor::x:li"/>

		<section class="contents">
			<header>Contents</header>
			<article>
				<xsl:for-each select="$levels">
					<div class="level level{position()}">
						<xsl:choose>
							<xsl:when test="position() = count($levels)">
								<ul class="siblings">
									<xsl:for-each select="$siblings">
										<li>
											<xsl:choose>
												<xsl:when test="@href = $currentHref">
													<span class="name{basic:iif(@href = $currentHref, ' current', '')}"><xsl:apply-templates select="text()"/></span>
													<xsl:if test="count($children) != 0">
														<ul class="children">
															<xsl:for-each select="$children">
																<li>
																	<xsl:apply-templates select="."/>
																</li>
															</xsl:for-each>
														</ul>
													</xsl:if>
												</xsl:when>
												<xsl:otherwise>
													<xsl:apply-templates select="."/>
												</xsl:otherwise>
											</xsl:choose>
										</li>
									</xsl:for-each>
								</ul>
							</xsl:when>
							<xsl:otherwise>
								<span class="name">
									<xsl:apply-templates select="x:a | x:span"/>
								</span>
							</xsl:otherwise>
						</xsl:choose>
					</div>
				</xsl:for-each>
			</article>
		</section>
	</xsl:template>

</xsl:stylesheet>
