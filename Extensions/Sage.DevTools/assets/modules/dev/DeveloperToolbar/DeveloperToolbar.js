atom.type.registerNamespace("sage.dev");

sage.dev.Toolbar = new function Toolbar()
{
	var jQuery = window.jQuery;
	var $toolbar, $icon, $text, $time, $commands, $frame, $iframe;
	var hideTimeout = 100, hideTimeoutId = null;
	var parameters = { basehref: "", thread: "0", url: encodeURIComponent(document.location) };
	var status = { log: [], errors: 0, warnings: 0, clientTime: 0, serverTime: 0 };

	var tooltip;
	var self = this;
	var logFrameLoaded = false;
	var logUrl = "{basehref}dev/log/{thread}/?view={view}";
	var inspectUrl = "{basehref}dev/inspect/?devtools=0#sage:vi=url%3D{url}%26inspector%3Dlog";

	var namespaces = { mod: "http://www.cycle99.com/schemas/sage/modules.xsd" };

	function setup()
	{
		if (top.window != window || atom.url.getQueryParam("devtools") == "0")
			return;

		$toolbar = jQuery("#developer-toolbar");
		$icon = $toolbar.find(".icon");
		$text = $toolbar.find(".status .text");
		$time = $toolbar.find(".status .time");
		$commands = $toolbar.find(".commands");
		$frame = jQuery(document.body)
			.append("<div id='developer-frame'><iframe frameborder='no'></iframe><div class='close' title='Close'></div></div>")
			.find("#developer-frame");

		tooltip = atom.controls.get($icon[0]);

		$iframe = $frame.find("iframe");

		parameters.thread = $toolbar.attr("data-thread");
		parameters.basehref = $toolbar.attr("data-basehref");

		status.clientTime = (Number(new Date()) - window.__time) || 0;

		$frame.find(".close").click(onCloseLogViewClick);
		$toolbar.find(".meta .button").click(onMetaViewCommandClick);
		$toolbar.find(".button.log").click(onLogCommandClick);
		$toolbar.find(".button.inspect").click(onInspectCommandClick);
		$toolbar.hover(onToolbarMouseOver, onToolbarMouseOut);

		if (atom.cookie.get("devtools") != "off")
			$toolbar.show();

		jQuery(document).bind("keydown", onDocumentKeyDown);

		loadLogData();

		atom.log.info("Developer console is ready, press CTRL-ALT-D to toggle it.");
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

		return atom.string.EMPTY;
	}

	function updateToolbar()
	{
		var statusClass = "", statusText = "", errorText;

		if (status.errors)
		{
			statusClass = "error";
			statusText = atom.string.format("{0} {1}.", status.errors, status.errors == 1 ? "error" : "errors");
			errorText = Enumerable.from(status.log)
				.where("$.severity == 'ERROR'")
				.select("\"<div class='error'>\" + $.message + \"</div>\"")
				.toArray()
					.join(atom.string.EMPTY);
		}
		else if (status.warnings)
		{
			statusClass = "warning";
			statusText = atom.string.format("{0} {1}.", status.warnings, status.warnings == 1 ? "warning" : "warnings");
			errorText = Enumerable.from(status.log)
				.where("$.severity == 'WARN'").select("\"<div class='error'>\" + $.message + \"</div>\"").toArray().join(atom.string.EMPTY);
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
				.text("{0}ms / {1}ms".format(status.serverTime, status.clientTime));
			$time
				.attr("title", "Server-side request: {0}ms, Client-side initialization: {1}ms".format(status.serverTime, status.clientTime));
		}

		setStatusClass(statusClass);
		$toolbar.find(".content").css({ width: getTotalToolbarWidth() });
	}

	function expandToolbar()
	{
		if (tooltip)
		{
			tooltip.hide();
		}

		var totalWidth = getTotalToolbarWidth();
		$toolbar.find(".content").css({ width: getTotalToolbarWidth() });
		$toolbar.animate({ width: totalWidth }, 50);
	}

	function shrinkToolbar()
	{
		var targetWidth = getToolbarMinWidth();
		$toolbar.animate({ width: targetWidth }, 50);
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
		jQuery.ajax({ url: expandUrl(logUrl, { view: "xml" }), success: function onLogDataLoaded(document) /**/
		{
			var rows = atom.xml.select("//mod:log/mod:line", document, namespaces);
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
		var url = atom.url();
		url.setQueryParam("view", viewName);

		var p = window.open(url.toString(), viewName);
		if (p)
			p.focus();
		else
			atom.log.warn("Could not open '{0}' in a new window. Is there a popup blocker running?", url);
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

		$frame.css({ visibility: "visible", height: Math.min(maxHeight, targetHeight) });
		logFrameLoaded = true;
	}

	function onDocumentKeyDown(e)
	{
		if (e.ctrlKey && e.altKey && e.keyCode == 68) // CTRL+ALT+D
		{
			self.toggle();
		}
	}

	this.show = function DeveloperToolbar$show()
	{
		$toolbar.show();
		atom.cookie.set("devtools", "on");
	};

	this.hide = function DeveloperToolbar$hide()
	{
		$toolbar.hide();
		atom.cookie.set("devtools", "off");
	};

	this.toggle = function DeveloperToolbar$toggle()
	{
		if ($toolbar.is(":visible"))
			this.hide();
		else
			this.show();
	};

	$(window).load(setup);
};
