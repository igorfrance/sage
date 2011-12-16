<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:intl="http://www.cycle99.com/projects/sage/internationalization"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:variable name="selectedUrl">
		<xsl:choose>
			<xsl:when test="request/form/@url">
				<xsl:value-of select="request/form/@url"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="request/querystring/@url"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:template match="/">
		<html>
			<head>
				<title>Save input XML for a Sage page</title>
				<base href="{request/@basehref}"/>
				<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
				<script type="text/javascript" src="{DevTools}/dashboard/global.js"></script>
				<script type="text/javascript" src="{DevTools}/dashboard/devview.js"></script>
				<script type="text/javascript" src="{DevTools}/dashboard/placeholder.js"></script>
				<link type="text/css" rel="stylesheet" href="{DevTools}/dashboard/devview.css"/>
			</head>
			<body>

				<div id="saveXmlOptions">
					<fieldset class="options">
						<legend>Save this page:</legend>
						<form method="post" action="{request/address/@url}">
							<div class="setting">
								<label for="ctrlUrl">URL: </label>
								<div class="value">
									<input id="ctrlUrl" type="text" name="URL" class="placeholder" placeholder="Specify the URL to save" value="{$selectedUrl}"/>
								</div>
							</div>
							<div class="setting">
								<label for="ctrlName">Name: </label>
								<div class="value">
									<input id="ctrlName" type="text" name="Name" class="placeholder tooltip" placeholder="Name" value="{request/data/value[@id='filename']}" title="This name will be used in lists to identify this saved page"/>
								</div>
							</div>
							<div class="setting">
								<label for="ctrlCategory">Category: </label>
								<div class="value">
									<input id="ctrlCategory" type="text" name="Category" class="placeholder" placeholder="Category" value="{request/data/value[@id='category']}"/>
								</div>
							</div>
							<div class="setting">
								<label for="ctrlController">Controller: </label>
								<div class="value">
									<input id="ctrlController" type="text" name="Controller" class="placeholder" placeholder="Controller" value="{request/data/value[@id='controller']}"/>
								</div>
							</div>
							<div class="setting">
								<label for="ctrlAction">Action: </label>
								<div class="value">
									<input id="ctrlAction" type="text" name="Action" class="placeholder" placeholder="Action" value="{request/data/value[@id='action']}"/>
								</div>
							</div>
							<div class="setting">
								<label for="ctrlRemarks">Remarks: </label>
								<div class="value">
									<textarea id="ctrlRemarks" type="text" name="Remarks" class="placeholder" placeholder="Remarks">
									</textarea>
								</div>
							</div>
							<div class="submit">
								<input type="submit" value="SUBMIT"/>
							</div>
						</form>
						<div class="submit">
						</div>
					</fieldset>
				</div>

			</body>
		</html>
	</xsl:template>


</xsl:stylesheet>
