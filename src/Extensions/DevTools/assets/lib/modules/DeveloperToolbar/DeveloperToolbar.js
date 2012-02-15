Type.registerNamespace("sage.dev");

sage.dev.Toolbar = new function Toolbar()
{
	var $toolbar, $icon, $text, $time, $commands, $frame, $iframe, $logbody;
	var hideTimeout = 100, hideTimeoutId = null;
	var tooltip;
	var parameters = { basehref: "", thread: "0", url: escape(document.location) };
	var status = { log: [], errors: 0, warnings: 0, clientTime: 0, serverTime: 0 };

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

		tooltip = $ctrl.getControl($text[0]);

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

	function setStatusClass(status)
	{
		$icon.removeClass("ok error warn loading").addClass(status);
	}

	function updateStatus()
	{
		var statusClass = "", statusText = "", errorText;

		if (status.errors)
		{
			statusClass = "error";
			statusText = String.format("{0} {1}.", status.errors, status.errors == 1 ? "error" : "errors");
			errorText = Enumerable.from(status.log)
				.where("$.severity == 'ERROR'").select("\"<div class='error'>\" + $.message + \"</div>\"").toArray().join(String.EMPTY);
		}
		else if (status.warnings)
		{
			statusClass = "warning";
			statusText = String.format("{0} {1}.", status.warnings, status.warnings == 1 ? "warning" : "warnings");
			errorText = Enumerable.from(status.log)
				.where("$.severity == 'WARN'").select("\"<div class='error'>\" + $.message + \"</div>\"").toArray().join(String.EMPTY);
		}
		else if (status.log.length)
		{
			statusClass = "ok";
			statusText = "OK.";
			errorText = "Ok";
		}

		// required for chrome, don't remove without verifying that it still works correctly
		//$toolbar.css({ width: $icon.outerWidth() });

		if (statusText)
		{
			$text.find("label").text(statusText);
			$icon.attr("title", errorText);
			$text.attr("title", errorText);
			if (tooltip && (status.errors + status.warnings != 0))
			{
				$text.animate({ width: $text.prop("scrollWidth") }, 50, function onExpandComplete()
				{
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
	}

	function loadLogHtml()
	{
		setStatusClass("loading");
		$iframe.attr("src", expandUrl(logUrl));
	}

	function disableTooltip()
	{
		if (tooltip)
		{
			tooltip.hide();
			tooltip.setSettingsValue("hideOn", "mouseout");
			tooltip.setSettingsValue("useFades", false);
		}

		$toolbar.find(".tooltip").addClass(".tooltip-suspend");
	}

	function enableTooltip()
	{
		$toolbar.find(".tooltip").removeClass(".tooltip-suspend");
	}

	function expandToolbar()
	{
		disableTooltip();

		var totalWidth = getTotalToolbarWidth();
		$toolbar.animate({ width: totalWidth }, 50, enableTooltip);
	}

	function getTotalToolbarWidth()
	{
		var content = $toolbar.find(".content");
		var totalWidth =
			$icon.prop("offsetWidth") +
			$text.prop("scrollWidth") +
			$time.prop("scrollWidth") +
			(parseInt(content.css("paddingLeft")) || 0) +
			(parseInt(content.css("paddingRight")) || 0) +
			getTotalCommandsWidth();

		return totalWidth;
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

	function contractToolbar()
	{
		var content = $toolbar.find(".content");
		var targetWidth =
			$icon.prop("offsetWidth") +
			(parseInt(content.css("paddingLeft")) || 0) +
			(parseInt(content.css("paddingRight")) || 0);

		if ((status.errors + status.warnings) != 0)
			targetWidth += $text.prop("scrollWidth");

		$toolbar.animate({ width: targetWidth }, 50, enableTooltip);
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
		setStatusClass(String.EMPTY);

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
				status.errors += 1;

			if (entry.severity == "WARN")
				status.warnings += 1;

			status.log.push(entry);
		}

		if (entry != null && entry.message.match(/\b(\d+)ms\b/))
			status.serverTime = RegExp.$1;

		var maxHeight = $(document.body).prop("offsetHeight") / 2;
		var targetHeight = 0;

		$frame.css({ display: "block", visibility: "hidden" });

		var children = $logbody.find(".logviewer > *");
		for (var i = 0; i < children.length; i++)
			targetHeight += children.eq(i).prop("scrollHeight");

		$frame.css({ display: "none", visibility: "visible", height: Math.min(maxHeight, targetHeight) });

		updateStatus();
	}

	$(window).load(setup);
};
