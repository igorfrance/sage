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
		var element = $(this).closest(".element, .comment");
		var children = element.find("> .children, > pre, > .text");
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

		if (xmltree.hasClass("namespaces"))
		{
			xmltree.removeClass("namespaces");
			state.text("off");
		}
		else
		{
			xmltree.addClass("namespaces");
			state.text("on");
		}
	}

	function onToggleWordWrapClick()
	{
		var xmltree = $(this).closest(".xmltree");
		var state = $(this).closest(".toggler").find(".state");

		if (xmltree.hasClass("wrap"))
		{
			xmltree.removeClass("wrap");
			state.text("off");
		}
		else
		{
			xmltree.addClass("wrap");
			state.text("on");
		}
	}

	function expandElement(element)
	{
		element.find("> .children, > pre, > .text").show();
		element.find("> .switch").text("-");
	}

	function collapseElement(element)
	{
		element.find("> .children, > pre, > .text").hide();
		element.find("> .switch").text("+");
	}

	$(window).load(setup);
};
