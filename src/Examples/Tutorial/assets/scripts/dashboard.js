Type.registerNamespace("sage.tutorial");

sage.tutorial.Dashboard = new function Dashboard()
{
	var inspector;
	var inspectorElement;
	var welcomeElement;
	var navigationElement;

	function setup()
	{
		if (!sage.dev || !sage.dev.ViewInspector)
			return $log.warn("sage.dev.ViewInspector is a required component for this page");

		inspector = sage.dev.ViewInspector.current;
		inspector.addListener("close", onViewInspectorClose);
		inspector.addListener("info", onViewInspectorInfo);
		inspector.addListener("contentloaded", onViewInspectorLoaded);

		inspectorElement = jQuery("#examples");
		welcomeElement = jQuery("#welcome");
		navigationElement = jQuery("#navigation");

		jQuery("#navigation ul a").bind("click", onNavigationLinkClick);
		jQuery("#welcome a.documentation").bind("click", onDocumentationLinkClick);

		if ($url.getHashParam("sage:vi") != null)
		{
			navigationElement.css({ left: 0 });
		}
	}

	function showInspector()
	{
		if (inspectorElement.is(":visible"))
			return;

		sage.dev.Toolbar.hide();
		inspectorElement.fadeIn();
		welcomeElement.fadeOut();
	}

	function hideInspector()
	{
		if (!inspectorElement.is(":visible"))
			return;

		navigationElement.animate({ left: -navigationElement.width() });
		welcomeElement.fadeIn();
		inspectorElement.fadeOut(function onFadeComplete()
		{
			sage.dev.Toolbar.show();
		});
	}

	function onViewInspectorClose()
	{
		hideInspector();
	}

	function onViewInspectorLoaded()
	{
		showInspector();
	}

	function onViewInspectorInfo()
	{
		showInspector();
	}

	function onNavigationLinkClick()
	{
		try
		{
			showInspector();
			inspector.inspect(this.href);
			expandNavigationGroup(jQuery(this).closest("li.expandable"));
		}
		catch(e)
		{
			$log.error(e);
		}

		return false;
	}

	function onDocumentationLinkClick(e)
	{
		var targetUrl = this.href;

		welcomeElement.animate({ opacity: 0 });
		navigationElement.animate({ left: 0 }, function ()
		{
			welcomeElement.hide();
			showInspector();
			inspector.inspect(targetUrl);
		});

		return false;
	}

	$(window).ready(setup);
};

