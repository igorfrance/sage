Type.registerNamespace("aeon.utils");

aeon.utils.Dom = {};

aeon.utils.Dom.uniqueID = function Dom$uniqueID(element)
{
	if (element && element.uniqueID)
		return element.uniqueID;

	if (aeon.utils.Dom.__uid == null)
		aeon.utils.Dom.__uid = {};

	var uniqueID = new Date().getTime();
	while (aeon.utils.Dom.__uid[uniqueID] != null)
		uniqueID++;

	aeon.utils.Dom.__uid[uniqueID] = uniqueID;

	uniqueID = "__unique" + uniqueID;

	if (element)
		element.uniqueID = uniqueID;

	return uniqueID;
};

aeon.utils.Dom.makeUnselectable = function Dom$makeUnselectable()
{
	var elements = Array.fromArguments(arguments);

	for (var i = 0; i < elements.length; i++)
	{
		var element = elements[i];
		element.style.userSelect =
			element.style.MozUserSelect = "none";

		element.setAttribute("unselectable", "on");
		var childNodes = element.getElementsByTagName("*");
		for (var i = 0; i < childNodes.length; i++)
		{
			childNodes[i].style.userSelect =
			childNodes[i].style.MozUserSelect = "none";
			childNodes[i].setAttribute("unselectable", "on");
		}
	}
}

aeon.utils.Dom.makeSelectable = function Dom$makeSelectable()
{
	var elements = Array.fromArguments(arguments);

	for (var i = 0; i < elements.length; i++)
	{
		var element = elements[i];
		element.style.userSelect =
			element.style.MozUserSelect = String.EMPTY;

		element.removeAttribute("unselectable");
		var childNodes = element.getElementsByTagName("*");
		for (var i = 0; i < childNodes.length; i++)
		{
			childNodes[i].style.userSelect =
			childNodes[i].style.MozUserSelect = String.EMPTY;
			childNodes[i].removeAttribute("unselectable");
		}
	}
}

var $dom = aeon.utils.Dom;
