<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:param name="problemType" select="0"/>
	
	<xsl:include href="error.xslt"/>

	<xsl:template match="/exception">
		<html>
			<head>
				<title>
					<xsl:apply-templates select="." mode="problem-title"/>
				</title>
				<xsl:apply-templates select="." mode="css"/>
				<xsl:apply-templates select="." mode="script"/>
				<style>
					
					body
						{ font-family: Century Gothic, Verdana, Helvetica, sans-serif; }
					h1
						{ font-size: 36px; margin: 0 0 30px 0; font-weight: normal; }
						
					.problem-source
						{	color: #009966; }
					.problem-message
						{ font-size: 21px; }
					.problem-details .switch 
						{ font-size: 12px; }
					.problem-details .switch a
						{ color: blue; }
					.problem-details .switch a:hover
						{ color: #f30; }
					.problem-details .content
						{ display: none; border-left: 3px solid #ccc; margin: 10px 0 50px 0; padding-left: 20px; }
					
				</style>
				<script>
					
					function expandProblemDetail()
					{
						var x = document.getElementById("problem-details-content"); 
						x.style.display = x.offsetHeight ? "none" : "block";
					}
					
				</script>
			</head>
			<body>
				<h1>
					<xsl:apply-templates select="." mode="problem-title"/>
				</h1>
				<p class="problem-description">
					<xsl:apply-templates select="." mode="problem-description"/>
				</p>
				<xsl:if test="@sourceuri">
					<div class="problem-file">
						The error occured in 
						<span class="problem-source"><xsl:value-of select="@sourceuri"/></span>
					</div>
					<div class="problem-location">
						<span class="line">Line <xsl:value-of select="@linenumber" />, </span>
						<span class="column">position <xsl:value-of select="@linenumber" />.</span>
					</div>
				</xsl:if>
				<p class="problem-message">
					<xsl:value-of select="@htmlDescription" disable-output-escaping="yes"/>
				</p>
				<div class="problem-details">
					<span class="switch">(<a href="javascript:expandProblemDetail()">Click to show technical details about this error</a>)</span>
					<div class="content" id="problem-details-content">
						<xsl:apply-templates select="." mode="developer"/>
					</div>
				</div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="/exception" mode="problem-title">
		<xsl:choose>
			<xsl:when test="$problemType = 'InvalidMarkup'">
				Invalid Markup Detected
			</xsl:when>
			<xsl:otherwise>An error was caught.</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template match="/exception" mode="problem-description">
		<xsl:choose>
			<xsl:when test="$problemType = 'InvalidMarkup'">
				Sage speaks XML, and therefore any XML needs to be valid. <br/>				HTML templates must parse as XHTML in order for them to use usable.
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	
</xsl:stylesheet>
