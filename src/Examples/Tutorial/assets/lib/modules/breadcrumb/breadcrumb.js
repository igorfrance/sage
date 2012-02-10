Type.registerNamespace("sage.tutorial");

sage.tutorial.BreadCrumb = new function BreadCrumb()
{
	var $bc = jQuery("div.breadcrumb");

	$bc.find(".separator")
		.bind("mouseenter", onSeparatorMouseEnter)
		.bind("mouseleave", onSeparatorMouseLeave);

	$bc.find(".children .expander")
		.bind("mouseup", onExpanderMouseUp);

	function onSeparatorMouseEnter(e)
	{
		var $children = jQuery(this).find(".children");
		$children.show();
	}

	function onSeparatorMouseLeave()
	{
		var $children = jQuery(this).find(".children");
		$children.find("li").removeClass("expanded");
		$children.hide();
	}

	function onExpanderMouseUp()
	{
		var $parent = jQuery(this).closest("li");
		$parent.toggleClass("expanded");
	}

};

