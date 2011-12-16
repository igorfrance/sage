<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:param name="developer" select="0"/>

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/exception">
		<html>
			<head>
				<title><xsl:value-of select="@description"/></title>
				<meta http-equiv="content-type" content="text/html; charset=utf-8" />
				<xsl:apply-templates select="." mode="css"/>
				<xsl:apply-templates select="." mode="script"/>
			</head>
			<body class="error">
				<xsl:choose>
					<xsl:when test="$developer = 1">
						<xsl:apply-templates select="." mode="developer"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." mode="standard"/>
					</xsl:otherwise>
				</xsl:choose>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="exception" mode="standard">
		<xsl:text>An error occured during the processing of your request.</xsl:text>
	</xsl:template>

	<xsl:template match="exception" mode="developer">
		<div class="exception">
			<xsl:apply-templates select="." mode="general"/>
			<xsl:apply-templates select="." mode="details"/>
			<xsl:if test="not(parent::exception)">
				<xsl:apply-templates select="." mode="requestInfo"/>
			</xsl:if>
			<xsl:if test="count(exception) != 0">
				<div class="inner">
					<xsl:apply-templates select="exception" mode="developer"/>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="exception" mode="css">
		<style>

			.error
				{ font-family: Verdana; font-size: 13px; }
			.error .errorType
				{ border-bottom: 3px solid #e2e2e2; font-weight: bold; font-style: italic; padding-bottom: 8px; color: #c00; font-size: 20px; }
			.error .errorDescription
				{ font-size: 20px; font-style: italic; margin: 6px 0 30px; }
			.error .errorFile
				{ font-size: 11px; }
			
			.error .exception .details
				{ margin-bottom: 20px; }
			
			.error .errorLogo
				{ position: absolute; right: 10px; top: 54px; }
			.error .stack-toggler
				{ font-size: 10px; color: #cccccc; cursor: default; }

			.error .stacktrace
				{ background-color: #E9E9E9; padding: 10px; }
			.error .stacktrace .line
				{ color: #7F7F7F; display: none; }
			.error .stacktrace .file
				{ color: #7F7F7F; padding-left: 50px; font-size: 85%; font-style: italic; }
			.error .stacktrace .myline
				{ color: #000000; }

			.error .requestInfo
				{ margin-top: 30px; }

			.error .errorLine
				{ margin-bottom: 2px; #height: 1px; }
			.error .errorLine .title
				{ color: #999; float: left; width: 160px; text-align: right; padding-right: 10px; }
			.error .errorLine .text
				{ margin-left: 120px; }
			.error .formdata
				{ background-color: #E9E9E9; border: 1px solid #CECECE; }
			.error .paramName
				{ background-color: #F5F5F5; }

			.error .inner .exception
				{ margin-top: 50px; }
			.error .exception .inner .stacktrace
				{ font-size: 72%; }
			.error .exception .inner .errorType
				{ font-size: 85%; }
			.error .inner .errorDescription
				{ font-size: 78%; margin-bottom: 15px;}
			.error .exception .inner .errorFile
				{ font-size: 78%; }
			

		</style>
	</xsl:template>

	<xsl:template match="exception" mode="script">
		<script type="text/javascript" language="javascript"><xsl:comment>
					<![CDATA[

			function toggleDetail()
			{
				var d = document.getElementById('errorDetail');
				d.style.display = d.offsetHeight ? 'none' : 'block'
			}

			function showStack(control)
			{
				var target = control.parentNode.nextSibling;
				while (target && target.nodeType != 1)
					target = target.nextSibling;
				
				if (target && target.className.indexOf("stacktrace") != -1)
				{
					var lines = target.getElementsByTagName("DIV");

					for (var i = 0; i < lines.length; i++)
						if (lines[i].className == "line")
							lines[i].style.display = lines[i].offsetHeight ? "none" : "block";
				}
			}

		]]></xsl:comment></script>
	</xsl:template>

	<xsl:template match="exception" mode="general">
		<div class="errorType">
			<xsl:value-of select="@type"/>
		</div>
		<div class="errorDescription">
			<xsl:value-of select="@htmlDescription" disable-output-escaping="yes"/>
			<xsl:if test="@sourceuri">
				<div class="errorFile"><a href="{@sourceuri}"><xsl:value-of select="@sourceuri"/></a></div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="exception" mode="details">
		<div class="detail">
			<xsl:choose>
				<xsl:when test="count(stacktrace/frame[@line])">
					<xsl:if test="count(stacktrace/frame[not(@line)])">
						<div class="stack-toggler">
							(<span onclick="showStack(this)" style="cursor: pointer">view <span id="callstack_other">full</span> call stack</span>)
						</div>
					</xsl:if>
					<div class="stacktrace">
						<xsl:for-each select="stacktrace/frame">
							<xsl:variable name="class">
								<xsl:choose>
									<xsl:when test="@line">myline</xsl:when>
									<xsl:otherwise>line</xsl:otherwise>
								</xsl:choose>
							</xsl:variable>
							<div class="{$class}">
								<xsl:value-of select="@text"/>
								<xsl:if test="@line">
									<xsl:text> line </xsl:text>
									<xsl:value-of select="@line"/>
								</xsl:if>
								<xsl:if test="@file">
									<div class="file">
										<xsl:text> in </xsl:text>
										<xsl:value-of select="@file"/>
									</div>
								</xsl:if>
							</div>
						</xsl:for-each>
					</div>
				</xsl:when>
				<xsl:otherwise>
					<div id="stacktrace" class="stacktrace">
						<xsl:for-each select="stacktrace/frame">
							<div><xsl:value-of select="@text"/></div>
						</xsl:for-each>
					</div>
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>

	<xsl:template match="exception" mode="requestInfo">
		<div class="requestInfo">
			<div class="errorLine">
				<div class="title">
					IP Address:
				</div>
				<div class="text">
					<xsl:value-of select="request/@remoteip"/>
				</div>
			</div>
			<div class="errorLine">
				<div class="title">
					Date and time:
				</div>
				<div class="text">
					<xsl:value-of select="@date"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="@time"/>
				</div>
			</div>
			<div class="errorLine">
				<div class="title">
					URL:
				</div>
				<div class="text">
					<a href="{request/address/@url}"><xsl:value-of select="request/address/@url"/></a>
				</div>
			</div>
			<xsl:if test="string-length(request/@referrer)">
				<div class="errorLine">
					<div class="title">
						Referrer:
					</div>
					<div class="text">
						<a href="{request/@referrer}"><xsl:value-of select="request/@referrer"/></a>
					</div>
				</div>
			</xsl:if>
			<div class="errorLine">
				<div class="title">
					User agent:
				</div>
				<div class="text">
					<xsl:value-of select="request/browser/@useragent"/>
				</div>
			</div>
		</div>
	</xsl:template>
	
</xsl:stylesheet>
