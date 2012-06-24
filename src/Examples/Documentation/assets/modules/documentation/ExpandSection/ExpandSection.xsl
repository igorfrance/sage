<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns:basic="http://www.cycle99.com/schemas/sage/xslt/extensions/basic.xsd"
	xmlns:string="http://www.cycle99.com/schemas/sage/xslt/extensions/string.xsd"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="xhtml mod sage basic string">

	<xsl:template match="mod:ExpandSection">
		<xsl:param name="config" select="mod:config"/>
		<xsl:param name="id">
			<xsl:apply-templates select="$config/mod:id"/>
		</xsl:param>
		<xsl:param name="class">
			<xsl:apply-templates select="$config/mod:class"/>
		</xsl:param>
		<xsl:param name="state" select="
			basic:isnull(/sage:view/sage:request/sage:cookies/@*[name()=$id],
			basic:isnull($config/mod:state, 'open'))"/>

		<section>
			<xsl:attribute name="class">
				<xsl:text>expand</xsl:text>
				<xsl:if test="string-length($class) != 0">
					<xsl:value-of select="concat(' ', $class)"/>
				</xsl:if>
				<xsl:if test="$state='collapsed'">
					<xsl:text> collapsed</xsl:text>
				</xsl:if>
			</xsl:attribute>
			<xsl:apply-templates select="mod:data/node()"/>
		</section>
	</xsl:template>

</xsl:stylesheet>
