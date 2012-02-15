Type.registerNamespace("sage.tutorial");

sage.tutorial.Dashboard = new function Dashboard()
{
	var inspector;
	var inspectorElement;
	var welcomeElement;

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

		jQuery("#navigation ul a").bind("click", onNavigationLinkClick);
	}

	function showInspector()
	{
		if (inspectorElement.is(":visible"))
			return;

		inspectorElement.fadeIn();
		welcomeElement.fadeOut();
	}

	function hideInspector()
	{
		if (!inspectorElement.is(":visible"))
			return;

		welcomeElement.fadeIn();
		inspectorElement.fadeOut();
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

	$(window).ready(setup);
};

