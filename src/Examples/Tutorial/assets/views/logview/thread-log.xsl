<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/sage:view">
		<html>
			<head>
				<title>Log viewer</title>
				<meta name="author" content="Cycle99" />
				<base href="{sage:request/@basehref}"/>
				<script type="text/javascript" src="{sage:request/sage:path/@assetPath}/scripts/lib/aeon.js"></script>
				<script type="text/javascript" src="{sage:request/sage:path/@assetPath}/scripts/lib/tooltip.js"></script>
				<style>

					html
						{ overflow-y: hidden; overflow-x: auto; }
					body, td, th
						{ font-family: verdana; font-size: 10px; }
					a, a:visited
						{ color: blue; }
					th
						{ text-align: left; cursor: default;  }
					table
						{ border-collapse: collapse; table-layout: fixed; }

					#header
						{ position: absolute; left: 0; right: 0; top: 0; height: 15px; background-color: #cecece; border-bottom: 1px solid #aaa; z-index: 1; }
					#log
						{ position: absolute; left: 0; right: 0; top: 15px; bottom: 0; z-index: 0; overflow-x: hidden; overflow-y: auto; }

					#header_table,
					#header_table th
						{ padding: 1px; background-color: #CECECE; border: 1px solid #CECECE; color: #7F7F7F; }

					#log_table
						{ }
					#log_table td
						{ border: 1px solid #eee; padding: 0px 1px; height: 14px; overflow: hidden; }
					#log_table td div
						{ height: 12px; overflow: hidden; white-space: pre; padding: 2px; }
					#log_table.dev
						{ border-bottom: 2px solid #666; }

					img.sql
						{	cursor:pointer; }

					tr.DEBUG td
						{ color: #7f7f7f; }
					tr.INFO td
						{ color: #000; }
					tr.WARN td
						{ color: #3333ff; }
					tr.ERROR td
						{ color: #cc0000; }
					tr.FATAL td
						{ color: #ff0000; font-weight: bold; font-style: italic; }

					#tooltip
						{
							background-color: #F5F5F5; color: black; border: 1px solid #4F5C71; padding: 2px;
							position: absolute; left: 0; top: 0; height: 18; font-size: 10px; z-index: 8;
						}
				</style>
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
					<xsl:apply-templates select="sage:response/log"/>
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
