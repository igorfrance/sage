Type.registerNamespace("sage.documentation");

sage.documentation.ExpandSection = new function ExpandSection()
{
	function setup()
	{
		$("section.expand > header").click(onExpandHeaderClick);
	}

	function onExpandHeaderClick(e)
	{
		var section = $(e.target).closest("section");
		section.toggleClass("collapsed");
	}

	$(window).ready(setup);
};

