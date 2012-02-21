<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:param name="problemType" select="0"/>
	<xsl:param name="path"/>
	
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
					h3
						{ font-size: 22px; margin: 0 0 5px 0; font-weight: normal; color: #c00; }
					p
						{ margin: 0 0 15px 0; }
					code
						{	font-family: Consolas, Courier; display: inline-block; background: #eaeaea; 
						  padding: 1px 3px; border-radius: 3px; margin-bottom: 1px; }
					
					.pre
						{ white-space: pre; }
					
					.problem-suggestion
						{ margin-bottom: 40px; }
					.problem-suggestion p
						{ font-size: 120%; color: #666; }
					.problem-location
						{ margin-bottom: 0px; }
						
					.problem-description
						{ margin-bottom: 20px; }
					.problem-file
						{ margin-bottom: 20px; }
					.problem-source
						{	color: #009966; }
					.problem-message
						{ font-weight: bold; margin-bottom: 5px; }
					.problem-message.inner
						{ font-size: 85%; color: #666; font-weight: normal; margin-top: -5px; }
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
				<xsl:apply-templates select="." mode="problem-message"/>
				<xsl:if test="string-length(@sourceuri) or $path">
					<div class="problem-file">
						File:
						<span class="problem-source">
							<xsl:choose>
								<xsl:when test="$path">
									<xsl:value-of select="$path"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="@sourceuri"/>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:text>.</xsl:text>
						</span>
					</div>
				</xsl:if>
				<div class="problem-description">
					<xsl:apply-templates select="." mode="problem-description"/>
				</div>
				<h3>Suggestion:</h3>
				<div class="problem-suggestion">
					<xsl:apply-templates select="." mode="problem-suggestion"/>
				</div>
				<div class="problem-details">
					<span class="switch">(<a href="javascript:expandProblemDetail()">Click to show technical details about this error</a>)</span>
					<div class="content" id="problem-details-content">
						<xsl:apply-templates select="." mode="developer"/>
					</div>
				</div>
			</body>
		</html>
	</xsl:template>
	
	<xsl:template match="/exception" mode="problem-message">
		<div class="problem-message">
			<label>Message:</label>
			<span class="text">
				<xsl:value-of select="@htmlDescription" disable-output-escaping="yes"/>
			</span>
		</div>
		<xsl:apply-templates select="exception" mode="problem-message"/>
	</xsl:template>

	<xsl:template match="exception/exception" mode="problem-message">
		<div class="problem-message inner">
			(<xsl:value-of select="@htmlDescription" disable-output-escaping="yes"/>)
		</div>
	</xsl:template>
	
	<xsl:template match="exception" mode="problem-title">
		<xsl:choose>
			<xsl:when test="$problemType = 'InvalidMarkup'">
				Invalid Markup Detected
			</xsl:when>
			<xsl:when test="$problemType = 'InvalidHtmlMarkup'">
				Invalid HTML Markup Detected
			</xsl:when>
			<xsl:when test="$problemType = 'MissingNamespaceDeclaration'">
				Missing namespace declaration
			</xsl:when>
			<xsl:when test="$problemType = 'MissingConfigurationFile'">
				No configuration file present
			</xsl:when>
			<xsl:when test="$problemType = 'ConfigurationMissingLocales'">
				At least one locale needs to be configured
			</xsl:when>
			<xsl:when test="$problemType = 'ConfigurationMissingCategories'">
				At least one category needs to be configured
			</xsl:when>
			<xsl:when test="$problemType = 'TransformResultMissingRootElement'">
				Transform result missing
			</xsl:when>
			<xsl:when test="$problemType = 'TransformError'">
				Transform error
			</xsl:when>
			<xsl:when test="$problemType = 'XsltLoadError'">
				XSLT load error
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeNotFound'">
				The specified include file was not found
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeFragmentNotFound'">
				The specified include subresource was not found
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeSyntaxError'">
				Syntax error in xpointer expression
			</xsl:when>
			<xsl:otherwise>An error was caught</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template match="exception" mode="problem-description">
		<xsl:choose>
			<xsl:when test="$problemType = 'InvalidMarkup' or $problemType = 'InvalidHtmlMarkup'">
				Sage speaks XML, and therefore any XML needs to be valid. <br/>
				HTML templates must parse as XHTML in order for them to use usable.
			</xsl:when>
			<xsl:when test="$problemType = 'MissingNamespaceDeclaration'">
				One or more elements in the XML document are using a qualified name without specifying the name's 
				corresponding namespace.
			</xsl:when>
			<xsl:when test="$problemType = 'MissingConfigurationFile'">
				No configuration file has been found.
			</xsl:when>
			<xsl:when test="$problemType = 'ConfigurationMissingLocales'">
				A Sage project needs to have one default locale in order to function correctly. Neither the system
				configuration (System.config) not the project configuration (Project.config) defined any locales.
			</xsl:when>
			<xsl:when test="$problemType = 'ConfigurationMissingCategories'">
				The current project is configured as multicategory <code><![CDATA[<project .. multiCategory="true">...</project>]]></code>,
				but there are no categories defined.
			</xsl:when>
			<xsl:when test="$problemType = 'TransformResultMissingRootElement'">
				The XSLT transform produced no result.
			</xsl:when>
			<xsl:when test="$problemType = 'TransformError'">
				An error happened during the XSLT transformation. 
			</xsl:when>
			<xsl:when test="$problemType = 'XsltLoadError'">
				An error happened during loading of the XSLT stylesheet.
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeNotFound'">
				The include resource that was specified in the <code>href</code> attribute doesn't exist.
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeFragmentNotFound'">
				The include subresource that was specified with the <code>xpointer</code> attribute doesn't exist.
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeSyntaxError'">
				The expression specified with the <code>xpointer</code> attribute contains a syntax error.
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template match="exception" mode="problem-suggestion">
		<xsl:choose>
			<xsl:when test="$problemType = 'InvalidMarkup' or $problemType = 'InvalidHtmlMarkup'">
				Look in the source document for any unclosed tags, attribute quotes or unknown character entities.
				<ul>
					<li>
						<label>The document must have at least one (root) element: </label>
						<code class="block"><![CDATA[<html></html>]]></code>
					</li>
					<li>
						<label>Tag pairs must be properly nested: </label>
						<code class="block"><![CDATA[<b><span>text</span></b>]]></code>
					</li>
					<li>
						<label>Single tags must be properly closed: </label>
						<code class="block"><![CDATA[<br />]]></code>
					</li>
					<li>
						<label>Element attributes must be unique, and must be quoted properly: </label> 
						<code><![CDATA[comment="This is &quot;great&quot"]]></code>
					</li>
					<li>
						<label>XML recognizes only a limited set of entities by a name: </label> 
						<code><![CDATA[&quot; (")]]></code>&#160;
						<code><![CDATA[&apos; (')]]></code>&#160; 
						<code><![CDATA[&amp; (&)]]></code>&#160; 
						<code><![CDATA[&lt; (<)]]></code> and 
						<code><![CDATA[&gt; (>)]]></code>
					</li>
					<li>
						<label>Any other entities must be expressed numerically: </label> 
						<code><![CDATA[&#160;]]></code> instead of <code><![CDATA[&nbsp;]]></code> etc.
					</li>
					<li>
						<label>Of course, the document can also be UTF-8 encoded and any entities can be embedded as they are, 
						without having to use codes.</label> 
					</li>
				</ul>
			</xsl:when>
			<xsl:when test="$problemType = 'MissingNamespaceDeclaration'">
				<p>Add the namespace declaration to the top of the document.</p>
				
				For instance, to use X-Include tags with namespace prefix 'xi': 
				<code>&lt;xi:include href="myinclude.html" /&gt;</code><br/>
				
				Add the 'xmlns:xi' attribute to the document's root element: 
				<code>&lt;document xmlns:xi="http://www.w3.org/2003/XInclude" ... &gt;</code>
			</xsl:when>
			<xsl:when test="$problemType = 'MissingConfigurationFile'">
				<p>Sage needs a configuration file in order to work. Make sure either System.config (configuration bundled with Sage), 
				Project.config (your own specific configuration) or both exist in the same directory from which Sage is being run 
				(typically the 'bin' directory of the web application running Sage)</p>
			</xsl:when>
			<xsl:when test="$problemType = 'ConfigurationMissingLocales'">
				Edit the Project.config file and make sure it contains the globalization element
				with at least one locale. For exampe:<br/>
<code class="pre"><![CDATA[<locale name="en" dictionaryNames="en" resourceNames="default">
	<format culture="en-us" shortDate="MMMM d, yyyy" longDate="D"/>
</locale>]]></code>
			</xsl:when>
			<xsl:when test="$problemType = 'ConfigurationMissingCategories'">
				Edit the Project.config file and make sure it contains at least one category:
<code class="pre"><![CDATA[<categories>
	<category
		name="running"
		locales="ae,ar,at,au,be,br,ca,cf,ch,cn,com,de,dk,es,fi,fr,gr,hk,hu,id,in,it,jp,kr,la,my,nl,no,nz,ph,pl,pt,ru,se,sg,th,tw,uk,us,vn,za"
		/>
</categories>]]></code>
			</xsl:when>
			<xsl:when test="$problemType = 'TransformResultMissingRootElement'">
				Make sure that the XSLT you use is contains the necessary template to process the 
				supplied input document.
			</xsl:when>
			<xsl:when test="$problemType = 'TransformError' or $problemType = 'XsltLoadError'">
				Read the message and try to resolve the problem.
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeNotFound'">
				Check the value of the <code>xi:include</code> element's <code>href</code> attribute
				and make sure if has been entered correctly, and that the resource it points to exists.
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeFragmentNotFound'">
				Check the value of the <code>xi:include</code> element's <code>xpointer</code> attribute
				and make sure it has been entered correctly, and that the subresource it points to exists.
			</xsl:when>
			<xsl:when test="$problemType = 'IncludeSyntaxError'">
				Check the value of the <code>xi:include</code> element's <code>xpointer</code> attribute
				and make sure it has been entered without errors.
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	
</xsl:stylesheet>
