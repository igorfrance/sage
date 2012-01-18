Type.registerNamespace("sage.dev");

sage.dev.ViewInspector = function ViewInspector()
{
	this.$super("close", "reload");

	var self = this;

	var layoutType =
		{ SINGLE: 1, DOUBLE: 2 };

	var orientationType =
		{ HORIZONTAL: 1, VERTICAL: 2 };

	var buttonState =
		{ DISABLED: 0, ENABLED: 1, ACTIVE: 2 };

	var contentFrame;
	var contentPreloader;
	var contentcontainer;
	var contentTitle;
	var toolscontainer;
	var toolFrame;
	var toolPreloader;
	var viewToolbar;
	var viewInspector;

	var currentCommand;
	var currentArguments;
	var currentLayout;
	var currentOrientation;
	var currentPage;

	var defaultCommand = "ShowLog";
	var defaultArguments = null;

	var metaViewUrl = "{url}{?}view={arguments}";
	var logViewUrl = "dev/log/{thread}";

	var defaultTitle = document.title;
	var titlePattern = "{0} - {1}";

	var isResizeReady = false;
	var isResizeActive = false;

	var dragClientX = 0;
	var dragClientY = 0;

	var contentHeight = 0;
	var contentWidth = 0;

	var updateTimeout = 500;
	var buttons = null;
	var loadingUrl = null;

	this.init = function ViewInspector$init()
	{
		viewInspector =  $(".view-inspector");
		viewToolbar = viewInspector.find(".viewtoolbar");
		toolFrame = viewInspector.find(".toolframe iframe");
		contentFrame = viewInspector.find(".contentframe iframe");
		contentPreloader = viewInspector.find(".contentpreloader");
		toolPreloader = viewInspector.find(".toolpreloader");
		contentTitle = viewInspector.find(".contenttitle");
		contentcontainer = viewInspector.find(".contentcontainer");
		toolscontainer = viewInspector.find(".toolscontainer");

		currentLayout = viewInspector.hasClass("double")
			? layoutType.DOUBLE
			: layoutType.SINGLE;

		currentOrientation = viewInspector.hasClass("horizontal")
			? orientationType.HORIZONTAL
			: orientationType.VERTICAL;

		var targetUrl = getTargetUrl() || viewInspector.attr("data-view");
		if (targetUrl)
			setTargetUrl(targetUrl);

		buttons = viewInspector.find(".button, .textbutton");
		buttons.click(onButtonClicked);

		viewToolbar.bind("mousedown", onViewTolbarMouseDown);
		contentFrame.bind("load", onContentFrameLoaded);
		toolFrame.bind("load", onToolFrameLoaded);

		$(document)
			.bind("mouseup", onDocumentMouseUp)
			.bind("mousemove", onDocumentMouseMove);
		$(window)
			.bind("hashchange", onDocumentHashChange);

		updateElement();
	}

	this.inspect = function ViewInspector$inspect(inspectUrl, showTool)
	{
		if (showTool == "xml")
			setTargetCommand("ShowMeta", "xml");

		if (showTool == "xmlx")
			setTargetCommand("ShowMeta", "xmlx");

		if (showTool == "json")
			setTargetCommand("ShowMeta", "json");

		if (showTool == "htmlx")
			setTargetCommand("ShowMeta", "htmlx");

		if (showTool == "log")
			setTargetCommand("ShowLog");

		setTargetUrl(inspectUrl);
	};

	function getTargetUrl()
	{
		var value = $url.getHashParam("page");
		return value ? unescape(value) : value;
	}

	function getTargetCommand()
	{
		var value = $url.getHashParam("command");
		return value ? unescape(value) : value;
	}

	function getTargetArguments()
	{
		var value = $url.getHashParam("arguments");
		return value ? unescape(value) : value;
	}

	function getToolWindow()
	{
		return toolFrame[0].contentWindow;
	}

	function getToolDocument()
	{
		return getToolWindow().document;
	}

	function getContentWindow()
	{
		return contentFrame[0].contentWindow;
	}

	function getContentDocument()
	{
		try
		{
			var result = getContentWindow().document;
			return result;
		}
		catch(e)
		{
			return null;
		}
	}

	function getContentDocumentUrl()
	{
		return getContentDocument().location.href;
	}

	function getContentDocumentThread()
	{
		var thread = getContentDocument().documentElement.getAttribute("data-thread");
		return thread;
	}

	function getContentDocumentSource()
	{
		return getContentDocument().documentElement.getAttribute("source") || "live";
	}

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
	}

	function setTargetUrl(url)
	{
		$url.setHashParam("page", escape(url));
	}

	function setTargetCommand(command, args)
	{
		$url.setHashParam("command", command);
		if (args)
			$url.setHashParam("arguments", args);
		else
			$url.removeHashParam("arguments");
	}

	function setDocumentUrl(url)
	{
		contentPreloader.show();
		toolPreloader.show();
		contentFrame.attr("src", url);
	}

	function setToolLocation(templateUrl, command, commandArgs)
	{
		var doc = getContentDocument();
		var currentUrl = getContentDocumentUrl();
		var contentThread = getContentDocumentThread();
		var contentSource = getContentDocumentSource();
		var page = currentUrl.replace(/^https?:\/\/[^\/]+(?:\/[^\/]+\/[^\/]+\/)?(.*)$/, "$1");
		var pageEscaped = page.replace(/\//g, "%2F");

		var logSource = contentSource == "dev" ? page : String.EMPTY;
		var toolUrl = templateUrl.replace(
			/\{url\}/, currentUrl).replace(
			/\{page\}/, page).replace(
			/\{thread\}/, contentThread).replace(
			/\{source\}/, logSource).replace(
			/\{arguments\}/, commandArgs).replace(
			/\{\?\}/, currentUrl.indexOf("?") != -1 ? "&" : "?");

		toolPreloader.show();
		toolFrame.attr("src", toolUrl);

		currentCommand = command;
		currentArguments = commandArgs;

		$url.setHashParam("command", command);

		if (!commandArgs)
			$url.removeHashParam("arguments");
		else
			$url.setHashParam("arguments", commandArgs);
	}

	function getCommand(element)
	{
		var name = element.getAttribute("data-command");
		var args = element.getAttribute("data-arguments");
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
	}

	function executeCommand(targetCommand, targetArguments)
	{
		if (commands[targetCommand] != null)
		{
			commands[targetCommand](targetArguments);
			updateControls();
		}
		else
			console.log("Unknown command: ", targetCommand);
	}

	function updateElement()
	{
		var targetUrl = getTargetUrl();
		var cmd = getTargetCommand();
		var args = getTargetArguments();

		if (currentPage != targetUrl)
		{
			if (loadingUrl != targetUrl)
				setDocumentUrl(targetUrl);

			loadingUrl = targetUrl;
		}
		else if (currentCommand != cmd || currentArguments != args)
		{
			executeCommand(cmd, args);
		}

		updateControls();
	}

	function updateControls()
	{
		buttons.each(updateButtonState);
	}

	function updateTitle()
	{
		var frameTitle = null;
		try
		{
			frameTitle = getContentDocument().title;
			if (frameTitle)
			{
				document.title = titlePattern.format(frameTitle, defaultTitle);
				contentTitle.text(frameTitle);
			}
		}
		catch(e)
		{
			document.title = defaultTitle;
		}
	}

	function updateButtonState(index, element)
	{
		var button = $(element);
		var commandName = button.attr("data-command");
		var commandArgs = button.attr("data-arguments");

		button.removeClass("active").addClass("disabled");

		try
		{
			if (commands[commandName] != null)
			{
				var state = commands[commandName].getState(commandArgs);
				if ((state & buttonState.ENABLED) != 0)
				{
					button.removeClass("disabled");
				}

				if ((state & buttonState.ACTIVE) != 0)
				{
					button.addClass("active");
				}
			}
		}
		catch(e) { $log.error(e); }
	}

	function resizeFrames(e)
	{
		e = e || window.event;

		var diffX = e.clientX - dragClientX;
		var diffY = e.clientY - dragClientY;

		var height = contentHeight + diffY;
		var width = contentWidth + diffX;

		if (currentOrientation == orientationType.HORIZONTAL)
			resizeFramesVertically(height);
		else
			resizeFramesHorizontally(width);

		sizeOverlays();
		return false;
	}

	function resizeFramesVertically(height)
	{
		var targetHeight = height;

		var availHeight = viewInspector.innerHeight();
		var percent = targetHeight / (availHeight * 0.01);

		if (percent >= 20 && percent <= 80)
		{
			var f1h = parseInt(percent);
			var f2h = 100 - f1h;

			contentcontainer.height(f1h + "%");
			toolscontainer.height(f2h + "%");

			$cookie.set("dev.viewinspector.fh", f1h);
		}
	}

	function resizeFramesHorizontally(width)
	{
		var targetWidth = width;

		var availWidth = viewInspector.innerWidth();
		var percent = targetWidth / (availWidth * 0.01);

		if (percent >= 20 && percent <= 80)
		{
			var f1w = parseInt(percent);
			var f2w = 100 - f1w;

			contentcontainer.width(f1w + "%");
			toolscontainer.width(f2w + "%");

			$cookie.set("dev.viewinspector.fw", f1w);
		}
	}

	function isInRange(elem, x, y)
	{
		var rect = getRect(elem);
		if (x >= rect.x1 && x <= rect.x2 && y >= rect.y1 && y <= rect.y2)
			return true;

		return false;
	}

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
				overlay.style.cssText = "position: absolute; background: #fff; opacity: .01; filter: alpha(opacity=1); z-index:10000;";

				document.body.appendChild(overlay);
				this.$overlays.push(overlay);
			}
		}

		sizeOverlays();
	}

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
	}

	function removeOverlays()
	{
		if (this.$overlays == null)
			return;

		while (this.$overlays.length)
		{
			var oly = this.$overlays.shift();
			oly.parentNode.removeChild(oly);
		}
	}

	function onToolFrameLoaded()
	{
		toolPreloader.hide();
	}

	function onContentFrameLoaded()
	{
		contentPreloader.hide();

		currentPage = getContentDocumentUrl();
		if (currentPage != "about:blank")
		{
			setTargetUrl(currentPage);

			var targetCommand = getTargetCommand();
			var targetArguments = getTargetArguments();

			if (targetCommand && targetCommand != currentCommand)
				executeCommand(targetCommand, targetArguments);

			else if (currentCommand && currentLayout == layoutType.DOUBLE)
				commands[currentCommand](currentArguments);
		}

		updateControls();
		updateTitle();
	}

	function onButtonClicked(e)
	{
		var button = $(this);
		if (button.hasClass("disabled"))
			return;

		var commandName = button.attr("data-command");
		var commandArguments = button.attr("data-arguments");

		if (commandName)
		{
			executeCommand(commandName, commandArguments);
			updateControls();
		}
	}

	function onViewTolbarMouseDown(e)
	{
		if (isResizeReady)
		{
			var e = e || window.event;

			dragClientX = e.clientX;
			dragClientY = e.clientY;

			contentHeight = contentcontainer.innerHeight();
			contentWidth = contentcontainer.innerWidth();

			isResizeActive = true;
			overlayFrames();
		}

		return false;
	}

	function onDocumentMouseUp()
	{
		isResizeActive = false;
		removeOverlays();
	}

	function onDocumentHashChange()
	{
		updateElement();
	}

	function onDocumentMouseMove(e)
	{
		if (currentLayout == layoutType.SINGLE)
			return false;

		if (isResizeActive)
			return resizeFrames(e);

		e = e || window.event;

		var x = e.clientX;
		var y = e.clientY;
		var t = viewToolbar[0];

		if (isInRange(t, x, y))
		{
			var overlap = false;
			$(".toolbar", t).each(function (i, e) { if (isInRange(e, x, y)) overlap = true; });

			var resizeCursor = currentOrientation == orientationType.VERTICAL ? "e-resize" : "n-resize";
			t.style.cursor = overlap ? "default" : resizeCursor;
			isResizeReady = overlap ? false : true;
		}
	}

	var commands = {};

	commands.ToggleFrame = function ToggleFrame()
	{
		if (currentLayout == layoutType.SINGLE)
		{
			executeCommand("DoubleFrame");
		}
		else
		{
			executeCommand("SingleFrame");
		}
	};

	commands.ToggleFrame.getState = function ToggleFrame$getState()
	{
		var state = buttonState.ENABLED;
		if (currentLayout == layoutType.DOUBLE)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.SingleFrame = function SingleFrame()
	{
		$cookie.set("dev.viewinspector.layout", "single", "/");
		$url.removeHashParam("command");
		$url.removeHashParam("arguments");

		viewInspector.removeClass("double").addClass("single");
		currentLayout = layoutType.SINGLE;
	};

	commands.SingleFrame.getState = function SingleFrame$getState()
	{
		var state = buttonState.ENABLED;
		if (currentLayout == layoutType.SINGLE)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.DoubleFrame = function DoubleFrame()
	{
		$cookie.set("dev.viewinspector.layout", "double", "/");
		viewInspector.removeClass("single").addClass("double");
		currentLayout = layoutType.DOUBLE;
	};

	commands.DoubleFrame.getState = function DoubleFrame$getState()
	{
		var state = buttonState.ENABLED;
		if (currentLayout == layoutType.DOUBLE)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.VerticalFrames = function VerticalFrames()
	{
		$cookie.set("dev.viewinspector.orientation", "vertical", "/");
		viewInspector.removeClass("horizontal").addClass("vertical");
		currentOrientation = orientationType.VERTICAL;

		if (currentLayout == layoutType.SINGLE)
			commands.DoubleFrame();
	};
	commands.VerticalFrames.getState = function VerticalFrames$getState()
	{
		var state = currentLayout == layoutType.DOUBLE;
		if (currentOrientation == orientationType.VERTICAL)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.HorizontalFrames = function HorizontalFrames()
	{
		$cookie.set("dev.viewinspector.orientation", "horizontal", "/");
		viewInspector.removeClass("vertical").addClass("horizontal");
		currentOrientation = orientationType.HORIZONTAL;

		if (currentLayout == layoutType.SINGLE)
			commands.DoubleFrame();
	};
	commands.HorizontalFrames.getState = function HorizontalFrames$getState()
	{
		var state = currentLayout == layoutType.DOUBLE;
		if (currentOrientation == orientationType.HORIZONTAL)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.ShowMeta = function ShowMeta(viewName)
	{
		if (currentLayout == layoutType.SINGLE)
			executeCommand("DoubleFrame");

		return setToolLocation(metaViewUrl, "ShowMeta", viewName);
	};

	commands.ShowMeta.getState = function ShowMeta$getState(viewName)
	{
		var state = buttonState.ENABLED;
		if (currentLayout == layoutType.DOUBLE && currentCommand == "ShowMeta" && currentArguments == viewName)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.ShowLog = function ShowLog()
	{
		if (currentLayout == layoutType.SINGLE)
			executeCommand("DoubleFrame");

		return setToolLocation(logViewUrl, "ShowLog");
	};

	commands.ShowLog.getState = function ShowLog$getState()
	{
		var state = buttonState.ENABLED;
		if (currentCommand == "ShowLog")
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.Close = function Close()
	{
		toolFrame.attr("src", "about:blank");
		contentFrame.attr("src", "about:blank");

		$url.removeHashParam("page");
		$url.removeHashParam("command")
		$url.removeHashParam("arguments");

		self.fireEvent("close");
	};

	commands.Close.getState = function Close$getState()
	{
		return buttonState.ENABLED;
	};


	commands.Reload = function Reload()
	{
		contentPreloader.show();
		toolPreloader.show();
		getContentWindow().location.reload(true);
		self.fireEvent("reload");
	};

	commands.Reload.getState = function Reload$getState(url)
	{
		return buttonState.ENABLED;
	};
};

sage.dev.ViewInspector.inherits(aeon.Dispatcher);

sage.dev.ViewInspector.setup = function ViewInspector$setup()
{
	if (sage.dev.ViewInspector.current == null)
	{
		sage.dev.ViewInspector.current = new sage.dev.ViewInspector();
		sage.dev.ViewInspector.current.init();
	}

	return sage.dev.ViewInspector.current;
};

$init.registerModule(sage.dev.ViewInspector);
