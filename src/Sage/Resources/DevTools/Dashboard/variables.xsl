<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:intl="http://www.cycle99.com/projects/sage/internationalization"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:variable name="category" select="/request/@category"/>
	<xsl:variable name="category-variables" select="//intl:variable"/>
	<xsl:variable name="context-variables" select="/request/data/sage:view/variables/variable"/>

	<xsl:template match="/">
		<html>
			<head>
				<title>Sage context and category variables</title>
				<base href="{request/@basehref}"/>
				<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
				<script type="text/javascript" src="{DevTools}/dashboard/aeon.js"></script>
				<script type="text/javascript" src="{DevTools}/dashboard/placeholder.js"></script>
				<script type="text/javascript" src="{DevTools}/dashboard/devview.js"></script>
				<link type="text/css" rel="stylesheet" href="{DevTools}/dashboard/devview.css"/>
			</head>
			<body>

				<div id="container">
					<div id="select_category" class="selectcontrol">
						<span class="selected"><xsl:value-of select="$category"/></span>
						<ul class="options">
							<xsl:for-each select="/request/data/links/@*">
								<li>
									<a href="{.}"><xsl:value-of select="name()"/></a>
								</li>
							</xsl:for-each>
						</ul>
					</div>
					<fieldset class="clear">
						<legend>Category Variables</legend>
						<xsl:choose>
							<xsl:when test="count($category-variables) != 0">
								<xsl:for-each select="$category-variables">
									<xsl:variable name="variable" select="."/>
									<div class="togglesection">
										<div class="header">
											<xsl:value-of select="@id"/>
										</div>
										<div class="content">
											<table class="settings">
												<tr>
													<th>Locale</th>
													<th>Value</th>
													<th>Source locale</th>
												</tr>
												<xsl:for-each select="resolved/value">
													<tr>
														<xsl:attribute name="class">
															<xsl:choose>
																<xsl:when test="@source = @locale">defined</xsl:when>
																<xsl:otherwise>inherited</xsl:otherwise>
															</xsl:choose>
														</xsl:attribute>
														<td class="locale">
															<xsl:value-of select="@locale"/>
														</td>
														<td class="value tooltip">
															<xsl:attribute name="title">
																<xsl:choose>
																	<xsl:when test="@source = @locale">This value is defined for this locale</xsl:when>
																	<xsl:otherwise>
																		This value is inherited from another locale (<xsl:value-of select="@source"/>)
																	</xsl:otherwise>
																</xsl:choose>
															</xsl:attribute>
															<xsl:value-of select="text()"/>
														</td>
														<td class="source">
															<xsl:if test="@source != @locale">
																(<xsl:value-of select="@source"/>)
															</xsl:if>
														</td>
													</tr>
												</xsl:for-each>
											</table>
										</div>
									</div>
								</xsl:for-each>
							</xsl:when>
							<xsl:otherwise>
								<div class="empty">
									Current category doesn't define any category specific variables.
								</div>
							</xsl:otherwise>
						</xsl:choose>
					</fieldset>
				</div>
			</body>
		</html>
	</xsl:template>


</xsl:stylesheet>
