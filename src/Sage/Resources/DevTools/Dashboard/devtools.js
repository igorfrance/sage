Type.registerNamespace("sage.dev");

/**
 * Implements a developer frame page that provides several useful development utilities.
 */
sage.dev.Tools = new function DevTools()
{
	/**
	 * @enum
	 */
	var LAYOUT =
		{ SINGLE: 1, DOUBLE: 2 };

	/**
	 * @enum
	 */
	var ORIENTATION =
		{ HORIZONTAL: 1, VERTICAL: 2 };

	/**
	 * @enum
	 */
	var BUTTONSTATE =
		{ DISABLED: 0, ENABLED: 1, ACTIVE: 2 };

	/**
	 * @type {HTMLElement}
	 */
	var contentFrame;
	/**
	 * @type {HTMLElement}
	 */
	var toolFrame;
	/**
	 * @type {String}
	 */
	var currentTool = "log";
	/**
	 * @type {LAYOUT}
	 */
	var currentLayout = LAYOUT.SINGLE;
	/**
	 * @type {ORIENTATION}
	 */
	var currentOrientation = ORIENTATION.VERTICAL;
	/**
	 * @type {String}
	 */
	var currentView = null;
	/**
	 * @type {Boolean}
	 */
	var commandsVisible = false;
	/**
	 * @type {Boolean}
	 */
	var commandsLoaded = false;
	/**
	 * @type {Object}
	 */
	var tools =
	{
		log:	{ name: "log",	  url: "dev/log/{thread}/{source}" },
		routes: { name: "routes", url: "dev/routes/?url={page}" },
		xml:	{ name: "xml",	  url: "{url}{?}view=xml" },
		xmlx:   { name: "xmlx",   url: "{url}{?}view=xmlx" },
		intl:   { name: "intl",   url: "dev/intl/?url={url}" },
		save:   { name: "save",   url: "dev/save/?url={url}" }
	};

	var commandDataPath = "dev/views/tree/{0}";

	var TITLE_DEFAULT = document.title;
	var TITLE_PATTERN = "{0} - {1}";

	var isResizeReady = false;
	var isResizeActive = false;

	var dragClientX = 0;
	var dragClientY = 0;

	var contentHeight = 0;
	var contentWidth = 0;

	var updateTimeout = 500;
	var commands = null;
	var loadingUrl = null;
	var loadedUrl = null;

	/**
	 * Sets up the developer tools.
	 */
	function setup()
	{
		if (top.window.location != document.location)
			top.window.location = document.location;

		toolFrame = $("#toolframe iframe");
		contentFrame = $("#contentframe iframe");

		commands = $(".button, .link");
		commands.click(onButtonClicked);

		$("#dev_commands .control").click(onTreeNodeClicked);
		$("#dev_commands .node.file > .header .title").click(onTreeFileClicked);

		currentTool = $url.getHashParam("tool");
		if (!currentTool && window.toolToLoad)
			currentTool = window.toolToLoad;
		if (!currentTool)
			currentTool = "log";

		commandsVisible = $("#dev_wrapper").hasClass("dashboard");
		currentLayout = $("#contentbox").hasClass("double")
			? LAYOUT.DOUBLE
			: LAYOUT.SINGLE;
		currentOrientation = $("#contentbox").hasClass("horizontal")
			? ORIENTATION.HORIZONTAL
			: ORIENTATION.VERTICAL;

		var targetUrl = currentView = getTargetUrl();
		if (!targetUrl && window.pageToLoad)
		{
			currentView = window.pageToLoad;
			setTargetUrl(window.pageToLoad);
		}

		updateControls();
		updateLoop();

		$("#viewtoolbar").bind("mousedown", onViewTolbarMouseDown);

		$(document).bind("mouseup", onDocumentMouseUp);
		$(document).bind("mousemove", onDocumentMouseMove);

		contentFrame.bind("load", onContentFrameLoaded);
		toolFrame.bind("load", onToolFrameLoaded);
	};

	function onTreeFileClicked()
	{
		var node = $(this).closest("div.node");
		var target = node.attr("target");

		setTargetUrl(target);
	}

	function onTreeNodeClicked()
	{
		var node = $(this).closest("div.node");
		var path = node.attr("path");
		var target = node.find("> .children");
		var control = node.find("> .header .control");

		if (target.html().trim() == String.EMPTY)
		{
			target.append("<div class='loading'></div>");
			$.get(commandDataPath.format(path), function onTreeLoaded(responseText)
			{
				function createTreeNode(children, parent)
				{
					for (var i = 0; i < children.length; i++)
					{
						var item = children.eq(i);
						var count = parseInt(item.attr("items")) || 0;
						var pos = i == children.length - 1 ? "l" : "s";
						var type = item.nodeName == "folder" || count > 0 ? "g" : "s";

						var node = $('<div class="node {0}{1} {2}" path="{3}" target="{4}"></div>'.format(pos, type,
							item[0].nodeName, item.attr("path") || String.EMPTY, item.attr("url")));

						var header = $('<div class="header"></div>');
						header.append('<div class="control {0}"></div>'.format(type == "g" ? " plus" : String.EMPTY));
						header.append('<div class="icon {0}"></div>'.format(item[0].nodeName));
						if (item.attr("url"))
							header.append('<div class="title"><a href="{0}" onclick="return false">{1}</a></div>'.format(item.attr("url"), item.attr("name")));
						else
							header.append('<div class="title">{0}</div>'.format(item.attr("name")));

						var childContainer = $('<div class="children"></div>');
						node.append(header);
						node.append(childContainer);

						createTreeNode(item.find("> *"), childContainer);
						parent.append(node);
					}
				}

				target.empty();

				var items = $(responseText).find("tree > *");
				createTreeNode(items, target);

				target.find(".control").click(onTreeNodeClicked);
				target.find(".node.file > .header .title").click(onTreeFileClicked);
			});
		}

		if (target.is(":visible"))
		{
			target.hide();
			control.removeClass("minus").addClass("plus");
		}
		else
		{
			target.show();
			control.removeClass("plus").addClass("minus");
		}
	}

	function updateLoop()
	{
		var targetUrl = getTargetUrl();
		if (loadedUrl != targetUrl)
		{
			if (loadingUrl != targetUrl)
			{
				setDocumentUrl(targetUrl);
			}

			loadingUrl = targetUrl;
		}

		setTimeout(updateLoop, updateTimeout);
	};

	function getTargetUrl()
	{
		var value = $url.getHashParam("page");
		return value ? unescape(value) : value;
	};

	function setTargetUrl(url)
	{
		$url.setHashParam("page", escape(url));
	};

	function getWindow()
	{
		return contentFrame[0].contentWindow;
	};

	function getDocument()
	{
		return getWindow().document;
	};

	function getDocumentUrl()
	{
		return getDocument().location.href;
	};

	function setDocumentUrl(url)
	{
		$("#contentpreloader").show();
		$("#toolpreloader").show();
		getDocument().location = url;
	};

	function getDocumentThread()
	{
		var id = getDocument().documentElement.getAttribute("id");
		if (id)
			return id.replace(/^t/, String.EMPTY);

		return null;
	};

	function getDocumentSource()
	{
		return getDocument().documentElement.getAttribute("source") || "live";
	};

	function getToolLocation(toolUrl)
	{
		var doc = getDocument();
		var currentUrl = getDocumentUrl();
		var contentThread = getDocumentThread();
		var contentSource = getDocumentSource();
		var page = currentUrl.replace(/^https?:\/\/[^\/]+(?:\/[^\/]+\/[^\/]+\/)?(.*)$/, "$1");
		var pageEscaped = page.replace(/\//g, "%2F");

		var logSource = contentSource == "dev" ? page : String.EMPTY;

		return toolUrl.replace(
			/\{url\}/, currentUrl).replace(
			/\{page\}/, page).replace(
			/\{thread\}/, contentThread).replace(
			/\{source\}/, logSource).replace(
			/\{\?\}/, currentUrl.indexOf("?") != -1 ? "&" : "?");
	};

	function getCommand(element)
	{
		var name = element.getAttribute("x:command");
		var args = element.getAttribute("x:arguments");
		if (!args)
		{
			args = new Array;
		}
		else
		{
			args = args.split(",").each(function (i, e, r) {
				r[i] = String(e).trim()
			});
		}

		var result = { name: name, arguments: args };
		return result;
	};

	function executeCommand(command)
	{
		var commandName = command.name;
		var commandArgs = command.arguments;
		if (Commands[commandName] != null)
			Commands[commandName].apply(this, commandArgs);
		else
			console.log("Unknown command: ", commandName);
	};

	function updateControls()
	{
		commands.each(updateButtonState);
	};

	function updateTitle()
	{
		var frameTitle = null;
		try
		{
			frameTitle = getDocument().title;
			document.title = TITLE_PATTERN.format(frameTitle, TITLE_DEFAULT);
		}
		catch(e)
		{
			document.title = TITLE_DEFAULT;
		}
	};

	function updateButtonState()
	{
		var command = getCommand(this);
		var commandName = command.name;
		var commandArgs = command.arguments;

		$(this).removeClass("active");
		$(this).addClass("disabled");

		if (Commands[commandName] != null)
		{
			var state = Commands[commandName].getState(commandArgs);
			if ((state & BUTTONSTATE.ENABLED) != 0)
			{
				$(this).removeClass("disabled");
			}
			if ((state & BUTTONSTATE.ACTIVE) != 0)
			{
				$(this).addClass("active");
			}
		}
	};

	function resizeFrames(e)
	{
		var e = e || window.event;

		var diffX = e.clientX - dragClientX;
		var diffY = e.clientY - dragClientY;

		var height = contentHeight + diffY;
		var width = contentWidth + diffX;

		if (currentOrientation == ORIENTATION.HORIZONTAL)
			resizeFramesVertically(height);
		else
			resizeFramesHorizontally(width);

		sizeOverlays();
		return false;
	};

	function resizeFramesVertically(height)
	{
		var dev_content = $("#contentbox");
		var frame1 = $("#content_container");
		var frame2 = $("#tools_container");

		var targetHeight = height;

		var availHeight = dev_content.innerHeight();
		var percent = targetHeight / (availHeight * 0.01);

		if (percent >= 20 && percent <= 80)
		{
			var f1h = parseInt(percent);
			var f2h = 100 - f1h;

			frame1.height(f1h + "%");
			frame2.height(f2h + "%");
		}
	};

	function resizeFramesHorizontally(width)
	{
		var dev_content = $("#contentbox");
		var frame1 = $("#content_container");
		var frame2 = $("#tools_container");

		var targetWidth = width;

		var availWidth = dev_content.innerWidth();
		var percent = targetWidth / (availWidth * 0.01);

		if (percent >= 20 && percent <= 80)
		{
			var f1w = parseInt(percent);
			var f2w = 100 - f1w;

			frame1.width(f1w + "%");
			frame2.width(f2w + "%");
		}
	};

	function getRect(elem)
	{
		var rect = { x1: 0, y1: 0, x2: 0, y2: 0 };
		if (Type.isHtmlElement(elem))
		{
			rect.x1 = $(elem).offset().left;
			rect.y1 = $(elem).offset().top;
			rect.x2 = rect.x1 + elem.offsetWidth;
			rect.y2 = rect.y1 + elem.offsetHeight;
		}

		return rect;
	};

	function isInRange(elem, x, y)
	{
		var rect = getRect(elem);
		if (x >= rect.x1 && x <= rect.x2 && y >= rect.y1 && y <= rect.y2)
			return true;

		return false;
	};

	function overlayFrames()
	{
		var iframes = document.getElementsByTagName("iframe");
		if (iframes.length == 0)
			return;

		if (!this.$overlays)
			this.$overlays = [];
		else
			removeOverlays();

		for (var i = 0; i < iframes.length; i++)
		{
			var iframe = iframes[i];
			if (iframe.offsetHeight > 0)
			{
				var overlay = document.createElement("div");
				overlay.iframe = iframe;
				overlay.style.cssText = "position: absolute; background: #fff; opacity: .01; z-index:10000;";

				document.body.appendChild(overlay);
				this.$overlays.push(overlay);
			}
		}

		sizeOverlays();
	};

	function sizeOverlays()
	{
		if (this.$overlays)
		{
			for (var i = 0; i < this.$overlays.length; i++)
			{
				var overlay = this.$overlays[i];
				$(overlay).css({
					top: $(overlay.iframe).offset().top,
					left: $(overlay.iframe).offset().left,
					width: overlay.iframe.offsetWidth,
					height: overlay.iframe.offsetHeight
				});
			}
		}
	};

	function removeOverlays()
	{
		if (this.$overlays == null)
			return;

		while (this.$overlays.length)
		{
			var oly = this.$overlays.shift();
			oly.parentNode.removeChild(oly);
		}
	};

	function onToolFrameLoaded()
	{
		$("#toolpreloader").hide();
	};

	function onContentFrameLoaded()
	{
		$("#contentpreloader").hide();

		if (currentLayout == LAYOUT.DOUBLE)
			Commands.SetTool(currentTool);

		loadedUrl = getDocumentUrl();
		setTargetUrl(loadedUrl);
		updateControls();
		updateTitle();
	};

	function onButtonClicked(e)
	{
		if (this.className.indexOf("disabled") != -1)
			return;

		var command = getCommand(this);
		executeCommand(command);
		updateControls();
	};

	function onViewTolbarMouseDown(e)
	{
		if (isResizeReady)
		{
			var e = e || window.event;

			dragClientX = e.clientX;
			dragClientY = e.clientY;

			contentHeight = $("#content_container").innerHeight();
			contentWidth = $("#content_container").innerWidth();

			isResizeActive = true;
			overlayFrames();
		}
	};

	function onDocumentMouseUp()
	{
		isResizeActive = false;
		removeOverlays();
	};

	function onDocumentMouseMove(e)
	{
		if (isResizeActive)
			return resizeFrames(e);

		var e = e || window.event;
		var x = e.clientX;
		var y = e.clientY;
		var t = $("#viewtoolbar")[0];

		if (isInRange(t, x, y))
		{
			var overlap = false;
			$(".toolbar", t).each(function (i, e) { if (isInRange(e, x, y)) overlap = true; });

			var resizeCursor = currentOrientation == ORIENTATION.VERTICAL ? "e-resize" : "n-resize";
			t.style.cursor = overlap ? "default" : resizeCursor;
			isResizeReady = overlap ? false : true;
		}
	};

	Commands = {};
	Commands.ToggleViews = function ToggleViews()
	{
		if (commandsVisible)
		{
			$("#dev_wrapper").removeClass("dashboard");
			$(".button.views").removeClass("on");
			commandsVisible = false;
			$cookie.set("COMMANDS", "OFF", "/");
		}
		else
		{
			$("#dev_wrapper").addClass("dashboard");
			$(".button.views").addClass("on");
			commandsVisible = true;
			$cookie.set("COMMANDS", "ON", "/");
		}
	};
	Commands.ToggleViews.getState = function ToggleViews$getState()
	{
		var state = BUTTONSTATE.ENABLED;
		if (commandsVisible)
			state |= BUTTONSTATE.ACTIVE;

		return state;
	};

	Commands.SwitchOff = function SwitchOff()
	{
		document.location = getDocumentUrl();
	};
	Commands.SwitchOff.getState = function SwitchOff$getState()
	{
		return BUTTONSTATE.ENABLED;
	};

	Commands.SingleFrame = function SingleFrame()
	{
		$cookie.set("LY", "SINGLE", "/");
		$("#contentbox").removeClass("double").addClass("single");
		currentLayout = LAYOUT.SINGLE;
	};
	Commands.SingleFrame.getState = function SingleFrame$getState()
	{
		var state = BUTTONSTATE.ENABLED;
		if (currentLayout == LAYOUT.SINGLE)
			state |= BUTTONSTATE.ACTIVE;

		return state;
	};

	Commands.DoubleFrame = function DoubleFrame()
	{
		$cookie.set("LY", "DOUBLE", "/");
		$("#contentbox").removeClass("single").addClass("double");
		currentLayout = LAYOUT.DOUBLE;
	};
	Commands.DoubleFrame.getState = function DoubleFrame$getState()
	{
		var state = BUTTONSTATE.ENABLED;
		if (currentLayout == LAYOUT.DOUBLE)
			state |= BUTTONSTATE.ACTIVE;

		return state;
	};

	Commands.VerticalFrames = function VerticalFrames()
	{
		$cookie.set("OR", "VERTICAL", "/");
		$("#contentbox").removeClass("horizontal").addClass("vertical");
		currentOrientation = ORIENTATION.VERTICAL;

		if (currentLayout == LAYOUT.SINGLE)
			Commands.DoubleFrame();
	};
	Commands.VerticalFrames.getState = function VerticalFrames$getState()
	{
		var state = currentLayout == LAYOUT.DOUBLE;
		if (currentOrientation == ORIENTATION.VERTICAL)
			state |= BUTTONSTATE.ACTIVE;

		return state;
	};

	Commands.HorizontalFrames = function HorizontalFrames()
	{
		$cookie.set("OR", "HORIZONTAL", "/");
		$("#contentbox").removeClass("vertical").addClass("horizontal");
		currentOrientation = ORIENTATION.HORIZONTAL;

		if (currentLayout == LAYOUT.SINGLE)
			Commands.DoubleFrame();
	};
	Commands.HorizontalFrames.getState = function HorizontalFrames$getState()
	{
		var state = currentLayout == LAYOUT.DOUBLE;
		if (currentOrientation == ORIENTATION.HORIZONTAL)
			state |= BUTTONSTATE.ACTIVE;

		return state;
	};

	Commands.SetTool = function SetTool(toolName)
	{
		var tool = tools[toolName];
		if (!tool || currentTool == tool)
			return;

		try
		{
			$("#toolpreloader").show();
			$(".button").removeClass("active");
			$(".button." + tool.name).addClass("active");

			toolFrame.attr("src", getToolLocation(tool.url));
			$url.setHashParam("tool", tool.name);
			currentTool = toolName;

			if (currentLayout == LAYOUT.SINGLE)
				Commands.DoubleFrame();
		}
		catch(e) { console.log(e.message); }
	};
	Commands.SetTool.getState = function SetTool$getState(toolName)
	{
		var state = BUTTONSTATE.ENABLED;
		if (currentTool == toolName)
			state |= BUTTONSTATE.ACTIVE;

		return state;
	};

	Commands.SetView = function SetView(url)
	{
		setTargetUrl(url);
		currentView = url;
	};
	Commands.SetView.getState = function SetView$getState(url)
	{
		var state = BUTTONSTATE.ENABLED;
		if (currentView == url)
			state |= BUTTONSTATE.ACTIVE;

		return state;
	};

	Commands.Reload = function Reload()
	{
		$("#contentpreloader").show();
		$("#toolpreloader").show();
		getWindow().location.reload(true);
	};
	Commands.Reload.getState = function Reload$getState(url)
	{
		return BUTTONSTATE.ENABLED | BUTTONSTATE.ACTIVE;
	};

	$(window).ready(setup);

};
