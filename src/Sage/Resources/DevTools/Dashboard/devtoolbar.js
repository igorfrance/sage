Type.registerNamespace("sage");

sage.Developer = new function ()
{
	var button;
	var menu;
	var tools;
	var frame;
	var thread;

	var toolsVisible = false;
	var logVisible = false;

	var devFrameUrl = "dev/#page={0}&tool={1}";

	/**
	 * @type {Object}
	 */
	var toolUrls =
	{
		log:	{ title: "Request log",	url: "dev/log/{thread}" },
		routes: { title: "Route debugger", url: "dev/routes/?url={page}" },
		xml:	{ title: "Request xml",	url: "{url}{?}view=xml" },
		xmlx:   { title: "Request xml",	url: "{url}{?}view=xmlx" },
		save: { title: "Save page xml", url: "dev/save/?url={url}" }
	};

	function setup()
	{
		thread = $("html").attr("id").match(/([0-9]+)/)[1];
		tools = $("#developer-tools");
		button = $("#developer-button");
		menu = $("#developer-menu");
		frame = $("#developer-frame");
		menu.attr("tabIndex", 1);
		menu.find("li").click(executeCommand);

		button.unselectable().click(toggleMenu);

		$(document).click(autoHideMenu);
		$(document).keydown(onKeyDown);

		$("#developer-frame .close").click(hideFrame);

		loadLogHtml();
	};

	function onKeyDown(e)
	{
		var event = e || window.event;

		// SHIFT-D
		if (event.shiftKey && event.keyCode == 68)
		{
			if (toolsVisible)
				$("#developer-tools").hide();
			else
				$("#developer-tools").show();

			toolsVisible = !toolsVisible;
		}

		autoHideMenu();
	};

	function toggleMenu()
	{
		if (menu.attr("offsetHeight") == 0)
			showMenu();
		else
			hideMenu();
	};

	function showMenu()
	{
		button.addClass("open");
		menu.show();
	};

	function hideMenu()
	{
		menu.hide();
		button.removeClass("open");
	};

	function showFrame()
	{
		frame.show();

		var wrapperHeight = $("#wrapper").height();
		var frameHeight = frame.height();
		$(document.body).height(wrapperHeight + frameHeight + 100);
	};

	function hideFrame()
	{
		var wrapperHeight = $("#wrapper").height();
		var frameHeight = frame.height();
		$(document.body).height(wrapperHeight - frameHeight);

		frame.hide();
	};

	function autoHideMenu(e)
	{
		if (menu.is(":visible") && !isMenuRelated(e.target))
			hideMenu();
	};

	function isMenuRelated(element)
	{
		if (element == button[0] || element == menu[0])
			return true;

		var parent = element.parentNode;
		while (parent)
		{
			if (parent == button[0] || parent == menu[0])
				return true;

			parent = parent.parentNode;
		}

		return false;
	};

	function executeCommand()
	{
		if (this.className.match(/command:(\w+)/))
		{
			var commandName = RegExp.$1;
			if (commands[commandName] != null)
			{
				commands[commandName].apply(this);
			}
		}

		hideMenu();
	};

	function loadLogHtml()
	{
		var logMessages = [];

		var url = getToolLocation("log", true);
		$.get(url, function (responseText)
		{
			var i;
			if (responseText.match(/<body[^>]*>([\s\S]*)<\/body>/i))
			{
				var rows = $(RegExp.$1).find("table#log_table tr");
				for (i = 0; i < rows.length; i++)
				{
					logMessages.push({
						severity: rows.eq(i).find("td.severity").text().toLowerCase(),
						elapsed: rows.eq(i).find("td.elapsed").text().trim(),
						duration: rows.eq(i).find("td.duration").text().trim(),
						logger: rows.eq(i).find("td.logger").text().trim(),
						message: rows.eq(i).find("td.message").text().trim()
					});
				}
			}

			console.groupCollapsed("SERVER SIDE LOG FOR CURRENT REQUEST");

			var LOG_MESSAGE = "{2#6} {0}: {1}";
			for (i = 0; i < logMessages.length; i++)
			{
				var severity = logMessages[i].severity.trim();
				if (severity == "error")
					severity = "warn";
				if (severity == "debug")
					severity = "info";

				if (Type.isFunction(console[severity]))
					console[severity](LOG_MESSAGE.format(
						logMessages[i].logger,
						logMessages[i].message,
						logMessages[i].elapsed,
						logMessages[i].duration));
			}

			console.groupEnd();
		});
	}

	function loadInline(toolName)
	{
		var url = getToolLocation(toolName, true);
		var iframe = frame.find("iframe");
		if (iframe.attr("src") != url)
		{
			iframe.attr("src", url);
		}

		frame.find("h6").text(toolUrls[toolName].title);
		showFrame();
	};

	function loadInFrame(toolName)
	{
		top.window.location = getToolLocation(toolName, false);
	};

	function getToolUrl(toolUrl)
	{
		var currentUrl = document.location.href;
		var page = currentUrl.replace(
			/^https?:\/\/[^\/]+(?:\/[^\/]+\/[^\/]+\/)?(.*)$/, "$1").replace(
			/\//g, "%2F");

		return toolUrl.replace(
			/\{url\}/, currentUrl).replace(
			/\{page\}/, page).replace(
			/\{thread\}/, thread).replace(
			/\{\?\}/, currentUrl.indexOf("?") != -1 ? "&" : "?");
	};

	function getToolLocation(toolName, inline)
	{
		if (inline)
		{
			switch (toolName)
			{
				case "xml":
				case "xmlx":
				case "log":
				case "routes":
					return getToolUrl(toolUrls[toolName].url);

				default:
					return "about:blank";
			}
		}

		return devFrameUrl.format(document.location.href, toolName);
	};

	var commands = {};
	commands.toggleLog = function toggleLog()
	{
		if (logVisible)
		{
			hideFrame();
			logVisible = false;
		}
		else
		{
			loadInline("log");
			logVisible = true;
		}
	};

	commands.openXml = function openXml()
	{
		var url = getToolLocation("xml", true);
		var w = window.open(url, "x", "toolbar=0,status=1,scrollbars=1");
		if (w)
			w.focus();
	};

	commands.openXmlx = function openXmlx()
	{
		var url = getToolLocation("xmlx", true);
		var w = window.open(url, "x", "toolbar=0,status=1,scrollbars=1");
		if (w)
			w.focus();
	};

	commands.openDevTools = function openDevTools()
	{
		loadInFrame("log");
	};

	commands.saveXml = function saveXml()
	{
		loadInFrame("save");
	};

	$(document).ready(setup);

};
