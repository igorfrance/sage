<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/installer">
		<html>
			<head>
				<title>DevTools Installer</title>
				<meta http-equiv="content-type" content="text/html; charset=utf-8" />
				<style>
					html, body
						{ height: 100%; overflow: hidden; margin: 0; padding: 0; }
					h1
						{ margin: 0; padding: 9px 30px 0 10px; float: left; }
					body
						{ font-family: Kalinga, "Lucida Sans Unicode", Verdana, Arial, Helvetica, sans-serif; font-size: 12px; }
					
					div.header
						{ position: absolute; top: 0; left: 0; right: 0; height: 49px; border-bottom: 1px solid #ccc; background: #f0f0f0;  }
					div.content
						{ position: absolute; top: 50px; bottom: 50px; left: 0; right: 0; overflow: auto; }
					div.footer
						{ position: absolute; bottom: 0; left: 0; right: 0; height: 49px; border-top: 1px solid #ccc; background: #f0f0f0;  }
					
					div.header h1
						{ float: left; }
					div.header p,
					div.footer p
						{ line-height: 14px; }
					
					input.submit
						{ position: absolute; bottom: 10px; right: 10px; border: 1px solid #ccc; padding: 2px 100px; }
					input.submit:hover
						{ background: #f30; }
					
					ul
						{ list-style-type: none; padding: 0; margin: 0; }
					li
						{ padding-left: 20px; background: 8px 6px url(data:image/gif;base64,R0lGODlhBwAHAIAAAAAAAP///yH5BAUUAAEALAAAAAAHAAcAAAIMjA9nwMj9wmuLIlUAADs=) no-repeat; }
					li.missing
						{ background-image: url(data:image/gif;base64,R0lGODlhBwAHAJEAAP8AAP///////wAAACH5BAUUAAIALAAAAAAHAAcAAAIMlIB2CmudXGrPWUQLADs=); }

				</style>
			</head>
			<body>
				<div class="header">
					<h1>Developer Tools Installer</h1>
					<p>
						Developer tools is a collection of controllers and their related resources that can be used with Sage to aid
						in development. This page provides an installer that copies the required resources to a folder from which they
						can be accessed.
					</p>
				</div>
				<div class="content">
					<ul>
						<xsl:apply-templates select="resources/resource"/>
					</ul>
				</div>
				<div class="footer">
					<form method="post">
						<input type="submit" value="Begin installation" class="submit"/>
					</form>
				</div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="resource">
		<li>
			<xsl:if test="@missing">
				<xsl:attribute name="class">missing</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="."/>
		</li>
	</xsl:template>

</xsl:stylesheet>
