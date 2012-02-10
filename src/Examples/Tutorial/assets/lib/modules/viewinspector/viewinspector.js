Type.registerNamespace("sage.dev");

sage.dev.ViewInspector = function ViewInspector()
{
	this.$super("close", "info", "contentloaded", "reload");

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

	var currentLayout;
	var currentOrientation;

	var defaultInspector = "log";
	var defaultArguments = null;

	var defaultTitle = document.title;
	var titlePattern = "{0} - {1}";

	var isResizeReady = false;
	var isResizeActive = false;

	var dragClientX = 0;
	var dragClientY = 0;

	var contentHeight = 0;
	var contentWidth = 0;

	var tick = 1000;
	var buttons = null;

	var metaViews = ["xml", "xmlx", "htmlx", "json"];
	var currentState = { url: "about:blank", inspector: null };
	var stateParamName = "sage:vi";
	var logUrl = "{0}dev/log/{1}";
	var baseHref;

	this.init = function ViewInspector$init()
	{
		viewInspector = $(".view-inspector");
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

		buttons = viewInspector.find(".button, .textbutton");
		buttons.click(onButtonClicked);

		viewToolbar.bind("mousedown", onViewToolbarMouseDown);
		contentFrame.bind("load", onContentFrameLoaded);
		toolFrame.bind("load", onToolFrameLoaded);

		baseHref = viewInspector.attr("data-basehref");

		$(document)
			.bind("mouseup", onDocumentMouseUp)
			.bind("mousemove", onDocumentMouseMove);

		var initialUrl = viewInspector.attr("data-view");
		if (initialUrl)
			setViewInspectorState({ url: escape(initialUrl) });

		setTimeout(updateLoop, tick);
	};

	this.inspect = function ViewInspector$inspect(url, inspector)
	{
		var state = { url: escape(url), inspector: inspector };
		setViewInspectorState(state);
	};

	this.getContentUrl = function ViewInspector$getContentUrl()
	{
		return currentState.url;
	};

	function getHashInspectorState()
	{
		return $url.serializeParams(currentState);
	}

	function setHashInspectorState()
	{
		var state = getHashInspectorState();
		var escapedState = escape(state);
		$url.setHashParam(stateParamName, escapedState);
	}

	function clearViewInspectorState()
	{
		$url.removeHashParam(stateParamName);
	}

	function setViewInspectorState(state)
	{
		var changed = false;

		if (state.url && currentState.url != state.url)
		{
			setContentLocation(unescape(state.url));
			currentState.url = state.url;

			changed = true;

			if (currentLayout == layoutType.DOUBLE && state.inspector == null)
				state.inspector = currentState.inspector || defaultInspector;
		}

		if (currentLayout == layoutType.DOUBLE)
		{
			if (currentState.inspector != state.inspector)
			{
				currentState.inspector = state.inspector;
				changed = true;
			}
		}

		if (changed)
		{
			setHashInspectorState(state);
		}
	}

	function setContentLocation(url)
	{
		contentPreloader.show();
		contentFrame.attr("src", url);
	}

	function loadMetaView(name)
	{
		var toolUrl = new aeon.utils.Url(getContentDocumentUrl());
		toolUrl.setQueryParam("view", name);

		toolPreloader.show();
		toolFrame.attr("src", toolUrl);
	}

	function loadLogView()
	{
		var document = getContentDocument();
		if (!document)
			return;

		var thread = document.documentElement.getAttribute("data-thread");

		toolPreloader.show();
		toolFrame.attr("src", logUrl.format(baseHref, thread));
	}

	/////////////////////////////////////////////////////// State

	function onViewToolbarMouseDown(e)
	{
		if (isResizeReady)
		{
			e = e || window.event;

			dragClientX = e.clientX;
			dragClientY = e.clientY;

			contentHeight = contentcontainer.innerHeight();
			contentWidth = contentcontainer.innerWidth();

			isResizeActive = true;
			overlayFrames();
		}

		return false;
	}

	function updateLoop()
	{
		var hashParam = $url.getHashParam(stateParamName);
		if (hashParam)
		{
			var hashState = unescape(hashParam);
			if (hashState != getHashInspectorState())
			{
				var state = $url.parseQuery(hashState);
				setViewInspectorState(state);
			}
		}

		setTimeout(updateLoop, tick);
	}

	function updateTitle()
	{
		var frameTitle;
		try
		{
			frameTitle = getContentDocument().title;
			if (frameTitle)
			{
				document.title = titlePattern.format(frameTitle, defaultTitle);
				contentTitle.text(frameTitle);
			}
		}
		catch (e)
		{
			document.title = defaultTitle;
		}
	}

	function updateControls()
	{
		buttons.each(updateButtonState);
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
				var state = buttonState.ENABLED;
				if (Type.isFunction(commands[commandName].getState))
					state = commands[commandName].getState(commandArgs);

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
		catch (e) { $log.error(e); }
	}

	function onButtonClicked()
	{
		var button = $(this);
		if (button.hasClass("disabled"))
			return;

		var commandName = button.attr("data-command");
		var commandArguments = button.attr("data-arguments");

		if (commandName)
		{
			executeCommand(commandName, commandArguments);
		}
	}


	/////////////////////////////////////////////////////// Frame synchronization

	function getContentDocument()
	{
		var w = contentFrame[0].contentWindow;
		var d = null;
		try
		{
			d = w.document;
		}
		catch (e) { }

		return d;
	}

	function getContentDocumentUrl()
	{
		var doc = getContentDocument();
		if (doc == null)
			return null;

		return doc.location.href;
	}

	function getContentDocumentThread()
	{
		var thread = getContentDocument().documentElement.getAttribute("data-thread");
		return thread;
	}

	function onToolFrameLoaded()
	{
		toolPreloader.hide();
		updateControls();
	}

	function onContentFrameLoaded()
	{
		contentPreloader.hide();

		var contentUrl = getContentDocumentUrl();
		if (contentUrl == null || contentUrl == "about:blank")
			return;

		setViewInspectorState({ url: contentUrl, inspector: currentState.inspector });

		if (currentState.inspector == "log")
			loadLogView();

		else if (metaViews.indexOf(currentState.inspector) != -1)
			loadMetaView(currentState.inspector);

		updateControls();
		updateTitle();

		self.fireEvent("contentloaded", { url: contentUrl });
	}

	/////////////////////////////////////////////////////// Frame resizing

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

	function onDocumentMouseUp()
	{
		isResizeActive = false;
		removeOverlays();
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
			$(".toolbar", t).each(function (i, element) { if (isInRange(element, x, y)) overlap = true; });

			var resizeCursor = currentOrientation == orientationType.VERTICAL ? "e-resize" : "n-resize";
			t.style.cursor = overlap ? "default" : resizeCursor;
			isResizeReady = overlap ? false : true;
		}

		return false;
	}

	/////////////////////////////////////////////////////// Commands

	function executeCommand(commandName, commandArgs)
	{
		var command = commands[commandName];
		if (command)
		{
			command(commandArgs);
			updateControls();
		}
		else
			$log.warn("Unknown command: {0}", commandName);
	}

	var commands = {};
	commands.toggleFrame = function toggleFrame()
	{
		if (currentLayout == layoutType.SINGLE)
		{
			executeCommand("doubleFrame");
		}
		else
		{
			executeCommand("singleFrame");
		}
	};

	commands.toggleFrame.getState = function toggleFrame$getState()
	{
		var state = buttonState.ENABLED;
		if (currentLayout == layoutType.DOUBLE)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.singleFrame = function singleFrame()
	{
		$cookie.set("dev.viewinspector.layout", "single", "/");

		currentState.inspector = null;

		viewInspector.removeClass("double").addClass("single");
		currentLayout = layoutType.SINGLE;
	};

	commands.singleFrame.getState = function singleFrame$getState()
	{
		var state = buttonState.ENABLED;
		if (currentLayout == layoutType.SINGLE)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.doubleFrame = function doubleFrame()
	{
		$cookie.set("dev.viewinspector.layout", "double", "/");
		viewInspector.removeClass("single").addClass("double");
		currentLayout = layoutType.DOUBLE;
	};

	commands.doubleFrame.getState = function doubleFrame$getState()
	{
		var state = buttonState.ENABLED;
		if (currentLayout == layoutType.DOUBLE)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.verticalFrames = function verticalFrames()
	{
		$cookie.set("dev.viewinspector.orientation", "vertical", "/");
		viewInspector.removeClass("horizontal").addClass("vertical");
		currentOrientation = orientationType.VERTICAL;

		if (currentLayout == layoutType.SINGLE)
			commands.DoubleFrame();
	};
	commands.verticalFrames.getState = function verticalFrames$getState()
	{
		var state = currentLayout == layoutType.DOUBLE;
		if (currentOrientation == orientationType.VERTICAL)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.horizontalFrames = function horizontalFrames()
	{
		$cookie.set("dev.viewinspector.orientation", "horizontal", "/");
		viewInspector.removeClass("vertical").addClass("horizontal");
		currentOrientation = orientationType.HORIZONTAL;

		if (currentLayout == layoutType.SINGLE)
			commands.DoubleFrame();
	};
	commands.horizontalFrames.getState = function horizontalFrames$getState()
	{
		var state = currentLayout == layoutType.DOUBLE;
		if (currentOrientation == orientationType.HORIZONTAL)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.viewMeta = function viewMeta(viewName)
	{
		if (currentLayout == layoutType.SINGLE)
			executeCommand("doubleFrame");

		currentState.inspector = viewName;
		loadMetaView(viewName);
	};

	commands.viewMeta.getState = function viewMeta$getState(viewName)
	{
		var state = buttonState.ENABLED;
		if (currentLayout == layoutType.DOUBLE && currentState.inspector == viewName)
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.viewLog = function viewLog()
	{
		if (currentLayout == layoutType.SINGLE)
			executeCommand("doubleFrame");

		currentState.inspector = "log";
		loadLogView();
	};

	commands.viewLog.getState = function viewLog$getState()
	{
		var state = buttonState.ENABLED;
		if (currentState.inspector == "log")
			state |= buttonState.ACTIVE;

		return state;
	};

	commands.close = function close()
	{
		var event = new aeon.Event(this, "close");
		self.fireEvent(event);

		if (!event.cancel)
		{
			toolFrame.attr("src", "about:blank");
			contentFrame.attr("src", "about:blank");

			clearViewInspectorState();
		}
	};

	commands.info = function info()
	{
		self.fireEvent("info");
	};

	commands.reload = function reload()
	{
		contentPreloader.show();
		toolPreloader.show();

		var document = getContentDocument();
		if (document)
		{
			document.location.reload(true);
			self.fireEvent("reload");
		}
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
