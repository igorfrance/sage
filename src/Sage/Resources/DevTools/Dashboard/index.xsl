<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:x="http://www.cycle99.com/projects/sage/dev"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules">

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/">
		<html>
			<head>
				<title>Welcome to Sage!</title>
				<base href="{request/@basehref}"/>
				<style>
					
					body
						{ font-family: Verdana; font-size: 12px; }
						
					a, a:visited
						{ color: blue; }
					ul
						{ list-style-type: none; }
					
				</style>
			</head>
			<body>
				<h1>Welcome to Sage!</h1>
				<p>
					On this system, the following categories are present:
				</p>
				<xsl:apply-templates select="/request/data/categories"/>
				<h2>Documentation</h2>
				<ul>
					<li>
						<a href="http://wiki.cycle99.com/Sage.Default.aspx">Wiki documentation</a>
					</li>
					<li>
						<xsl:choose>
							<xsl:when test="/request/data/docs/net/@exists=1">
								<a href="{/request/data/docs/net/@path}">API documentation (.NET)</a>
							</xsl:when>
							<xsl:otherwise>
								No .NET API documenation (<xsl:value-of select="/request/data/docs/net/@path"/> doesn't exist)
							</xsl:otherwise>
						</xsl:choose>
					</li>
					<li>
						<xsl:choose>
							<xsl:when test="/request/data/docs/js/@exists=1">
								<a href="{/request/data/docs/js/@path}">API documentation (JavaScript)</a>
							</xsl:when>
							<xsl:otherwise>
								No JavaScript API documenation (<xsl:value-of select="/request/data/docs/js/@path"/> doesn't exist)
							</xsl:otherwise>
						</xsl:choose>
					</li>
				</ul>
				<h2>Tools</h2>
				<ul>
					<li>
						<a href="dev/dashboard/">Developer Dashboard</a>
					</li>
				</ul>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="categories">
		<ul class="categories">
			<xsl:choose>
				<xsl:when test="count(category) = 0">
					<li>No categories present</li>
				</xsl:when>
				<xsl:otherwise>
					<xsl:for-each select="category">
						<li>
							<a href="{@link}">
								<xsl:value-of select="@name"/>
							</a>
						</li>
					</xsl:for-each>
				</xsl:otherwise>
			</xsl:choose>
		</ul>
	</xsl:template>

</xsl:stylesheet>
