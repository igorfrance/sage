<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="mod">

	<xsl:template match="mod:SyntaxHighlighter">
		<div class="syntaxbox"><xsl:value-of select="mod:data/mod:formatted" disable-output-escaping="yes" /></div>
	</xsl:template>

</xsl:stylesheet>
