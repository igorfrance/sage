<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:template match="root">
		<hello>
			<xsl:apply-templates select="*"/>
		</hello>
	</xsl:template>

	<xsl:template match="group">
		<world>!</world>
	</xsl:template>

</xsl:stylesheet>
