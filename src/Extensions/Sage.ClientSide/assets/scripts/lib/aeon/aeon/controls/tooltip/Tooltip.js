Type.registerNamespace("aeon.controls");

aeon.controls.Tooltip = function Tooltip(element, settings)
{
	this.$super(element);

	var tooltip = aeon.controls.Tooltip;
	this.settings = new aeon.controls.TooltipSettings(settings, element);

	this.element.bind("mouseenter", Function.createDelegate(this, this.onMouseOver));
	this.element.bind("mouseleave", Function.createDelegate(this, this.onMouseOut));
	this.element.bind("focus", Function.createDelegate(this, this.onFocus));
	this.element.bind("blur", Function.createDelegate(this, this.onBlur));
	this.element.bind("click", Function.createDelegate(this, this.onClick));

	$(tooltip.document).bind("keydown click", Function.createDelegate(this, this.onDocumentEvent));

	this.showTooltip = Function.createDelegate(this, this.show);
	tooltip.popup.bind("click", Function.createDelegate(this, this.onPopupClick));
};
aeon.controls.Tooltip.inherits(aeon.controls.HtmlControl);

aeon.controls.Tooltip.POPUP_ELEMENT_ID = "tooltip";

/**
 * @internal
 */
aeon.controls.Tooltip.setup = function Tooltip$setup()
{
	$.fn.tooltip = aeon.controls.Tooltip.tooltip;

	aeon.controls.Tooltip.initializeCoordinates();
	aeon.controls.Tooltip.createPopupLayer();

	$(window).resize(aeon.controls.Tooltip.initializeCoordinates);
};

aeon.controls.Tooltip.tooltip = function Tooltip$tooltip(settings)
{
	return this.each(function Tooltip$tooltip$initializeInstance(i, element)
	{
		aeon.controls.Tooltip.initializeSingleElement(element, settings);
	});
};

/**
 * Initializes the window coordinates for proper calculation when the hover elements appear within an iframe.
 * @private
 */
aeon.controls.Tooltip.initializeCoordinates = function Tooltip$initializeCoordinates()
{
	var tooltip = aeon.controls.Tooltip
	tooltip.windowTop = 0;
	tooltip.windowLeft = 0;

	var domainRestricted = false;

	try
	{
		tooltip.document = top.document;
	}
	catch(e)
	{
		tooltip.document = document;
		domainRestricted = true;
	}

	if (domainRestricted)
		return;

	var currentWindow = window;
	if (window != window.parent)
	{
		while (currentWindow != top)
		{
			currentWindow = currentWindow.parent;
			var frames = $("iframe, frame", currentWindow.document);
			var foundFrame = false;

			for (var i = 0; i < frames.length; i++)
			{
				if (frames[i].contentWindow == window)
				{
					tooltip.windowLeft += frames.eq(i).offset().left;
					tooltip.windowTop += frames.eq(i).offset().top;
					foundFrame = true;
					break;
				}
			}
		}
	}
};

/**
 * Creates the popup layer.
 * @private
 */
aeon.controls.Tooltip.createPopupLayer = function Tooltip$createPopupLayer()
{
	var tooltip = aeon.controls.Tooltip;

	var hlayer = $(tooltip.document.getElementById(tooltip.POPUP_ELEMENT_ID));
	if (hlayer.length != 0)
	{
		tooltip.popup = hlayer;
	}
	else
	{
		var tooltipHtml = '<div id="{0}"><div class="arrow"></div><div class="bg"></div><div class="content"></div></div>'.format(tooltip.POPUP_ELEMENT_ID);
		tooltip.popup = $(tooltipHtml);

		$(tooltip.document.body).append(tooltip.popup);

		if (tooltip.document != document)
		{
			var styleElement = $("<style id='tooltipStyles'></style>");
			$(tooltip.document.body).append(styleElement);

			var stylesheet = styleElement[0];
			var styleText = [];

			var tooltipRules = $css.findRules("#tooltip");
			var index = 0;
			for (var selectorText in tooltipRules)
				styleText.push(tooltipRules[selectorText].cssText);


			styleElement.html(styleText.join("\n"));
		}
	}

	tooltip.target = tooltip.popup.find(".content");
	tooltip.arrow = tooltip.popup.find(".arrow");
};

/**
 * @private
 */
aeon.controls.Tooltip.prototype.hide = function Tooltip$hide()
{
	this.element.attr("title", this.element.attr("_title"));
	this.element.removeAttr("_title");

	var tooltip = aeon.controls.Tooltip;
	tooltip.current = null;

	var className = this.settings.className;
	if (this.settings.useFades)
	{
		if (tooltip.popup)
		{
			tooltip.popup.fadeOut(function ()
			{
				if (className)
					aeon.controls.Tooltip.popup.removeClass(className);
			});
		}
	}
	else
	{
		aeon.controls.Tooltip.popup.hide();
		if (className)
			aeon.controls.Tooltip.popup.removeClass(className);
	}
};

/**
 * @private
 */
aeon.controls.Tooltip.prototype.show = function Tooltip$show()
{
	if (this.element.hasClass("tooltip-suspend"))
		return;

	if (this.settings.obscureOnly)
	{
		var elem = this.element[0];
		if (elem.scrollWidth <= elem.offsetWidth && elem.scrollHeight <= elem.offsetHeight)
			return;
	}

	var tooltip = aeon.controls.Tooltip;
	var title = this.element.attr("title") || this.element.attr("data-title") || this.element.attr("_title");
	this.element.attr("_title", title);
	this.element.attr("title", String.EMPTY);

	var maxWidth = this.settings.maxWidth || $(tooltip.document).width() * .75;
	tooltip.popup.css("maxWidth", maxWidth);
	tooltip.target.html(title);

	var orientation = this.settings.orientation;
	var orientationP = orientation[0].match(/T|L|R|B/) ? orientation[0] : "T";
	var orientationS = orientation[1].match(/T|L|R|B|C/) ? orientation[1] : "L";

	var tooltip = aeon.controls.Tooltip;
	var html = $("html", tooltip.document)[0];
	var body = $("body", tooltip.document)[0];
	var bodyWidth = html.scrollWidth || body.scrollWidth;
	var bodyHeight = html.scrollHeight || body.scrollHeight;
	var scrollTop = html.scrollTop || body.scrollTop;
	var scrollLeft = html.scrollLeft || body.scrollLeft;

	tooltip.popup.css({ left: 0, top: -1000, display: "block" }).removeClass("t l r b");

	if (orientationP.match(/T|B/))
	{
		var primary = this.getPrimaryTop(orientationP, 0, bodyHeight);
		var secondary = this.getSecondaryLeft(orientationS, 0, bodyWidth);

		var specs = {
			popupLeft: secondary.left,
			popupTop: primary.top,
			arrowLeft: secondary.arrowLeft,
			arrowRight: secondary.arrowRight,
			arrowTop: primary.arrowTop,
			arrowBottom: primary.arrowBottom,
			orientation: primary.orientation
		};
	}
	else
	{
		var primary = this.getPrimaryLeft(orientationP, 0, bodyWidth);
		var secondary = this.getSecondaryTop(orientationS, 0, bodyHeight);

		var specs = {
			popupLeft: primary.left,
			popupTop: seondary.top,
			arrowLeft: primary.arrowLeft,
			arrowRight: primary.arrowRight,
			arrowTop: secondary.arrowTop,
			arrowBottom: secondary.arrowBottom,
			orientation: primary.orientation
		};
	}

	tooltip.popup.css({ left: specs.popupLeft, top: specs.popupTop });
	tooltip.arrow.css({ left: specs.arrowLeft, top: specs.arrowTop });
	tooltip.popup.addClass(specs.orientation.toLowerCase());
	tooltip.current = this;

	if (this.settings.useFades)
		tooltip.popup.fadeIn();
};

aeon.controls.Tooltip.prototype.getPrimaryLeft = function Tooltip$getPrimaryLeft(orientation, edgeL, edgeR)
{
	var result = { orientation: orientation, left: 0, arrowLeft: 0, arrowRight: 0 };

	var arrowWidth = 6;
	var tooltip = aeon.controls.Tooltip;
	var scrollLeft = tooltip.document.documentElement.scrollLeft;
	var elementLeft = tooltip.windowLeft + this.element.offset().left - scrollLeft;
	var elementWidth = this.element.width();
	var popupWidth = tooltip.popup.outerWidth() + arrowWidth;
	var offset = this.settings.offset;

	if (orientation == "L")
	{
		result.left = elementLeft - popupWidth - offset;
		result.arrowLeft = "auto";
		result.arrowRight = 0;
	}
	else
	{
		result.left = elementLeft + elementWidth + arrowWidth + offset;
		result.arrowLeft = 0;
		result.arrowRight = "auto";
	}

	if (result.left < edgeL)
	{
		result.left = elementLeft + elementWidth + offset;
		result.orientation = "R";
		result.arrowLeft = 0;
		result.arrowRight = "auto";
	}
	else if ((result.left + popupWidth) > edgeR)
	{
		result.left = elementLeft - popupWidth - offset;
		result.orientation = "L";
		result.arrowLeft = "auto";
		result.arrowRight = 0;
	}

	return result;
};

aeon.controls.Tooltip.prototype.getSecondaryLeft = function Tooltip$getSecondaryLeft(orientation, edgeL, edgeR)
{
	var result = { left: 0, arrowLeft: 0, arrowRight: 0 };

	var tooltip = aeon.controls.Tooltip;
	var scrollLeft = tooltip.document.documentElement.scrollLeft;
	var elementLeft = tooltip.windowLeft + this.element.offset().left - scrollLeft;
	var elementWidth = this.element.width();
	var popupWidth = tooltip.popup.outerWidth();
	var arrowWidth = 12;

	if (orientation == "L")
	{
		result.left = elementLeft;
		result.arrowLeft = arrowWidth;
	}
	else if (orientation == "R")
	{
		result.left = (elementLeft + elementWidth) - popupWidth;
		result.arrowLeft = popupWidth - (arrowWidth * 2);
	}
	else
	{
		result.left = elementLeft + ((elementWidth / 2) - popupWidth / 2);
		result.arrowLeft = (popupWidth / 2) - (arrowWidth / 2);
	}

	if (result.left < edgeL)
	{
		result.left = elementLeft;
		result.arrowLeft = arrowWidth;
		orientation = "L";
		if ((result.left + popupWidth) > edgeR)
		{
			var difference = (result.left + popupWidth) - edgeR;
			result.left -= difference;
			result.arrowLeft += difference;
		}
	}
	else if ((result.left + popupWidth) > edgeR)
	{
		result.left = (elementLeft + elementWidth) - popupWidth;
		result.arrowLeft = popupWidth - (arrowWidth * 2);
		orientation = "R";
		if (result.left < edgeL)
		{
			var difference = edgeL - result.left;
			result.left += difference;
			result.arrowLeft -= difference;
		}
	}

	if (elementWidth < (arrowWidth * 3))
	{
		var difference = (arrowWidth * 1.5) - elementWidth / 2;
		if (orientation == "L")
			result.left -= difference;
		else
			result.left += difference;
	}

	return result;
};

aeon.controls.Tooltip.prototype.getPrimaryTop = function Tooltip$getPrimaryTop(orientation, edgeT, edgeB)
{
	var result = { orientation: orientation, top: 0, arrowTop: 0, arrowBottom: 0 };

	var arrowHeight = 6;
	var tooltip = aeon.controls.Tooltip;
	var scrollTop = tooltip.document.documentElement.scrollTop;
	var elementTop = tooltip.windowTop + this.element.offset().top - scrollTop;
	var elementHeight = this.element.height();
	var popupHeight = tooltip.popup.outerHeight() + arrowHeight;
	var offset = this.settings.offset;

	if (orientation == "T")
	{
		result.top = elementTop - popupHeight - offset;
		result.arrowTop = "auto";
		result.arrowBottom = 0;
	}
	else
	{
		result.top = elementTop + elementHeight + arrowHeight + offset;
		result.arrowTop = 0;
		result.arrowBottom = "auto";
	}

	if (result.top < edgeT)
	{
		var updatedTop = elementTop + elementHeight + offset;
		var correctionTop = result.top - updatedTop;
		result.top = updatedTop;
		result.orientation = "B";
		result.arrowTop = 0;
		result.arrowBottom = "auto";
	}
	else if ((result.top + popupHeight) > edgeB)
	{
		var updatedTop = elementTop - popupHeight - offset;
		var correctionTop = result.top - updatedTop;
		result.top = updatedTop;
		result.orientation = "T";
		result.arrowTop = "auto";
		result.arrowBottom = 0;
	}

	return result;
};

aeon.controls.Tooltip.prototype.getSecondaryTop = function Tooltip$getSecondaryTop(orientation, edgeT, edgeB)
{
	var result = { top: 0, arrowTop: 0, arrowBottom: 0 };

	var arrowHeight = 12;
	var tooltip = aeon.controls.Tooltip;
	var scrollTop = tooltip.document.documentElement.scrollTop;
	var elementTop = tooltip.windowTop + this.element.offset().top - scrollTop;
	var elementHeight = this.element.height() + arrowHeight;
	var popupHeight = tooltip.popup.outerHeight();

	if (orientation == "B")
	{
		result.top = elementTop;
		result.arrowTop = arrowHeight;
	}
	else if (orientation == "T")
	{
		result.top = (elementTop + elementHeight) - popupHeight;
		result.arrowTop = popupHeight - (arrowHeight * 2);
	}
	else
	{
		result.top = elementTop + ((elementHeight / 2) - popupHeight / 2);
		result.arrowTop = (popupHeight / 2) - (arrowHeight / 2);
	}

	if (result.top < edgeT)
	{
		result.top = elementTop;
		result.arrowTop = arrowHeight;
		orientation = "T";
		if ((result.top + popupHeight) > edgeB)
		{
			var difference = (result.top + popupHeight) - edgeB;
			result.top -= difference;
			result.arrowTop += difference;
		}
	}
	else if ((result.top + popupHeight) > edgeB)
	{
		result.top = (elementTop + elementHeight) - popupHeight;
		result.arrowTop = popupHeight - (arrowHeight * 2);
		orientation = "B";
		if (result.top < edgeT)
		{
			var difference = edgeT - result.top;
			result.top += difference;
			result.arrowTop -= difference;
		}
	}

	if (elementHeight < (arrowHeight * 2))
	{
		if (orientation == "T")
			result.top -= (arrowHeight * .8);
		else
			result.top += (arrowHeight * .8);
	}

	return result;
};

aeon.controls.Tooltip.prototype.setSettingsValue = function Tooltip$setSettingsValue(name, value)
{
	if (value && (name == "showOn" || name == "hideOn"))
		value = value.split(/(?:\s*,\s*)|(?:\s+)/);

	this.settings[name] = value;
};

/**
 * @private
 */
aeon.controls.Tooltip.prototype.onMouseOver = function Tooltip$onMouseOver(e)
{
	window.clearTimeout(this.delayId);

	var tooltip = aeon.controls.Tooltip;
	if (tooltip.current && tooltip.current != this && !tooltip.current.settings.hideOn.contains("mouseout"))
		return;

	if (!this.settings.showOn.contains("mouseover"))
		return;

	if (this.element.hasClass("tooltip-suspend"))
		return;

	this.delayId = window.setTimeout(this.showTooltip, this.settings.delay);
};

/**
 * @private
 */
aeon.controls.Tooltip.prototype.onMouseOut = function Tooltip$onMouseOut()
{
	window.clearTimeout(this.delayId);
	if (aeon.controls.Tooltip.current != this)
		return;

	if (!this.settings.hideOn.contains("mouseout"))
		return;

	this.hide();
};

aeon.controls.Tooltip.prototype.onClick = function Tooltip$onClick()
{
	var tooltip = aeon.controls.Tooltip;

	if (tooltip.popup.is(":visible") && this.settings.hideOn.contains("click"))
		this.hide();

	else if (!tooltip.popup.is(":visible") && this.settings.showOn.contains("click"))
		this.show();
};

aeon.controls.Tooltip.prototype.onPopupClick = function Tooltip$onPopupClick()
{
	if (aeon.controls.Tooltip.current == this && this.settings.hideOn.contains("click"))
		this.hide();
};

aeon.controls.Tooltip.prototype.onDocumentEvent = function Tooltip$onDocumentEvent(e)
{
	var tooltip = aeon.controls.Tooltip;
	if (!tooltip.popup.is(":visible") || tooltip.current != this)
		return;

	if (!jQuery.contains(this.element[0], e.target) && !jQuery.contains(tooltip.popup[0], e.target))
		this.hide();
};

aeon.controls.Tooltip.prototype.onFocus = function Tooltip$onFocus()
{
	if (this.settings.showOn.contains("focus"))
		this.show();
};

aeon.controls.Tooltip.prototype.onBlur = function Tooltip$onBlur()
{
	if (this.settings.hideOn.contains("blur"))
		this.hide();
};

aeon.controls.Tooltip.prototype.toString = function Tooltip$toString()
{
	return "Tooltip";
};

aeon.controls.ControlRegistry.registerControl(aeon.controls.Tooltip, ".tooltip");

/**
 * Defines the settings of the <c>Tooltip</c> control.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override The object with overriding settings. If a setting exist both in <c>data</c> and in <c>override</c>
 * objects, the setting from <c>override</c> takes precedence.
 */
aeon.controls.TooltipSettings = function TooltipSettings(data, override)
{
	this.$super(data, override);

	this.className = this.getString("className", data, override, null);
	this.followMouse = this.getBoolean("followMouse", data, override, true);
	this.maxWidth = this.getString("maxWidth", data, override, null);
	this.showOn = this.getString("showOn", data, override, "mouseover").split(/(?:\s*,\s*)|(?:\s+)/);
	this.hideOn = this.getString("hideOn", data, override, "mouseout").split(/(?:\s*,\s*)|(?:\s+)/);
	this.orientation = this.getString("orientation", data, override, "TL").toUpperCase();
	this.offset = this.getNumber("offset", data, override, 3);
	this.delay = this.getNumber("delay", data, override, 200);
	this.obscureOnly = this.getBoolean("obscureOnly", data, override, false);
	this.useFades = this.getBoolean("useFades", data, override, false);
}
aeon.controls.TooltipSettings.inherits(aeon.Settings);
