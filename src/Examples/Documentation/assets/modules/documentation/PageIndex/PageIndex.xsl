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
		<xsl:param name="config" select="mod:config"/>

		<xsl:variable name="headers" select="//xhtml:*[string:matches(local-name(), 'h\d') and @data-index]"/>
		<div class="pageindex">
			<xsl:choose>
				<xsl:when test="count($headers)">
					<h3 class="section">On this page:</h3>
					<blockquote>
						<ul>
							<xsl:for-each select="$headers">
								<xsl:variable name="headerid">
									<xsl:apply-templates select="." mode="page-index-generate-id"/>
								</xsl:variable>
								<li class="level{basic:isnull(@data-level, 1)}">
									<a href="{concat($request/sage:address/@url, '#', $headerid)}">
										<xsl:choose>
											<xsl:when test="@data-text">
												<xsl:value-of select="@data-text"/>
											</xsl:when>
											<xsl:otherwise>
												<xsl:apply-templates select="node()"/>
											</xsl:otherwise>
										</xsl:choose>
									</a>
								</li>
							</xsl:for-each>
						</ul>
					</blockquote>
				</xsl:when>
				<xsl:otherwise>
					<h3 class="section">No headers with @data-index attribute found on this page</h3>
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>

	<xsl:template match="
		xhtml:h1[@data-index='yes'] |
		xhtml:h2[@data-index='yes'] |
		xhtml:h3[@data-index='yes'] |
		xhtml:h4[@data-index='yes'] |
		xhtml:h5[@data-index='yes'] |
		xhtml:h6[@data-index='yes']">

		<xsl:element name="{name()}">
			<xsl:attribute name="id">
				<xsl:apply-templates select="." mode="page-index-generate-id"/>
			</xsl:attribute>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</xsl:element>

	</xsl:template>

	<xsl:template match="
		xhtml:h1[@data-index='yes'] |
		xhtml:h2[@data-index='yes'] |
		xhtml:h3[@data-index='yes'] |
		xhtml:h4[@data-index='yes'] |
		xhtml:h5[@data-index='yes'] |
		xhtml:h6[@data-index='yes']" mode="page-index-generate-id">

		<xsl:value-of select="basic:isnull(@id, generate-id(.))"/>

	</xsl:template>

</xsl:stylesheet>
