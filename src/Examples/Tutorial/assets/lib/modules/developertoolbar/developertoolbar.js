Type.registerNamespace("sage.dev");

sage.dev.Toolbar = new function ()
{
	var $toolbar, $icon, $text, $time, $commands, $frame, $iframe, $logbody;
	var hideTimeout = 100, hideTimeoutId = null;
	var parameters = { basehref: "", thread: "0", url: escape(document.location) };
	var status = { log: [], errors: [], warnings: [], clientTime: 0, serverTime: 0 };

	var logUrl = "{basehref}dev/log/{thread}";
	var inspectUrl = "{basehref}dev/inspect/?devtools=0#sage:vi=url%3D{url}%26inspector%3Dlog";

	function setup()
	{
		if (top.window != window || $url.getQueryParam("devtools") == "0")
			return;

		$toolbar = jQuery("#developer-toolbar");
		$icon = $toolbar.find(".icon");
		$text = $toolbar.find(".status .text");
		$time = $toolbar.find(".status .time");
		$commands = $toolbar.find(".commands");
		$frame = jQuery(document.body)
			.append("<div id='developer-frame'><iframe frameborder='no'></iframe><div class='close' title='Close'></div></div>")
			.find("#developer-frame");

		$iframe = $frame.find("iframe");

		parameters.thread = $toolbar.attr("data-thread");
		parameters.basehref = $toolbar.attr("data-basehref");

		status.clientTime = (Number(new Date()) - window.__time) || 0;

		$iframe.load(onDeveloperFrameLoaded);
		$frame.find(".close").click(onCloseLogViewClick);
		$toolbar.find(".meta .button").click(onMetaViewCommandClick);
		$toolbar.find(".button.log").click(onLogCommandClick);
		$toolbar.find(".button.inspect").click(onInspectCommandClick);
		$toolbar.hover(onToolbarMouseOver, onToolbarMouseOut);
		$toolbar.show();

		loadLogHtml();
	}

	function setStatusIcon(status)
	{
		$icon.removeClass("ok error warn loading").addClass(status);
	}

	function updateStatus()
	{
		var statusName = "", statusText = "";
		if (status.errors.length)
		{
			statusName = "error";
			statusText = String.format("{0} {1}.", status.errors.length, status.errors.length == 1 ? "error" : "errors");
		}
		else if (status.warnings.length)
		{
			statusName = "warning";
			statusText = String.format("{0} {1}.", status.warnings.length, status.warnings.length == 1 ? "warning" : "warnings");
		}
		else if (status.log.length)
		{
			statusName = "ok";
			statusText = "OK.";
		}

		if (statusText)
		{
			$text.find("label").text(statusText);
			if (status.errors.length + status.warnings.length != 0)
				$text.animate({ width: $text.prop("scrollWidth") }, 50);
		}

		if (status.serverTime || status.clientTime)
		{
			$time.find("label").text("{0}ms / {1}ms".format(status.serverTime, status.clientTime));
		}

		setStatusIcon(statusName);
	}

	function loadLogHtml()
	{
		setStatusIcon("loading");
		$iframe.attr("src", expandUrl(logUrl));
	}

	function expandToolbar()
	{
		if (status.errors.length + status.warnings.length == 0)
			$text.animate({ width: $text.prop("scrollWidth") }, 50);

		$time.animate({ width: $time.prop("scrollWidth") }, 50);
		$commands.animate({ width: $commands.prop("scrollWidth") }, 50);
	}

	function contractToolbar()
	{
		if (status.errors.length + status.warnings.length == 0)
			$text.animate({ width: 0 }, 50);

		$time.animate({ width: 0 }, 50);
		$commands.animate({ width: 0 }, 50);
	}

	function expandUrl(template)
	{
		for (var name in parameters)
		{
			template = template.replace(new RegExp("\\{" + name + "\\}"), parameters[name]);
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
		$frame.show();
	}

	function hideLogWindow()
	{
		$frame.hide();
	}

	function openMetaView(viewName)
	{
		var url = new $url();
		url.setQueryParam("view", viewName);

		var p = window.open(url.toString(), viewName);
		if (p)
			p.focus();
		else
			$log.warn("Could not open '{0}' in a new window. Is there a popup blocker running?", url);
	}

	function onToolbarMouseOver(e)
	{
		clearTimeout(hideTimeout);
		expandToolbar();
	}

	function onToolbarMouseOut(e)
	{
		hideTimeoutId = setTimeout(contractToolbar, hideTimeout);
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

	function onDeveloperFrameLoaded()
	{
		setStatusIcon(String.EMPTY);

		// this works because we are on the same domain
		$logbody = jQuery($iframe[0].contentWindow.document.body);

		var rows = $logbody.find(".logviewer .content table tr");
		var entry = null;
		for (var i = 0; i < rows.length; i++)
		{
			entry = {
				severity: rows.eq(i).find("td.severity").text().trim(),
				elapsed: rows.eq(i).find("td.elapsed").text().trim(),
				duration: rows.eq(i).find("td.duration").text().trim(),
				logger: rows.eq(i).find("td.logger").text().trim(),
				message: rows.eq(i).find("td.message").text().trim()
			};

			if (entry.severity == "ERROR")
				status.errors.push(entry.message);

			if (entry.severity == "WARN")
				status.warnings.push(entry.message);

			status.log.push(entry);
		}

		if (entry != null && entry.message.match(/\b(\d+)ms\b/))
			status.serverTime = RegExp.$1;

		var maxHeight = $(document.body).prop("scrollHeight") / 2;
		var targetHeight = 0;

		$frame.css({ display: "block", visibility: "hidden" });

		var children = $logbody.find(".logviewer > *");
		for (var i = 0; i < children.length; i++)
			targetHeight += children.eq(i).prop("scrollHeight");

		$frame.css({ display: "none", visibility: "visible", height: targetHeight });

		updateStatus();
	}

	$(window).load(setup);
};
