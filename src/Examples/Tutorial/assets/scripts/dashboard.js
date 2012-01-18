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

	function onNavigationLinkClick()
	{
		try
		{
			showInspector();

			inspector.inspect(this.href);
		}
		catch(e)
		{
			$log.error(e);
		}

		return false;
	}

	$init.registerMain(setup);
};

