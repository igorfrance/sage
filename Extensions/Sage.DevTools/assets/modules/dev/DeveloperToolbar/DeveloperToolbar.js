window.sage = window.sage || {};
window.sage.dev = window.sage.dev || {};

sage.dev.Toolbar = (function Toolbar()
{
	var jQuery = window.jQuery;
	var $toolbar, $icon, $text, $time, $commands, $frame, $iframe;
	var hideTimeout = 100, hideTimeoutId = null;
	var parameters = { basehref: "", thread: "0", url: encodeURIComponent(document.location) };
	var status = { log: [], errors: 0, warnings: 0, clientTime: 0, serverTime: 0 };

	var tooltip;
	var logFrameLoaded = false;
	var logUrl = "{basehref}dev/log/{thread}/?view={view}";
	var inspectUrl = "{basehref}dev/inspect/?devtools=0#sage:vi=url%3D{url}%26inspector%3Dlog";

	var namespaces = { mod: "http://www.cycle99.com/schemas/sage/modules.xsd" };

	function setup()
	{
		var one = window.one || {};
		if (top.window != window || String(location.href).match(new RegExp("[?&]devtools=0")))
			return;

		$toolbar = jQuery("#developer-toolbar");
		$icon = $toolbar.find(".icon");
		$text = $toolbar.find(".status .text");
		$time = $toolbar.find(".status .time");
		$commands = $toolbar.find(".commands");
		$frame = jQuery(document.body)
			.append("<div id='developer-frame'><iframe frameborder='no'></iframe><div class='close' title='Close'></div></div>")
			.find("#developer-frame");

		tooltip = one.controls ? one.controls.get($icon[0]) : null;

		$iframe = $frame.find("iframe");

		parameters.thread = $toolbar.attr("data-thread");
		parameters.basehref = $toolbar.attr("data-basehref");

		status.clientTime = (Number(new Date()) - window.__time) || 0;

		$frame.find(".close").click(onCloseLogViewClick);
		$toolbar.find(".meta .button").click(onMetaViewCommandClick);
		$toolbar.find(".button.log").click(onLogCommandClick);
		$toolbar.find(".button.inspect").click(onInspectCommandClick);
		$toolbar
			.on("mouseenter", onToolbarMouseOver)
			.on("mouseleave", onToolbarMouseOut);

		if (getCookie("devtools") == "on")
			$toolbar.show();

		jQuery(document).bind("keydown", onDocumentKeyDown);

		loadLogData();

		console.log("Developer console is ready, press CTRL-ALT-D to toggle it.");
	}

	function setStatusClass(status)
	{
		$icon.removeClass("ok error warn loading").addClass(status);
	}

	function getStatusClass()
	{
		if (status.errors)
			return "error";

		if (status.warnings)
			return "warning";

		if (status.log.length)
			return "ok";

		return "";
	}

	function updateToolbar()
	{
		var statusClass = "", statusText = "", errorText;

		if (status.errors)
		{
			statusClass = "error";
			statusText = status.errors + " " + status.errors == 1 ? "error" : "errors";
			errorText = getLogMessages("ERROR");;
		}
		else if (status.warnings)
		{
			statusClass = "warning";
			statusText = status.warnings + " " + status.warnings == 1 ? "warning" : "warnings";
			errorText = getLogMessages("WARN");;
		}
		else if (status.log.length)
		{
			statusClass = "ok";
			statusText = "OK.";
			errorText = "Ok";
		}

		if (statusText)
		{
			$icon.attr("title", errorText);
			$icon.data("tooltip", errorText);
			$text.find("label").text(statusText);

			if ((status.errors + status.warnings) != 0)
			{
				$toolbar.show();

				var targetWidth = getToolbarMinWidth();
				$toolbar.animate({ width: targetWidth }, 50, function onExpandComplete()
				{
					if (tooltip)
						tooltip.show();
				});
			}
		}

		if (status.serverTime || status.clientTime)
		{
			$time
				.find("label")
				.text(status.serverTime + "ms / " + status.clientTime + "ms");
			$time
				.attr("title",
					"Server-side request: " + status.serverTime + "ms, " +
					"Client-side initialization: " + status.clientTime + "ms");
		}

		setStatusClass(statusClass);
		$toolbar.find(".content");
	}

	function getLogMessages(severity)
	{
		var text = [];
		for (var i = 0; i < status.log.length; i++)
		{
			var $ = status.log[i];
			if ($.severity == severity)
				text.push("<div class='" + severity.toLowerCase() + "'>" + $.message + "</div>");
		}

		return text.join("");
	}

	function expandToolbar()
	{
		if (tooltip)
		{
			tooltip.hide();
		}

		var currentWidth = $toolbar.width();
		if ($toolbar.data("oldwidth") == null)
			$toolbar.data("oldwidth", currentWidth);

		$toolbar.stop().css("width", "auto");

		var targetWidth = $toolbar.width() + 2;
		$toolbar.css("width", currentWidth).animate({ width: targetWidth }, 150);
	}

	function shrinkToolbar()
	{
		$toolbar.stop().animate({ width: $toolbar.data("oldwidth") }, 150);
	}

	function getTotalToolbarWidth()
	{
		var content = $toolbar.find(".content");
		return (
			(parseInt(content.css("paddingLeft")) || 0) +
			(parseInt(content.css("paddingRight")) || 0) +
			$toolbar.find(".status").outerWidth() +
			$toolbar.find(".commands").outerWidth());
	}

	function getTotalCommandsWidth()
	{
		var totalWidth = 0;
		var groups = $commands.find(".group");
		for (var i = 0; i < groups.length; i++)
		{
			var g = groups.eq(i);
			totalWidth += g.prop("scrollWidth");
			totalWidth += parseInt(g.css("marginLeft")) || 0;
			totalWidth += parseInt(g.css("marginRight")) || 0;
		}

		return totalWidth;
	}

	function getToolbarMinWidth()
	{
		var content = $toolbar.find(".content");
		var totalWidth =
			(parseInt(content.css("paddingLeft")) || 0);

		if ((status.errors + status.warnings) != 0)
		{
			totalWidth += $text.prop("scrollWidth");
			totalWidth += parseInt($text.css("marginLeft")) || 0;
			totalWidth += parseInt($text.css("marginRight")) || 0;
			totalWidth += parseInt(content.css("paddingRight")) || 0;
		}

		return totalWidth;
	}

	function loadLogData()
	{
		setStatusClass("loading");
		jQuery.ajax({ url: expandUrl(logUrl, { view: "xml" }), success: function onLogDataLoaded(response) /**/
		{
			var rows = selectNodes("//mod:log/mod:line", response, namespaces);
			var entry = null;
			for (var i = 0; i < rows.length; i++)
			{
				entry = {
					severity: rows[i].getAttribute("severity"),
					elapsed: rows[i].getAttribute("elapsed"),
					duration: rows[i].getAttribute("duration"),
					logger: rows[i].getAttribute("logger"),
					message: rows[i].getAttribute("message")
				};

				if (entry.severity == "ERROR")
					status.errors += 1;

				if (entry.severity == "WARN")
					status.warnings += 1;

				status.log.push(entry);
			}

			if (entry != null && entry.message.match(/\b(\d+)ms\b/))
				status.serverTime = RegExp.$1;

			updateToolbar();
		}});
	}

	function selectNodes(xpath, subject, namespaces)
	{
		if (arguments.length == 2 && (typeof arguments[1].nodeType) != "number")
		{
			xpath = arguments[0];
			namespaces = arguments[1];
		}

		if (!subject || (typeof arguments[1].nodeType) != "number")
			subject = document;

		var ownerDocument = subject.nodeType == 9 ? subject : subject.ownerDocument;
		var nsResolver = namespaces
			? function (prefix) { return namespaces[prefix]; }
			: null;

		var result = ownerDocument.evaluate(xpath, subject, nsResolver, 5, null);
		var node = result.iterateNext();
		var nodeList = [];
		while (node)
		{
			nodeList.push(node);
			node = result.iterateNext();
		}
		return nodeList;
	}


	function expandUrl(template, extraParams)
	{
		var params = jQuery.extend(parameters, extraParams);
		for (var name in params)
		{
			if (!params.hasOwnProperty(name))
				continue;

			template = template.replace(new RegExp("\\{" + name + "\\}"), params[name]);
		}

		return template;
	}

	function toggleLogWindow()
	{
		if ($frame.is(":visible"))
			hideLogWindow();
		else
			showLogWindow();
	}

	function showLogWindow()
	{
		if (logFrameLoaded)
		{
			$frame.show();
		}
		else
		{
			setStatusClass("loading");
			$iframe.load(onLogFrameLoaded);
			$iframe[0].contentWindow.location.replace(expandUrl(logUrl, { view: "html" }));
		}
	}

	function hideLogWindow()
	{
		$frame.hide();
	}

	function openMetaView(viewName)
	{
		var query = parseQuery(location.search.substring(1));
		query.view = viewName;

		var url = location.origin + location.pathname + "?" + query;
		var p = window.open(url, viewName);
		if (p)
			p.focus();
		else
			console.warn("Could not open '{0}' in a new window. Is there a popup blocker running?", url);
	}

	function parseQuery(queryString, options)
	{
		if (!queryString)
			return {};

		queryString = String(queryString).replace(/^\s*(.*)\s*$/, "$1");

		if (queryString.length == 0)
			return {};

		options = options || {};
		var opt = {
			separator: options.separator || "&",
			equals: options.equals || "="
		};

		var param = String(queryString).split(opt.separator);
		var query = {};
		for (var i = 0; i < param.length; i++)
		{
			if (param[i].length == 0)
				continue;

			var pair = param[i].split(opt.equals);
			var key = pair[0];
			var itemValue = pair[1] || $string.EMPTY;

			if (query[key] != null)
			{
				if (!query[key].push && !query[key].pop)
					query[key] = [query[key]];

				query[key].push(itemValue);
			}
			else
			{
				query[key] = itemValue;
			}
		}

		return query;
	}


	function onMetaViewCommandClick(e)
	{
		var metaView = $(this).attr("data-meta");
		openMetaView(metaView);
	}

	function onCloseLogViewClick()
	{
		hideLogWindow();
	}

	function onLogCommandClick()
	{
		toggleLogWindow();
	}

	function onInspectCommandClick()
	{
		document.location = expandUrl(inspectUrl);
	}

	function onToolbarMouseOver(e)
	{
		clearTimeout(hideTimeout);
		expandToolbar();
	}

	function onToolbarMouseOut(e)
	{
		hideTimeoutId = setTimeout(shrinkToolbar, hideTimeout);
	}

	function onLogFrameLoaded()
	{
		setStatusClass(getStatusClass());

		// this works because we are on the same domain
		var logbody = jQuery($iframe[0].contentWindow.document.body);
		var maxHeight = window.innerHeight / 2;
		var targetHeight = 0;

		$frame.css({ display: "block", visibility: "hidden" });

		var children = logbody.find(".logviewer > *");
		for (var i = 0; i < children.length; i++)
			targetHeight += children.eq(i).prop("scrollHeight");

		targetHeight = Math.max(targetHeight, 150);
		$frame.css({ visibility: "visible", height: Math.min(maxHeight, targetHeight) });
		logFrameLoaded = true;
	}

	function onDocumentKeyDown(e)
	{
		if (e.ctrlKey && e.altKey && e.keyCode == 68) // CTRL+ALT+D
		{
			toggle();
		}
	}

	function show()
	{
		$toolbar.show();
		setCookie("devtools", "on");
	}

	function hide()
	{
		$toolbar.hide();
		setCookie("devtools", "off");
	}

	function setCookie(name, value)
	{
		document.cookie = name + "=" + encodeURIComponent(value);
		return getCookie(name);
	}

	function getCookie(name)
	{
		var cookies = document.cookie.split("; ");
		for (var i = 0; i < cookies.length; i++)
		{
			var cookieName  = cookies[i].substring(0, cookies[i].indexOf("="));
			var cookieValue = cookies[i].substring(cookies[i].indexOf("=") + 1, cookies[i].length);
			if (cookieName == name)
			{
				if (cookieValue.indexOf("&") != -1)
				{
					var pairs  = cookieValue.split("&");
					var cookie = {};
					for (var j = 0; j < pairs.length; j++)
					{
						var arrTemp = pairs[j].split("=");
						cookie[arrTemp[0]] = arrTemp[1];
					}
					return cookie;
				}
				else
					return decodeURIComponent(cookieValue);
			}
		}
		return null;
	}

	function toggle()
	{
		if ($toolbar.is(":visible"))
			hide();
		else
			show();
	}

	window.addEventListener("load", setup);

	return {
		show: show,
		hide: hide,
		toggle: toggle
	}
})();
