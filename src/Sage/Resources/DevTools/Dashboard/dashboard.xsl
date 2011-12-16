<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:x="http://www.cycle99.com/projects/sage/dev"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules">

	<xsl:param name="pageToLoad" select="/request/data/value[@id='pageToLoad']"/>
	<xsl:param name="toolToLoad" select="/request/data/value[@id='toolToLoad']"/>
	<xsl:param name="viewtree" select="/request/data/tree"/>
	<xsl:param name="customTools" select="/request/data/commands"/>

	<xsl:variable name="cookies" select="/request/cookies"/>

	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"
		doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>

	<xsl:template match="/">
		<html>
			<head>
				<title>Sage Developer Dashboard</title>
				<base href="{request/@basehref}"/>
				<link rel="stylesheet" type="text/css" href="{DevTools}/dashboard/devtools.css" />
				<script type="text/javascript" src="{DevTools}/dashboard/aeon.js"></script>
				<script type="text/javascript" src="{DevTools}/dashboard/devtools.js"></script>
				<script type="text/javascript">

					var pageToLoad = "<xsl:value-of select="$pageToLoad"/>";
					var toolToLoad = "<xsl:value-of select="$toolToLoad"/>";

				</script>
			</head>
			<body class="frame">
				<div id="dev_wrapper">
					<xsl:attribute name="class">
						<xsl:choose>
							<xsl:when test="$cookies/@COMMANDS = 'ON'">dashboard</xsl:when>
							<xsl:otherwise></xsl:otherwise>
						</xsl:choose>
					</xsl:attribute>
					<div id="dev_commands">
						<div class="header">
							<h2>Commands</h2>
						</div>
						<div id="command_tree">
							<div class="node root lg">
								<div class="header">
									<div class="control minus"></div>
									<div class="icon"></div>
									<div class="title">Commands</div>
								</div>
								<div class="children">
									<xsl:for-each select="$viewtree/*">
										<xsl:variable name="pos">
											<xsl:choose>
												<xsl:when test="position() = last()">l</xsl:when>
												<xsl:otherwise>s</xsl:otherwise>
											</xsl:choose>
										</xsl:variable>
										<xsl:variable name="type">
											<xsl:choose>
												<xsl:when test="name() = 'folder' or @items > 0">g</xsl:when>
												<xsl:otherwise>s</xsl:otherwise>
											</xsl:choose>
										</xsl:variable>
										<div class="node {$pos}{$type} {name()}" path="{@path}">
											<div class="header">
												<div class="control plus"></div>
												<div class="icon {name()}"></div>
												<div class="title">
													<xsl:value-of select="@name"/>
												</div>
											</div>
											<div class="children"></div>
										</div>
									</xsl:for-each>
								</div>
							</div>
						</div>
					</div>
					<div id="dev_content">
						<div id="maintoolbar">
							<div class="toolbar">
								<div class="button global views tooltip" x:command="ToggleViews" title="Toggle controller/view list"></div>
							</div>
							<div class="toolbar">
								<div class="button global singleframe tooltip" x:command="SingleFrame" title="Switch to layout without tools enabled"></div>
								<div class="button global doubleframe tooltip" x:command="DoubleFrame" title="Switch to layout with tools enabled"></div>
								<div class="spacer v10"></div>
							</div>
							<div class="toolbar">
								<div class="button global verticalframes tooltip" x:command="VerticalFrames" title="Switch to vertical tool-enabled layout"></div>
								<div class="button global horizontalframes tooltip" x:command="HorizontalFrames" title="Switch to horizontal tool-enabled layout"></div>
							</div>
							<div class="toolbar commands">
								<xsl:for-each select="$customTools/command">
									<div class="button global command tooltip {@id}" x:command="SetView" x:arguments="{@url}" title="{@title}"></div>
								</xsl:for-each>
							</div>
						</div>
						<div id="contentbox" class="horizontal double">
							<xsl:attribute name="class">
								<xsl:choose>
									<xsl:when test="$cookies/@OR = 'VERTICAL'">vertical </xsl:when>
									<xsl:otherwise>horizontal </xsl:otherwise>
								</xsl:choose>
								<xsl:choose>
									<xsl:when test="$cookies/@LY = 'DOUBLE'">double </xsl:when>
									<xsl:otherwise>single </xsl:otherwise>
								</xsl:choose>
							</xsl:attribute>
							<div id="content_container">
								<div id="contenttoolbar">
									<div class="toolbar right">
										<div class="button tool reload tooltip" x:command="Reload" title="Reload the current page"></div>
									</div>
								</div>
								<div id="contentframe">
									<iframe src="about:blank" frameborder="0" width="100%" height="100%"></iframe>
								</div>
								<div id="contentpreloader"></div>
							</div>
							<div id="tools_container">
								<div id="viewtoolbar">
									<div class="toolbar right">
										<div class="button tool log tooltip" x:command="SetTool" x:arguments="log" title="View request log"></div>
										<div class="button tool intl tooltip" x:command="SetTool" x:arguments="intl" title="View globalization summary"></div>
										<div class="button tool xml tooltip" x:command="SetTool" x:arguments="xml" title="View XML input"></div>
										<div class="button tool xmlx tooltip" x:command="SetTool" x:arguments="xmlx" title="View fancy XML input"></div>
									</div>
								</div>
								<div id="toolframe">
									<iframe src="about:blank" frameborder="0" width="100%" height="100%"></iframe>
								</div>
								<div id="toolpreloader"></div>
							</div>
						</div>
					</div>
				</div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="categories" mode="commandlist">
		<xsl:for-each select="category">
			<div class="category {@name}">
				<h5>
					<xsl:value-of select="@name"/>
				</h5>
				<xsl:apply-templates select="mod:navigation/mod:links" mode="commandlist"/>
			</div>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="mod:links" mode="commandlist">
		<ul>
			<xsl:apply-templates select="mod:link" mode="commandlist"/>
		</ul>
	</xsl:template>

	<xsl:template match="mod:link" mode="commandlist">
		<li>
			<div class="link" x:command="SetView" x:arguments="{@href}">
				<span>
					<xsl:apply-templates select="mod:title" mode="commandlist"/>
				</span>
			</div>
			<xsl:apply-templates select="mod:links" mode="commandlist"/>
		</li>
	</xsl:template>

	<xsl:template match="mod:title" mode="commandlist">
		<span>
			<xsl:apply-templates />
		</span>
	</xsl:template>

</xsl:stylesheet>
