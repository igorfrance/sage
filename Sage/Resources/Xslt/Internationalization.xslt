<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	xmlns:sage="http://www.cycle99.com/schemas/sage/sage.xsd"
	xmlns:kelp="http://www.cycle99.com/projects/kelp"
	xmlns:intl="http://www.cycle99.com/schemas/sage/internationalization.xsd"
	xmlns="http://www.w3.org/1999/xhtml"

	exclude-result-prefixes="sage intl msxsl">

	<xsl:param name="mode" select="'translate'"/>
	<xsl:param name="locale" select="'en'"/>
	<!--
	<xsl:param name="dictionary" select="document('test-dictionary.xml')/*"/>
	<xsl:param name="fallbacks" select="document('test-fallbacks.xml')/*"/>
	<xsl:param name="globalvariables" select="document('test-variables.xml')/*"/>
	-->
	<xsl:param name="dictionary"/>
	<xsl:param name="fallbacks"/>
	<xsl:param name="globalvariables" select="."/>
	<xsl:param name="categoryvariables" select="."/>

	<!--

		Features we support:

			- Insertion of locale-specific text nodes through the use of <intl:phrase ref=""/> nodes
			- Insertion of locale-specific attribute values through the use of intl:phrase('xyz') attribute values
			- Rendering nodes for specific locales only through the use of @intl:locale="locale1,locale2..." and @intl:locale="not(locale)" attribute
			- Insertion of locale-specific value nodes through the use of <intl:variable ref=""/> nodes
			- Insertion of locale-specific attribute values through the use of intl:variable('xyz') attribute values
			- Conditional blocks
					intl:localization
						intl:if locale="jp,us,fr"
						intl:if locale="de"
						intl:if locale="not(uk)"
						intl:else
			- Insertion of various locale/category specific values
					intl:value[@type='locale']

	-->

	<xsl:template match="/">
		<xsl:choose>
			<xsl:when test="$mode = 'diagnose'">
				<xsl:element name="intl:diagnostics">
					<xsl:apply-templates select="node()"/>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="node()"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="*">
		<xsl:choose>
			<xsl:when test="$mode = 'translate'">
				<xsl:element name="{name()}" namespace="{namespace-uri()}">
					<xsl:apply-templates select="@*"/>
					<xsl:apply-templates select="node()"/>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="node()"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@*">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()">
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template match="*[@intl:locale]">
		<xsl:if test="sage:localeMatchesCondition($locale, @intl:locale, $fallbacks/locale)">
			<xsl:element name="{name()}" namespace="{namespace-uri()}">
				<xsl:apply-templates select="@*"/>
				<xsl:apply-templates select="node()"/>
			</xsl:element>
		</xsl:if>
	</xsl:template>

	<xsl:template match="intl:localize">
		<xsl:variable name="validnodes" select="intl:if[sage:localeMatchesCondition($locale, @locale, $fallbacks/locale)]"/>
		<xsl:choose>
			<xsl:when test="count($validnodes) != 0">
				<xsl:apply-templates select="$validnodes[1]"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="intl:else"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="intl:value[@type='locale']">
		<xsl:value-of select="$locale"/>
	</xsl:template>

	<xsl:template match="intl:if | intl:else">
		<xsl:apply-templates select="node()"/>
	</xsl:template>

	<xsl:template match="intl:phrase[@ref]">
		<xsl:variable name="phrase" select="$dictionary/intl:phrase[@id=current()/@ref]"/>
		<xsl:choose>
			<xsl:when test="$mode = 'diagnose'">
				<xsl:element name="phrase">
					<xsl:attribute name="ref"><xsl:value-of select="current()/@ref"/></xsl:attribute>
					<xsl:attribute name="source"><xsl:value-of select="$phrase/@source"/></xsl:attribute>
				</xsl:element>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="$phrase">
						<xsl:apply-templates select="$phrase"/>
					</xsl:when>
					<xsl:otherwise>
						{<xsl:value-of select="@ref"/>}
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="intl:variable[@ref]">
		<xsl:variable name="variables" select="ancestor::*/intl:variables/intl:variable[@id=current()/@ref]"/>
		<xsl:variable name="variable" select="$variables[last()]"/>
		<xsl:variable name="globalvar" select="$globalvariables/intl:variable[@id=current()/@ref]"/>
		<xsl:variable name="categoryvar" select="$categoryvariables/intl:variable[@id=current()/@ref]"/>
		<xsl:choose>
			<xsl:when test="$variable">
				<xsl:choose>
					<xsl:when test="$mode = 'diagnose'">
						<xsl:apply-templates select="$variable" mode="diagnose"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="$variable" mode="substitute"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="$categoryvar">
				<xsl:choose>
					<xsl:when test="$mode = 'diagnose'">
						<xsl:apply-templates select="$categoryvar" mode="diagnose"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="$categoryvar" mode="substitute"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="$globalvar">
				<xsl:choose>
					<xsl:when test="$mode = 'diagnose'">
						<xsl:apply-templates select="$globalvar" mode="diagnose"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="$globalvar" mode="substitute"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				{<xsl:value-of select="@ref"/> (global vars: <xsl:value-of select="$globalvariables[1]/@id"/>)}
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="intl:phrase[@id]">
		<xsl:apply-templates select="node()"/>
	</xsl:template>

	<xsl:template match="intl:variables"/>

	<xsl:template match="intl:variable[@id]"/>

	<xsl:template match="intl:variable[@id]" mode="diagnose">
		<intl:variable ref="{@id}">
			<xsl:attribute name="source">
				<xsl:apply-templates select="." mode="substitute">
					<xsl:with-param name="diagnose" select="1"/>
				</xsl:apply-templates>
			</xsl:attribute>
			<xsl:apply-templates select="." mode="substitute"/>
		</intl:variable>
	</xsl:template>

	<xsl:template match="intl:variable[@id]" mode="substitute">
		<xsl:param name="diagnose" select="0"/>
		<xsl:variable name="fallbackLocale" select="$fallbacks/locale[.=current()/intl:value/@locale][1]/text()"/>
		<xsl:choose>
			<xsl:when test="intl:value[@locale=$locale]">
				<xsl:choose>
					<xsl:when test="$diagnose = 1">
						<xsl:value-of select="$locale"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="intl:value[@locale=$locale]/node()"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="intl:value[@locale=$fallbackLocale]">
				<xsl:choose>
					<xsl:when test="$diagnose = 1">
						<xsl:value-of select="$fallbackLocale"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="intl:value[@locale=$fallbackLocale]/node()"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="intl:value[@locale='default']">
				<xsl:choose>
					<xsl:when test="$diagnose = 1">
						<xsl:text>default</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="intl:value[@locale='default']/node()"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="$diagnose = 1">
						<xsl:text>text</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="node()[name() != 'intl:value']"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@*[starts-with(., 'intl:phrase(')]">
		<xsl:variable name="phraseID" select="substring(., 13, string-length(.) - 14)"/>
		<xsl:variable name="phrase" select="$dictionary/intl:phrase[@id=$phraseID]"/>
		<xsl:choose>
			<xsl:when test="$phrase">
				<xsl:value-of select="$phrase"/>
			</xsl:when>
			<xsl:otherwise>
				{<xsl:value-of select="@ref"/>}
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="@*[starts-with(., 'intl:variable(')]">
		<xsl:variable name="variableId" select="substring(., 15, string-length(.) - 15)"/>
		<xsl:variable name="variables" select="ancestor::*/intl:variables/intl:variable[@id=$variableId]"/>
		<xsl:variable name="variable" select="$variables[last()]"/>
		<xsl:variable name="globalvar" select="$globalvariables/intl:variable[@id=$variableId]"/>
		<xsl:variable name="categoryvar" select="$categoryvariables/intl:variable[@id=$variableId]"/>
		<xsl:variable name="value">
			<xsl:choose>
				<xsl:when test="$variable">
					<xsl:apply-templates select="$variable" mode="substitute"/>
				</xsl:when>
				<xsl:when test="$categoryvar">
					<xsl:apply-templates select="$categoryvar" mode="substitute"/>
				</xsl:when>
				<xsl:when test="$globalvar">
					<xsl:apply-templates select="$globalvar" mode="substitute"/>
				</xsl:when>
				<xsl:otherwise>
					{<xsl:value-of select="$variableId"/>}
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:attribute name="{name()}">
			<xsl:value-of select="$value"/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="@intl:locale"/>

	<msxsl:script language="c#" implements-prefix="sage"><![CDATA[

		public bool localeMatchesCondition(string locale, string criteria, XPathNodeIterator fallbackNodes)
		{
			if (locale == criteria)
				return true;

			if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(criteria))
				return false;

			bool reverse = criteria.StartsWith("not(") ? true : false;
			string[] locales = (reverse ? criteria.Substring(4, criteria.Length - 5) : criteria).Split(new char[] { ',' });
			bool matches = false;
			foreach (string test in locales)
			{
				string testLocale = test.Trim();
				if (testLocale == locale || localeIsSubsetOf(testLocale, fallbackNodes))
				{
					matches = true;
					break;
				}
			}

			return reverse ? !matches : matches;
		}

		public bool localeIsSubsetOf(string locale, XPathNodeIterator fallbackNodes)
		{
			while (fallbackNodes.MoveNext())
			{
				if (fallbackNodes.Current.Value == locale)
					return true;
			}

			return false;
		}

	]]></msxsl:script>

	<xsl:template match="sage:literal">
		<xsl:apply-templates select="." mode="preserve"/>
	</xsl:template>

	<xsl:template match="*" mode="preserve">
		<xsl:element name="{name()}" namespace="{namespace-uri()}">
			<xsl:apply-templates select="@*" mode="preserve"/>
			<xsl:apply-templates select="node()" mode="preserve"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*" mode="preserve">
		<xsl:attribute name="{name()}" namespace="{namespace-uri()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()" mode="preserve">
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template match="comment()" mode="preserve">
		<xsl:comment>
			<xsl:value-of select="."/>
		</xsl:comment>
	</xsl:template>

</xsl:stylesheet>