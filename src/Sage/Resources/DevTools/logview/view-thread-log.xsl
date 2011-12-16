<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/request">
		<html>
			<head>
				<title>Log viewer</title>
				<meta name="author" content="Cycle99" />
				<base href="{@basehref}"/>
				<script type="text/javascript" src="{DevTools}/aeon.js"></script>
				<script type="text/javascript" src="{DevTools}/tooltip.js"></script>
				<link rel="stylesheet" type="text/css" href="assets/shared/views/logview/logview.css" />
			</head>
			<body>
				<div id="header">
					<table width="100%" cellpadding="0" cellspacing="0" id="header_table">
						<colgroup>
							<col width="50"/>
							<col width="50"/>
							<col width="50"/>
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
				<div id="log">
					<xsl:apply-templates select="/request/data/log"/>
				</div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="log">
		<table width="100%" cellpadding="0" cellspacing="0" id="log_table" class="{@source}">
			<colgroup>
				<col width="50"/>
				<col width="50"/>
				<col width="50"/>
				<col width="200"/>
				<col/>
			</colgroup>
			<xsl:apply-templates select="line"/>
		</table>
	</xsl:template>

	<xsl:template match="line">
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
			<td class="logger tooltip" title="{@logger}">
				<div>
					<xsl:value-of select="@logger"/>
				</div>
			</td>
			<td class="message tooltip" title="{@message}" height="14">
				<div>
					<xsl:value-of select="@message"/>
				</div>
			</td>
		</tr>
	</xsl:template>

</xsl:stylesheet>
