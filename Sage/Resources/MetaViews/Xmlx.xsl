<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:param name="path"/>
	<xsl:param name="expandLevels" select="4"/>

	<xsl:include href="sageresx://sage/resources/xslt/tree.xsl"/>
	
	<xsl:output indent="no"/>

	<xsl:template match="/">
		<xsl:apply-templates select="." mode="xmltree">
			<xsl:with-param name="expandLevels" select="$expandLevels"/>
		</xsl:apply-templates>
	</xsl:template>

</xsl:stylesheet>
