<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/">
		<html>
			<head>
				<title>Sage application settings</title>
				<base href="{request/@basehref}"/>
				<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
				<script type="text/javascript" src="{DevTools}/dashboard/aeon.js"></script>
				<script type="text/javascript" src="{DevTools}/dashboard/placeholder.js"></script>
				<script type="text/javascript" src="{DevTools}/dashboard/devview.js"></script>
				<link type="text/css" rel="stylesheet" href="{DevTools}/dashboard/devview.css"/>
			</head>
			<body>

				<div id="container">
					<fieldset>
						<legend>Application Settings</legend>
						<form method="post" class="settings">
							<div class="setting">
								<label>Effective date:</label>
								<div class="value">
									<input type="text" name="effectiveDate" class="placeholder" placeholder="dd-MM-yyyy" value="{request/data/value[@id='effective-date']}"/>
								</div>
							</div>
							<div class="setting">
								<label>Current environment:</label>
								<div class="value">
									<xsl:variable name="environment" select="request/data/value[@id='environment']"/>
									<select name="environment">
										<option>
											<xsl:if test="$environment = 'staging'">
												<xsl:attribute name="selected">selected</xsl:attribute>
											</xsl:if>
											staging</option>
										<option>
											<xsl:if test="$environment = 'production'">
												<xsl:attribute name="selected">selected</xsl:attribute>
											</xsl:if>
											production
										</option>
									</select>
								</div>
							</div>
							<div class="submit">
								<input type="submit" value="SAVE SETTINGS"/>
							</div>
						</form>
					</fieldset>
				</div>

			</body>
		</html>
	</xsl:template>


</xsl:stylesheet>
