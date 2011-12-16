$(document).ready(function setupView()
{
	var selectControl = $(".selectcontrol");
	var selectOptions = $(".selectcontrol ul.options");

	function showSelectControl()
	{
		selectOptions.show();
	}

	function hideSelectControl()
	{
		selectOptions.hide();
	}

	function onDocumentKeyDown(e)
	{
		if (selectOptions.is(":visible"))
			hideSelectControl();
	}

	function onDocumentMouseDown(e)
	{
		if (selectControl.has(e.target).length == 0)
		{
			if (selectOptions.is(":visible"))
				hideSelectControl();
		}
	}

	function onSelectControlClick()
	{
		if (selectOptions.is(":visible"))
			hideSelectControl();
		else
			showSelectControl();
	}

	function isElementChildOf(element, parent)
	{
		if (element == parent)
			return true;

		while ((parent = parent.parentNode))
		{
			if (parent == element)
				return true;
		}

		return false;
	}

	function onToggleSectionHeaderClicked()
	{
		var target = $(this).parent(".togglesection");
		if (target.hasClass("expanded"))
			target.removeClass("expanded");
		else
			target.addClass("expanded");
	}

	function onSelectLocaleClicked(e)
	{
		var srcElement = $($evt.srcElement(e)).closest("li");
		var locale = srcElement.text().trim();
		$("#resources_files .locale").hide();
		$("#resources_files .locale." + locale).show();
	}

	function onSelectOptionClicked(e)
	{
		var srcElement = $($evt.srcElement(e)).closest("li");
		var parentControl = srcElement.closest(".selectcontrol");

		parentControl.find(".selected").text(srcElement.text());
	}


	selectControl.click(onSelectControlClick);

	$(".togglesection > .header").click(onToggleSectionHeaderClicked);
	$(".selectcontrol li").click(onSelectOptionClicked);
	$("#select_locale li").click(onSelectLocaleClicked);

	$(document).keydown(onDocumentKeyDown);
	$(document).mousedown(onDocumentMouseDown);

});
