<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:include href="sageresx://sage/resources/metaviews/xmlx.xsl"/>
	
	<xsl:output indent="no"/>

	<xsl:template match="/">
		<html xml:lang="en" lang="en">
			<head>
				<title>Project Config View</title>
				<xsl:apply-templates select="." mode="xmltree-styles"/>
			</head>
			<body>
				<xsl:apply-templates select="*" mode="xmlroot"/>
			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>
