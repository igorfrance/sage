<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:logic="http://www.cycle99.com/projects/sage/logic"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="logic xhtml">

	<xsl:template match="logic:conditions | logic:condition"/>

	<xsl:template match="*[@logic:if]">
		<xsl:variable name="valid">
			<xsl:apply-templates select="@logic:if" mode="isTrue"/>
		</xsl:variable>
		<xsl:if test="$valid = 1">
			<xsl:element name="{name()}" namespace="{namespace-uri()}">
				<xsl:apply-templates select="@*[namespace-uri() != 'http://www.cycle99.com/projects/sage/logic']"/>
				<xsl:apply-templates select="node()"/>
			</xsl:element>			
		</xsl:if>
	</xsl:template>

	<xsl:template match="*[@logic:ifnot]">
		<xsl:variable name="valid">
			<xsl:apply-templates select="@logic:ifnot" mode="isTrue"/>
		</xsl:variable>
		<xsl:if test="$valid = 0">
			<xsl:element name="{name()}" namespace="{namespace-uri()}">
				<xsl:apply-templates select="@*[namespace-uri() != 'http://www.cycle99.com/projects/sage/logic']"/>
				<xsl:apply-templates select="node()"/>
			</xsl:element>			
		</xsl:if>
	</xsl:template>
	
	<xsl:template match="logic:switch">
		<xsl:variable name="index">
			<xsl:call-template name="choose">
				<xsl:with-param name="selection" select="*"/>
				<xsl:with-param name="position" select="1"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:apply-templates select="*[number($index)]/node()"/>
	</xsl:template>

	<xsl:template name="choose">
		<xsl:param name="selection"/>
		<xsl:param name="position"/>

		<xsl:choose>
			<xsl:when test="local-name($selection[$position]) = 'default'">
				<xsl:value-of select="$position"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="valid">
					<xsl:apply-templates select="$selection[$position]/@when" mode="isTrue"/>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$valid=1">
						<xsl:value-of select="$position"/>
					</xsl:when>
					<xsl:when test="$position &lt; count($selection)">
						<xsl:call-template name="choose">
							<xsl:with-param name="selection" select="*"/>
							<xsl:with-param name="position" select="$position + 1"/>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>-1</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@when | @ref | @logic:if | @logic:ifnot" mode="isTrue">
		<xsl:variable name="conditions" select="//logic:conditions/logic:and | //logic:conditions/logic:or"/>
		<xsl:choose>
			<xsl:when test="/sage:view/sage:request/@debug = '1'">
				<xsl:choose>
					<xsl:when test="$conditions[@id=current()]">
						<xsl:apply-templates select="$conditions[@id=current()]" mode="isTrue"/>
					</xsl:when>
					<xsl:otherwise>
						<!-- Preserve the plain text formatting for the error message: -->
						<xsl:message terminate="yes"
>Missing condition '<xsl:value-of select="."/>'. <xsl:choose>
<xsl:when test="count($conditions) = 0">
The current document contains no logical definition elements. In order to use conditon with name '<xsl:value-of select="."/>', try specifying something like:
&lt;logic:conditions>
	&lt;logic:and id="<xsl:value-of select="."/>">
		&lt;logic:condition object="..." property="..." equals="..." />
		...
	&lt;/logic:and>
&lt;/logic:conditions>
</xsl:when>
<xsl:otherwise>
Current document contains these logical definition elements:

<xsl:for-each select="$conditions">
	<xsl:value-of select="concat('&#160;&#160;&#160;- ', @id, ' (', local-name(), ')&#10;')"/>
</xsl:for-each>
</xsl:otherwise>
</xsl:choose>
(This exception can be disabled by setting the 'debugMode' property of the project configuration to false.)
</xsl:message>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="$conditions[@id=current()]" mode="isTrue"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="logic:and" mode="isTrue">
		<xsl:variable name="result">
			<xsl:apply-templates select="logic:condition" mode="isTrue"/>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="contains($result, '0')">0</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="logic:or" mode="isTrue">
		<xsl:variable name="result">
			<xsl:apply-templates select="*" mode="isTrue"/>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="contains($result, '1')">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="logic:or[@ref] | logic:and[@ref]" mode="isTrue">
		<xsl:apply-templates select="@ref" mode="isTrue"/>
	</xsl:template>

	<xsl:template match="logic:condition[@object='useragent']" mode="isTrue">
		<xsl:apply-templates select="@*" mode="evaluate">
			<xsl:with-param name="selection" select="//sage:useragent/@*[name() = current()/@property]"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="logic:condition[@object='querystring']" mode="isTrue">
		<xsl:apply-templates select="@*" mode="evaluate">
			<xsl:with-param name="selection" select="//sage:querystring/@*[name() = current()/@property]"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="logic:condition[@object='cookie']" mode="isTrue">
		<xsl:apply-templates select="@*" mode="evaluate">
			<xsl:with-param name="selection" select="//sage:cookie/@*[name() = current()/@property]"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="logic:condition[@object='address']" mode="isTrue">
		<xsl:apply-templates select="@*" mode="evaluate">
			<xsl:with-param name="selection" select="//sage:address/@*[name() = current()/@property]"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="logic:condition[@object='dateTime']" mode="isTrue">
		<xsl:apply-templates select="@*" mode="evaluate">
			<xsl:with-param name="selection" select="//sage:dateTime/@*[name() = current()/@property]"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="logic:condition" mode="isTrue">
		<xsl:param name="selection"/>
		<xsl:apply-templates select="@*" mode="evaluate">
			<xsl:with-param name="selection" select="$selection"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="@*" mode="evaluate"/>
	
	<xsl:template match="@equals" mode="evaluate">
		<xsl:param name="selection"/>
		<xsl:choose>
			<xsl:when test="$selection = .">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@not" mode="evaluate">
		<xsl:param name="selection"/>
		<xsl:choose>
			<xsl:when test="$selection != .">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template match="@count" mode="evaluate">
		<xsl:param name="selection"/>
		<xsl:choose>
			<xsl:when test="starts-with(., 'not(')">
				<xsl:variable name="value" select="substring(., 5, string-length(.) - 5)"/>
				<xsl:choose>
					<xsl:when test="count($selection) != $value">1</xsl:when>
					<xsl:otherwise>0</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="count($selection) = .">1</xsl:when>
					<xsl:otherwise>0</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@greaterThan" mode="evaluate">
		<xsl:param name="selection"/>
		<xsl:choose>
			<xsl:when test="$selection > .">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@greaterThenOrEqual" mode="evaluate">
		<xsl:param name="selection"/>
		<xsl:choose>
			<xsl:when test="$selection >= .">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@lessThan" mode="evaluate">
		<xsl:param name="selection"/>
		<xsl:choose>
			<xsl:when test="$selection &lt; .">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@lessThanOrEqual" mode="evaluate">
		<xsl:param name="selection"/>
		<xsl:choose>
			<xsl:when test="$selection &lt;= .">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@*" mode="evaluate"/>
	
</xsl:stylesheet>
