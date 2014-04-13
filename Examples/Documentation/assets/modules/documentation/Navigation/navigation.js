atom.type.registerNamespace("sage.documentation");

sage.documentation.Navigation = new function Navigation()
{
	function setup()
	{
		jQuery("#navigation li.expandable").bind("click", onExpandableItemClick);
	}

	function expandNavigationGroup(li)
	{
		li.addClass("expanded");
	}

	function collapseNavigationGroup(li)
	{
		li.removeClass("expanded");
	}

	function toggleNavigationGroup(li)
	{
		if (li.hasClass("expanded"))
			collapseNavigationGroup(li);
		else
			expandNavigationGroup(li);
	}

	function onExpandableItemClick(e)
	{
		toggleNavigationGroup(jQuery(this));
	}

	atom.init.registerMain(setup);
};
