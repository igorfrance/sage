Type.registerNamespace("sage.devtools");

sage.devtools.XmlTree = new function XmlTree()
{
	function setup()
	{
		$(".xmltree .toolbar a.wrap").click(onToggleWordWrapClick);
		$(".xmltree .toolbar a.namespaces").click(onToggleNamespacesClick);
		$(".xmltree .toolbar a.toggleall").click(onToggleAllClick);
		$(".xmltree .switch").click(onSwitchClick);
	}

	function onSwitchClick()
	{
		var element = $(this).closest(".element");
		var children = element.find("> .children");
		if (children.length)
		{
			if (children.is(":visible"))
				collapseElement(element);
			else
				expandElement(element);
		}
	}

	function onToggleAllClick()
	{
		var xmltree = $(this).closest(".xmltree");
		var allvisible = xmltree.find(".children:hidden").length == 0;

		xmltree.find(".element").each(function (i, element)
		{
			if (allvisible)
				collapseElement($(element));
			else
				expandElement($(element));
		});
	}

	function onToggleNamespacesClick()
	{
		var xmltree = $(this).closest(".xmltree");
		var state = $(this).closest(".toggler").find(".state");
		var visible = xmltree.attr("data-namespacesVisible") == "true";
		visible = !visible;

		xmltree.find(".nsattrib").each(function (i, element)
		{
			$(element).toggle(visible);
		});

		xmltree.attr("data-namespacesVisible", visible);
		state.text(visible ? "on" : "off");
	}

	function onToggleWordWrapClick()
	{
		var xmlroot = $(this).closest(".xmltree").find(".xmlroot");
		var state = $(this).closest(".toggler").find(".state");
		var whitespace = xmlroot.css("white-space");
		whitespace = whitespace == "nowrap" ? "normal" : "nowrap";

		xmlroot.css("white-space", whitespace);
		state.text(whitespace == "nowrap" ? "off" : "on");
	}

	function expandElement(element)
	{
		element.find("> .children").show();
		element.find("> .switch").text("-");
	}

	function collapseElement(element)
	{
		element.find("> .children").hide();
		element.find("> .switch").text("+");
	}

	$(window).load(setup);
};
