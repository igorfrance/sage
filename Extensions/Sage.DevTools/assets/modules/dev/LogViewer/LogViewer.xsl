<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:template match="mod:LogViewer">
		<xsl:variable name="theme" select="mod:config/mod:theme"/>
		<div class="logviewer">
			<xsl:attribute name="class">
				<xsl:text>logviewer</xsl:text>
				<xsl:if test="string-length($theme) != 0">
					<xsl:text> </xsl:text>
					<xsl:value-of select="$theme"/>
				</xsl:if>
			</xsl:attribute>
			<div class="header">
				<table>
					<colgroup>
						<col width="60"/>
						<col width="60"/>
						<col width="60"/>
						<col width="200"/>
						<col/>
					</colgroup>
					<tr>
						<th>Severity</th>
						<th class="tooltip" title="The time in milliseconds that elapsed between the start of the request, and the moment this message got logged">Elapsed</th>
						<th class="tooltip" title="The time in milliseconds that was measured within the method that logged the message">Duration</th>
						<th class="tooltip" title="The name of the logger that logged this message">Logger</th>
						<th>Message</th>
					</tr>
				</table>
			</div>
			<div class="content">
				<xsl:choose>
					<xsl:when test="count(mod:data/mod:log/mod:line) = 0">
						<h4 class="empty">No log data</h4>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="mod:data/mod:log"/>
					</xsl:otherwise>
				</xsl:choose>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="mod:LogViewer/mod:data/mod:log">
		<table width="100%" cellpadding="0" cellspacing="0" class="{@source}">
			<colgroup>
				<col width="60"/>
				<col width="60"/>
				<col width="60"/>
				<col width="200"/>
				<col/>
			</colgroup>
			<xsl:apply-templates select="*"/>
		</table>
	</xsl:template>

	<xsl:template match="mod:LogViewer/mod:data/mod:log/mod:line">
		<tr class="{@severity}">
			<td class="severity">
				<div>
					<xsl:value-of select="@severity"/>
				</div>
			</td>
			<td class="elapsed" align="right">
				<div>
					<xsl:value-of select="@elapsed"/>
				</div>
			</td>
			<td class="duration" align="right">
				<div>
					<xsl:value-of select="@duration"/>
				</div>
			</td>
			<td class="logger">
				<div class="tooltip" data-title="{@logger}" data-obscureOnly="true">
					<xsl:value-of select="@logger"/>
				</div>
			</td>
			<td class="message">
				<div class="tooltip" data-title="{@message}" data-obscureOnly="true">
					<xsl:value-of select="@message"/>
				</div>
			</td>
		</tr>
	</xsl:template>

</xsl:stylesheet>
