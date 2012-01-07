<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:include href="sageresx://sage/resources/xslt/global.xslt"/>

	<xsl:output method="xml" indent="yes" omit-xml-declaration="no" encoding="iso-8859-1"/>

	<xsl:template match="sage:dateTime">
		<div>
			<xsl:value-of select="concat(/sage:view/sage:request/sage:dateTime/@date, 'T', /sage:view/sage:request/sage:dateTime/@time)"/>
		</div>
	</xsl:template>

</xsl:stylesheet>
