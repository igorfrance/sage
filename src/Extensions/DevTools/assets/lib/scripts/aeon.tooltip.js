Type.registerNamespace("aeon.controls");

/**
 * Implements a control that show a floating layer with title text when the mouse is moved over certain elements.
 * For correct working, the css appplicable in the current document needs to define the style for an element with
 * id defined with <c>LayerID</c>. See the example for more info.
 * @example lang=css Example CSS for the tooltip element
 * #tooltip {
 *	background-color: #FF6600; color: white; border: 1px solid #4F5C71; padding: 2px; position: absolute;
 *	height: 18px; visibility: hidden; filter: alpha(opacity=0.8); opacity: 0.8;
 * }
 * @param {HTMLElement} element The element to initialize.
 */
aeon.controls.Tooltip = function Tooltip(element)
{
	$assert.isNotNull(element);

	this.element = $(element);

	var title = this.element.attr("title");

	if (title != null && title != "")
		this.title = title.replace(/\n/g, "<br/>");

	this.element.removeAttr("title");
	this.className = this.element.attr("data-tooltipClass") || null;

	this.element.bind("mouseover", Function.createDelegate(this, this.onmouseover));
	this.element.bind("mousemove", Function.createDelegate(this, this.onmousemove));
	this.element.bind("mouseout", Function.createDelegate(this, this.onmouseout));
};

/**
 * Indicates that any element that has the title attribute set should cause the <c>Tooltip</c> layer to appear on mouseover.
 * @type {Boolean}
 */
aeon.controls.Tooltip.HoverAll = false;
/**
 * Specifies the maximum width for the popup layer
 * @type {Number}
 */
aeon.controls.Tooltip.MaxWidth = 300;
/**
 * Specifies the class name of the element for which the <c>Tooltip</c> layer should appear on mouseover.
 * @type {Number}
 */
aeon.controls.Tooltip.ClassName = "tooltip";
/**
 * Specifies the id of the <c>Tooltip</c> layer element.
 * @type {Number}
 */
aeon.controls.Tooltip.LayerID = "tooltip";

/**
 * Sets up the <c>Tooltip</c> control
 */
aeon.controls.Tooltip.setup = function Tooltip$setup()
{
	var tt = aeon.controls.Tooltip;
	tt.initializeElements();
	tt.initializeCoordinates();

	$(window).resize(aeon.controls.Tooltip.initializeCoordinates);
};

/**
 * Initializes the elements for which the <c>Tooltip</c>layer should appear on mouseover.
 */
aeon.controls.Tooltip.initializeElements = function ()
{
	var tt = aeon.controls.Tooltip;

	var elements = Array.fromArguments(arguments);
	if (elements.length == 0)
		elements = $("*");

	for (var i = 0; i < elements.length; i++)
	{
		var init = false;
		var elem = $(elements[i]);
		if (elem.hasClass(tt.ClassName))
			init = true;

		else if (elem.attr("title") && tt.HoverAll)
			init = true;

		if (init)
			tt.initializeSingleElement(elements[i]);
	}

	if (tt.__layer == null)
		tt.createPopupLayer();
};

/**
 * Initializes a single element for which the <c>Tooltip</c>layer should appear on mouseover.
 */
aeon.controls.Tooltip.initializeSingleElement = function Tooltip$initializeSingleElement(element)
{
	var uniqueID = $dom.uniqueID(element);

	var tt = aeon.controls.Tooltip;
	if (tt.__elements == null)
		tt.__elements = {};
	if (tt.__elements[uniqueID] == null)
		tt.__elements[uniqueID] = new aeon.controls.Tooltip(element);
};

/**
 * Initializes the window coordinates for proper calculation when the hover elements appear within an iframe.
 */
aeon.controls.Tooltip.initializeCoordinates = function Tooltip$initializeCoordinates()
{
	var tt = aeon.controls.Tooltip;
	tt.__windowTop = 0;
	tt.__windowLeft = 0;

	var currentWindow = window;
	if (window != window.parent)
	{
		while (currentWindow != top)
		{
			currentWindow = currentWindow.parent;
			var frames = $("iframe, frame", currentWindow.document);
			for (var i = 0; i < frames.length; i++)
			{
				if (frames[i].contentWindow == window)
				{
					tt.__windowLeft += frames.eq(i).offset().left;
					tt.__windowTop += frames.eq(i).offset().top;
					break;
				}
			}
		}
	}
};

/**
 * Creates the popup layer.
 */
aeon.controls.Tooltip.createPopupLayer = function Tooltip$createPopupLayer()
{
	var tt = aeon.controls.Tooltip;
	var hlayer = $("#" + tt.LayerID);
	if (hlayer.length != 0)
	{
		tt.__layer = hlayer;
	}
	else
	{
		tt.__layer = $('<div id="{0}"><div class="bg"></div><div class="text"></div></div>'.format(tt.LayerID));
		tt.__layer.bind("mouseover", tt.hide);

		$(top.document.body).append(tt.__layer);

		tt.__layer[0].style.cssText = aeon.controls.Tooltip.DEFAULT_CSS;
	}

	tt.__target = tt.__layer.find(".text");

	tt.__layerPaddingH =
		(parseInt(tt.__layer.css("paddingLeft")) || 0) +
		(parseInt(tt.__layer.css("paddingRight")) || 0);

	tt.__layerPaddingV =
		(parseInt(tt.__layer.css("paddingTop")) || 0) +
		(parseInt(tt.__layer.css("paddingBottom")) || 0);
};

/**
 * Shows the <c>Tooltip</c> layer for tooltip elements.
 * @param {DOMEvent} e The DOM event that occured.
 */
aeon.controls.Tooltip.prototype.onmouseover = function Tooltip$onmouseover(e)
{
	if (e.ctrlKey)
		return;

	var tt = aeon.controls.Tooltip;
	var maxWidth = tt.MaxWidth;

	var layer = tt.__layer;
	var target = tt.__target;

	target.text(String.EMPTY);
	layer.css({ visibility: "hidden" });

	if (this.className)
		layer.addClass(this.className);

	var title = this.title;
	if (title && title.trim().length != 0)
	{
		target.html(title);
		layer.css({ width: "auto", height: "auto" });

		if (layer.outerWidth() > maxWidth)
			layer.css({ width: maxWidth - tt.__layerPaddingH });
		else
			layer.css({ width: layer.outerWidth() - tt.__layerPaddingH });

		tt.active = true;
	}
	else
		tt.active = false;

	layer.css({ visibility: "visible" });
};

/**
 * Keeps the <c>Tooltip</c> layer moving close to the mouse as long as it is moving above a tooltip element.
 * @param {DOMEvent} e The DOM event that occured.
 */
aeon.controls.Tooltip.prototype.onmousemove = function Tooltip$onmousemove(e)
{
	if (e.ctrlKey || !aeon.controls.Tooltip.active)
		return;

	var tt = aeon.controls.Tooltip;
	var layer = tt.__layer;

	var body = $("body", top.document)[0];
	var html = $("html", top.document)[0];

	var bodyHeight = html.scrollHeight || body.scrollHeight;
	var bodyWidth = html.scrollWidth || body.scrollWidth;

	var scrollTop = html.scrollTop || body.scrollTop;
	var scrollLeft = html.scrollLeft || body.scrollLeft;

	var eventX = e.clientX + tt.__windowLeft;
	var eventY = e.clientY + tt.__windowTop;

	var targetLeft = (eventX + scrollLeft) + 10;
	var targetTop = (eventY + scrollTop) - layer.outerHeight() - 3;

	if ((targetTop + layer.outerHeight()) > bodyHeight - 5)
		targetTop -= layer.outerHeight();
	if (targetTop < 0)
		targetTop = 5;

	if ((targetLeft + layer.outerWidth()) > bodyWidth - 5)
		targetLeft -= (layer.outerWidth() + 30);
	if (targetLeft < 0)
		targetLeft = 5;

	layer.css({ visibility: "visible", left: targetLeft, top: targetTop });
};

/**
 * Hides the <c>Tooltip</c> layer.
 * @param {DOMEvent} e The DOM event that occured.
 */
aeon.controls.Tooltip.prototype.onmouseout = function Tooltip$onmouseout()
{
	if (this.className)
		aeon.controls.Tooltip.__layer.removeClass(this.className);

	aeon.controls.Tooltip.hide();
};

/**
 * Hides the <c>Tooltip</c> layer.
 * @param {DOMEvent} e The DOM event that occured.
 */
aeon.controls.Tooltip.hide = function Tooltip$hide()
{
	aeon.controls.Tooltip.__layer.css({ visibility: "hidden" });
};

aeon.controls.Tooltip.DEFAULT_CSS =
	"background-color: #000; color: white; padding: 5px; position: absolute; border-radius: 3px; font-size: 12px; " +
	"height: 18px; visibility: hidden; filter: alpha(opacity=0.8); opacity: 0.8; z-index: 500; " +
	"box-shadow: 6px 4px 5px rgba(0, 0, 0, 0.80); -moz-box-shadow: 6px 4px 5px rgba(0, 0, 0, 0.80); -webkit-box-shadow: 6px 4px 5px rgba(0, 0, 0, 0.80);";

$evt.addListener(window, "onready", aeon.controls.Tooltip.setup);
$evt.addListener(window, "onresize", aeon.controls.Tooltip.initializeCoordinates);
