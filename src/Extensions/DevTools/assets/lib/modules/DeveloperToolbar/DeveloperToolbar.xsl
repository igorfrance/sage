<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:mod="http://www.cycle99.com/schemas/sage/modules.xsd"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml">

	<xsl:template match="mod:DeveloperToolbar">
		<div id="developer-toolbar"
				data-basehref="{/sage:view/sage:request/@basehref}"
				data-thread="{/sage:view/sage:request/@thread}">

			<div class="background"></div>
			<div class="content">
				<div class="icon tooltip" data-hideOn="click, blur" data-useFades="true" title="Unknown status"></div>
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
							<span class="button {@name} tooltip" title="{.}" data-meta="{@name}">
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
