Type.registerNamespace("sage.tutorial");

sage.tutorial.BreadCrumb = new function BreadCrumb()
{
	var $bc = jQuery("div.breadcrumb");

	var showTimer, hideTimer;
	var delay = 100;

	$bc.find(".separator")
		.bind("mouseenter", onSeparatorMouseEnter)
		.bind("mouseleave", onSeparatorMouseLeave);

	$bc.find(".children .expander")
		.bind("mouseup", onExpanderMouseUp);

	function onSeparatorMouseEnter(e)
	{
		window.clearTimeout(hideTimer);

		var subject = jQuery(this);
		showTimer = Function.setTimeout(delay, function showChildren()
		{
			subject.find(".children").show();
		});
	}

	function onSeparatorMouseLeave()
	{
		window.clearTimeout(showTimer);

		var subject = jQuery(this);
		hideTimer = Function.setTimeout(delay, function hideChildren()
		{
			subject.find(".children").hide().find("li").removeClass("expanded");
		});
	}

	function onExpanderMouseUp()
	{
		var $parent = jQuery(this).closest("li");
		$parent.toggleClass("expanded");
	}

};

