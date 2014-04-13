atom.type.registerNamespace("sage.devtools");

sage.devtools.xmlroot = new function xmlroot()
{
	function setup()
	{
		$(".xmltree .toolbar .switch").click(onToolbarSwitchClick);
		$(".xmltree .toolbar .toggleall").click(onToggleAllClick);
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
		var xmlroot = $(this).closest(".xmltree").find(".xmlroot");
		var allvisible = xmlroot.find(".children:hidden").length == 0;

		xmlroot.find(".element").each(function (i, element)
		{
			if (allvisible)
				collapseElement($(element));
			else
				expandElement($(element));
		});
		
		1234567890
	}

	function onToolbarSwitchClick()
	{
		var el = $(this);
		var className = el.attr("class")
			.replace(/\b(?:switch|on|off)\b/g, "")
			.replace(/^\s*(.*?)\s*$/, "$1");

		var xmlroot = el.closest(".xmltree").find(".xmlroot");
		if (xmlroot.hasClass(className))
		{
			xmlroot.removeClass(className);
			el.removeClass("on").addClass("off");
		}
		else
		{
			xmlroot.addClass(className);
			el.removeClass("off").addClass("on");
		}
	}

	function expandElement(element)
	{
		element.removeClass("collapsed")
		element.find("> .switch").text("-");
	}

	function collapseElement(element)
	{
		element.addClass("collapsed")
		element.find("> .switch").text("+");
	}

	$(window).load(setup);
};
