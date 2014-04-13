<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:template match="mod:DeveloperToolbar">
		<div id="developer-toolbar">
			<xsl:attribute name="data-thread">
				<xsl:value-of select="/sage:view/sage:request/@thread"/>
			</xsl:attribute>
			<xsl:attribute name="data-basehref">
				<xsl:value-of select="/sage:view/sage:request/@basehref"/>
			</xsl:attribute>

			<div class="background"></div>
			<div class="content">
				<div class="icon tooltip" title="Unknown status">
					<xsl:attribute name="data-hideOn">
						<xsl:text>click, blur</xsl:text>
					</xsl:attribute>
					<xsl:attribute name="data-useFades">
						<xsl:text>true</xsl:text>
					</xsl:attribute>
				</div>
				<div class="status">
					<span class="text">
						<label></label>
					</span>
					<span class="time tooltip">
						<label></label>
					</span>
				</div>
				<div class="commands">
					<span class="group meta">
						<xsl:for-each select="mod:data/mod:meta/mod:view">
							<span class="button {@name} tooltip" title="{.}">
								<xsl:attribute name="data-meta">
									<xsl:value-of select="@name"/>
								</xsl:attribute>
								<label><xsl:value-of select="@name"/></label>
							</span>
						</xsl:for-each>
					</span>
					<span class="group fixed">
						<span class="button inspect tooltip" title="View this page in view inspector">
							<label>inspect</label>
						</span>
						<span class="button log tooltip" title="View server side log of this page">
							<label>log</label>
						</span>
					</span>
				</div>
			</div>
		</div>
	</xsl:template>

</xsl:stylesheet>
