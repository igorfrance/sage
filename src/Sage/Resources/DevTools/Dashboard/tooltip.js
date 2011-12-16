Type.registerNamespace("wl.controls");

/**
 * Implements a control that show a floating layer with title text when the mouse is moved over certain elements.
 * This control can be enabled in several ways:
 * <ol><li>If the <c>HoverAll</c> setting is set to true, the layer will appear on mouseover for all elements
 * that have their title attribute set</li>
 * <li>If some elements in the current DOM have the attribute x:Tooltip set, the layer will appear on mouseover for
 * those layers and display the value of this attribute</li>
 * <li>If some elements in the current DOM have the <c>ClassName</c> class name set, the layer will appear on
 * mouseover for those layers and display the value of their title attribute</li></ul>
 * For correct working, the css appplicable in the current document needs to define the style for an element with
 * id defined with <c>LayerID</c>. See the example for more info.
 * @example lang=css Example CSS for the Tooltip element
 * #tooltip {
 *	background-color: #FF6600; color: white; border: 1px solid #4F5C71; padding: 2px; position: absolute;
 *	height: 18px; visibility: hidden; filter: alpha(opacity=0.8); opacity: 0.8;
 * }
 * @param {HTMLElement} element The element to initialize.
 */
wl.controls.Tooltip = function Tooltip(element)
{
	this.element = element;

	var title = this.element.getAttribute("title");
	var tooltip = this.element.getAttribute("tooltip");

	if (title != null && title != "")
		this.title = title.replace(/\n/g, "<br/>");

	if (tooltip != null && tooltip != "")
		this.tooltip = tooltip;

	this.element.removeAttribute("title");
	$(this.element).bind("mouseover", Function.createDelegate(this, this.onmouseover)); /**/
	$(this.element).bind("mousemove", Function.createDelegate(this, this.onmousemove)); /**/
	$(this.element).bind("mouseout", Function.createDelegate(this, this.onmouseout)); /**/
};

wl.controls.Tooltip.isFramed = false;
/**
 * Indicates that any element that has the title attribute set should cause the <c>Tooltip</c> layer to appear on mouseover.
 * @type {Boolean}
 */
wl.controls.Tooltip.HoverAll = false;
/**
 * Specifies the maximum width for the popup layer
 * @type {Number}
 */
wl.controls.Tooltip.MaxWidth = 300;
/**
 * Specifies the class name of the element for which the <c>Tooltip</c> layer should appear on mouseover.
 * @type {Number}
 */
wl.controls.Tooltip.ClassName = "tooltip";
/**
 * Specifies the id of the <c>Tooltip</c> layer element.
 * @type {Number}
 */
wl.controls.Tooltip.LayerID = "tooltip";

wl.controls.Tooltip.OFFSET_LEFT = 10;

wl.controls.Tooltip.OFFSET_TOP = 3;

wl.controls.Tooltip.MARGIN = 5;

/**
 * Counter used to generate unique IDs for page elements.
 * @type {Number}
 */
wl.controls.Tooltip.uniqueIdCounter = 0;
/**
 * Specifies the minimum necessary css for the floating tooltip element to make sense.
 * @type {String}
 */
wl.controls.Tooltip.DefaultCss =
	"background-color: #FF6600; color: #fff; padding: 4px; position: absolute; left: 0; top: 0; font-size: 12px; font-family: verdana; opacity: .9;";
/**
 * Creates a unique id for the <c>Tooltip</c> element.
 * @param {HTMLElement} element
 */
wl.controls.Tooltip.uniqueId = function Tooltip$uniqueId(element)
{
	var suffix = "UniqueId";
	if (!element.id)
	{
		element.id = "n"+ (++wl.controls.Tooltip.uniqueIdCounter) + suffix;
	}

	return element.id;
};

/**
 * Sets up the <c>Tooltip</c> control
 */
wl.controls.Tooltip.setup = function Tooltip$setup()
{
	wl.controls.Tooltip.initializeElements($(".tooltip"));
	wl.controls.Tooltip.initializeCoordinates();

	$(window).resize(wl.controls.Tooltip.initializeCoordinates);
};
/**
 * Disposes the <c>Tooltip</c> control
 */
wl.controls.Tooltip.dispose = function Tooltip$dispose()
{
	for (var uniqueID in wl.controls.Tooltip.__elements)
	{
		wl.controls.Tooltip.disposeInstance(uniqueID);
	}
};
/**
 * Disposes the <c>Tooltip</c> element
 * @param {HTMLElement} element
 */
wl.controls.Tooltip.disposeElement = function Tooltip$disposeElement(element)
{
	var uniqueID = wl.controls.Tooltip.uniqueId(element);
	wl.controls.Tooltip.disposeInstance(uniqueID);
};
/**
 * Disposes the <c>Tooltip</c> instance
 
 */
wl.controls.Tooltip.disposeInstance = function Tooltip$disposeInstance(uniqueID)
{
	if (wl.controls.Tooltip.__elements && wl.controls.Tooltip.__elements[uniqueID] != null)
	{
		wl.controls.Tooltip.__elements[uniqueID].dispose();
		wl.controls.Tooltip.__elements[uniqueID] = null;
	}
};

/**
 * Initializes the elements for which the <c>Tooltip</c>layer should appear on mouseover.
 * @param {HTMLElement} element
 */
wl.controls.Tooltip.initializeElements = function Tooltip$initializeElements(elements)
{
	elements.each(function (index, element)
	{
		if (!element || !element.tagName)
			return;

		var init = false;
		if ($(element).hasClass(wl.controls.Tooltip.ClassName))
			init = true;

		else if (element.getAttribute("tooltip"))
			init = true;

		else if (element.getAttribute("title") && wl.controls.Tooltip.HoverAll)
			init = true;

		if (init)
			wl.controls.Tooltip.initializeElement(element);
	});

	if (wl.controls.Tooltip.__layer == null)
		wl.controls.Tooltip.createPopupLayer();
};

/**
 * Initializes a single element for which the <c>Tooltip</c>layer should appear on mouseover.
 * @param {HTMLElement} element
 */
wl.controls.Tooltip.initializeElement = function Tooltip$initializeElement(element)
{
	var uniqueID = wl.controls.Tooltip.uniqueId(element);

	if (wl.controls.Tooltip.__elements == null)
		wl.controls.Tooltip.__elements = {};

	if (wl.controls.Tooltip.__elements[uniqueID] == null)
		wl.controls.Tooltip.__elements[uniqueID] = new wl.controls.Tooltip(element);
};

/**
 * Initializes the window coordinates for proper calculation when the hover elements appear within an iframe.
 */
wl.controls.Tooltip.initializeCoordinates = function Tooltip$initializeCoordinates()
{
	wl.controls.Tooltip.__windowCoord = [0,0];

	var currentWindow = window;
	if (window != window.parent)
	{
		wl.controls.Tooltip.isFramed = true;
		while (currentWindow != top)
		{
			currentWindow = currentWindow.parent;
			var frames1 = currentWindow.document.getElementsByTagName("IFRAME");
			var frames2 = currentWindow.document.getElementsByTagName("FRAME");
			var foundFrame = false;
			for (var i = 0; i < frames1.length; i++)
			{
				if (frames1[i].contentWindow == window)
				{
					wl.controls.Tooltip.__windowCoord[0] += $(frames1[i]).offset().left;
					wl.controls.Tooltip.__windowCoord[1] += $(frames1[i]).offset().top;
					foundFrame = true;
					break;
				}
			}
			if (foundFrame != true)
			{
				for (var i = 0; i < frames2.length; i++)
				{
					if (frames2[i].contentWindow == window)
					{
						wl.controls.Tooltip.__windowCoord[0] += $(frames2[i]).offset().left;
						wl.controls.Tooltip.__windowCoord[1] += $(frames2[i]).offset().top;
						foundFrame = true;
						break;
					}
				}
			}
		}
	}
};

/**
 * Creates the popup layer.
 */
wl.controls.Tooltip.createPopupLayer = function Tooltip$createPopupLayer()
{
	var doc = top.document;
	var body = doc.body;

	var hlayer = doc.getElementById(wl.controls.Tooltip.LayerID);
	if (hlayer != null)
	{
		wl.controls.Tooltip.__layer = hlayer;
	}
	else
	{
		var l = body.appendChild(doc.createElement("DIV"));
		l.style.cssText = wl.controls.Tooltip.DefaultCss;
		l.id = wl.controls.Tooltip.LayerID;

		$(l).mouseover(wl.controls.Tooltip.hide);
		$(l).hide();

		wl.controls.Tooltip.__layer = l;
	}
};
/**
 * Disposes <c>Tooltip</c>
 */
wl.controls.Tooltip.prototype.dispose = function Tooltip$dispose()
{
	this.element.setAttribute("title", this.title);
	$(this.element).unbind("mouseover", Function.getDelegate(this, this.onmouseover)); /**/
	$(this.element).unbind("mousemove", Function.getDelegate(this, this.onmousemove)); /**/
	$(this.element).unbind("mouseout", Function.getDelegate(this, this.onmouseout)); /**/
};

/**
 * Shows the <c>Tooltip</c> layer for Tooltip elements.
 * @param {DOMEvent} e The DOM event that occured.
 */
wl.controls.Tooltip.prototype.onmouseover = function Tooltip$onmouseover(e)
{
	var maxWidth = this.element.getAttribute("tooltipwidth") || wl.controls.Tooltip.MaxWidth;

	var layer = $(wl.controls.Tooltip.__layer);

	layer.text("");
	layer.hide();

	var title = this.Tooltip || this.title;
	if (title && title.trim().length != 0)
	{
		layer.text(title);
		layer.css({ width: "auto", height: "auto" });

		var layerPadding = layer.css("paddingLeft") + layer.css("paddingRight");
		if (layer.width() > maxWidth)
			layer.width(maxWidth - layerPadding);
		else
			layer.width(layer.width() - layerPadding);

		layer.show();
		wl.controls.Tooltip.active = true;
	}
	else
		wl.controls.Tooltip.active = false;
};

/**
 * Keeps the <c>Tooltip</c> layer moving close to the mouse as long as it is moving above a Tooltip element.
 * @param {DOMEvent} e The DOM event that occured.
 */
wl.controls.Tooltip.prototype.onmousemove = function Tooltip$onmousemove(e)
{
	if (wl.controls.Tooltip.active == false)
		return;

	var layer = $(wl.controls.Tooltip.__layer);
	layer.hide();
	layer.position(0, 0);

	var doc = top.document;

	var body = $("body")[0];
	var html = $("html")[0];

	var bodyHeight = html.scrollHeight || body.scrollHeight;
	var bodyWidth = html.scrollWidth || body.scrollWidth;

	if (wl.controls.Tooltip.isFramed)
	{
		var scrollTop = 0;
		var scrollLeft = 0;
	}
	else
	{
		var scrollTop = html.scrollTop || body.scrollTop;
		var scrollLeft = html.scrollLeft || body.scrollLeft;
	}

	var eventX = e.clientX + wl.controls.Tooltip.__windowCoord[0];
	var eventY = e.clientY + wl.controls.Tooltip.__windowCoord[1];

	var targetLeft = (eventX + scrollLeft) + wl.controls.Tooltip.OFFSET_LEFT;
	var targetTop = (eventY + scrollTop) - layer.height() - wl.controls.Tooltip.OFFSET_TOP;

	if ((targetTop + layer.height()) > bodyHeight - wl.controls.Tooltip.MARGIN)
		targetTop -= layer.height();
	if (targetTop < 0)
		targetTop = 5;

	if ((targetLeft + layer.width()) > bodyWidth - wl.controls.Tooltip.MARGIN)
		targetLeft -= (layer.width() + 30);
	if (targetLeft < 0)
		targetLeft = 5;

	layer.show();
	layer.css({ left: targetLeft, top: targetTop });
	layer.css({ "z-index": 1000001 });
};

/**
 * Hides the <c>Tooltip</c> layer.
 * @param {DOMEvent} e The DOM event that occured.
 */
wl.controls.Tooltip.prototype.onmouseout = function Tooltip$onmouseout()
{
	wl.controls.Tooltip.hide();
};

/**
 * Hides the <c>Tooltip</c> layer.
 * @param {DOMEvent} e The DOM event that occured.
 */
wl.controls.Tooltip.hide = function Tooltip$hide()
{
	$(wl.controls.Tooltip.__layer).hide();
};

$(window).ready(wl.controls.Tooltip.setup);
