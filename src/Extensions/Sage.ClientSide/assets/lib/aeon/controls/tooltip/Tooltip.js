/**
 * Implements the tooltip control and all of its public and private code.
 */
aeon.controls.Tooltip = aeon.controls.register(new function Tooltip()
{
	var POPUP_ELEMENT_ID = "tooltip_popup";

	var tooltip = this;
	var tooltipInstance = null;
	var tooltipPopup = null;
	var tooltipArrow = null;
	var tooltipDocument = null;
	var windowTop = 0;
	var windowLeft = 0;
	var timeoutId;

	/**
	 * This control doesn't need asynchronous initialization.
	 * @type {Boolean}
	 */
	this.async = false;

	/**
	 * The css expression of elements that should be initialized as tooltips.
	 * @type {String}
	 */
	this.expression = ".tooltip";

	/**
	 * Prepares the tooltip control
	 */
	this.setup = function setup()
	{
		if (tooltipPopup != null)
			return;

		initializeCoordinates();
		createPopupLayer();

		$.fn.tooltip = tooltip;
		$(window).resize(initializeCoordinates);
	};

	/**
	 * Defines the settings of the <c>Tooltip</c> control.
	 * @constructor
	 * @param {Object} data The object with initial settings.
	 * @param {Object} element The object with overriding settings. If a setting exist both in
	 * <c>data</c> and in <c>override</c> objects, the setting from <c>override</c> takes precedence.
	 */
	this.Settings = aeon.Settings.extend(function Settings(data, override)
	{
		this.construct(data, override);

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
	});

	/**
	 * Defines the <c>Tooltip</c> control.
	 * @constructor
	 * @param {HTMLElement} element The HTML element that shows the tooltip popup.
	 * @param {Settings} settings Optiona settings to use with this instannce.
	 */
	this.Control = aeon.HtmlControl.extend(function Tooltip(element, settings)
	{
		this.construct(element);

		this.settings = new tooltip.Settings(element, settings);
		this.$element
			.bind("mouseenter", $.proxy(tooltipOnMouseOver, this))
			.bind("mouseleave", $.proxy(tooltipOnMouseOut, this))
			.bind("focus", $.proxy(tooltipOnFocus, this))
			.bind("blur", $.proxy(tooltipOnBlur, this))
			.bind("click", $.proxy(tooltipOnClick, this));

		$(tooltipDocument).bind("keydown click", $.proxy(tooltipOnDocumentEvent, this));

		this.showTooltip = $.proxy(this.show, this);
		tooltipPopup.bind("click", $.proxy(tooltipOnClick, this));
		tooltip.registerInstance(this);
	});

	/**
	 * Hides the tooltip
	 */
	this.Control.prototype.hide = function Tooltip$hide()
	{
		this.$element.attr("title", this.$element.attr("_title"));
		this.$element.removeAttr("_title");

		var className = this.settings.className;
		if (this.settings.useFades)
		{
			if (tooltipPopup)
			{
				tooltipPopup.fadeOut(function oncomplete()
				{
					if (className)
						tooltipPopup.removeClass(className);
				});
			}
		}
		else
		{
			tooltipPopup.hide();
			if (className)
				tooltipPopup.removeClass(className);
		}

		tooltipInstance = null;
	};

	/**
	 * Shows the tooltip
	 */
	this.Control.prototype.show = function Tooltip$show()
	{
		if (this.$element.hasClass("tooltip-suspend"))
			return;

		if (this.settings.obscureOnly)
		{
			var elem = this.$element[0];
			if (elem.scrollWidth <= elem.offsetWidth && elem.scrollHeight <= elem.offsetHeight)
				return;
		}

		var title = this.$element.attr("title") || this.$element.attr("data-title") || this.$element.attr("_title");
		if (!title)
			return;

		this.$element.attr("_title", title);
		this.$element.attr("title", aeon.string.EMPTY);

		var maxWidth = this.settings.maxWidth || $(tooltipDocument).width() * .75;
		tooltipPopup.css("maxWidth", maxWidth);
		tooltipContent.html(title);

		var orientation = this.settings.orientation;
		var orientationP = orientation[0].match(/T|L|R|B/) ? orientation[0] : "T";
		var orientationS = orientation[1].match(/T|L|R|B|C/) ? orientation[1] : "L";

		var html = $("html", tooltipDocument)[0];
		var body = $("body", tooltipDocument)[0];
		var bodyWidth = html.scrollWidth || body.scrollWidth;
		var bodyHeight = html.scrollHeight || body.scrollHeight;
		var scrollTop = html.scrollTop || body.scrollTop;
		var scrollLeft = html.scrollLeft || body.scrollLeft;

		tooltipPopup.css({ left: 0, top: -1000, display: "block" }).removeClass("t l r b");

		var offset = this.settings.offset;
		var elementTop = this.$element.offset().top;
		var elementLeft = this.$element.offset().left;
		var elementHeight = this.$element.height();
		var elementWidth = this.$element.width();

		if (orientationP.match(/T|B/))
		{
			var primary = getPrimaryTop(elementTop, elementHeight, orientationP, 0, bodyHeight, offset);
			var secondary = getSecondaryLeft(elementLeft, elementWidth, orientationS, 0, bodyWidth);

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
			var primary = getPrimaryLeft(elementLeft, elementWidth, orientationP, 0, bodyWidth, offset);
			var secondary = getSecondaryTop(elementTop, elementHeight, orientationS, 0, bodyHeight);

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

		tooltipPopup.css({ left: specs.popupLeft, top: specs.popupTop });
		tooltipArrow.css({ left: specs.arrowLeft, top: specs.arrowTop });
		tooltipPopup.addClass(specs.orientation.toLowerCase());
		tooltipInstance = this;

		if (this.settings.useFades)
			tooltipPopup.fadeIn();
	};

	/**
	 * Sets on this instance's <c>Settings</c> values.
	 * @param {String} name The name of the setting to set.
	 * @param {Object} value The value of the setting to set.
	 * @returns {Object} The current value of the setting with the specified <c>name</c>.
	 */
	this.Control.prototype.set = function Tooltip$set(name, value)
	{
		if (value && (name == "showOn" || name == "hideOn"))
			value = value.split(/(?:\s*,\s*)|(?:\s+)/);

		if (this.settings[name] != undefined)
			this.settings[name] = value;

		return this.settings[name];
	};

	function getPrimaryLeft(elementLeft, elementWidth, orientation, edgeL, edgeR, offset)
	{
		var result = { orientation: orientation, left: 0, arrowLeft: 0, arrowRight: 0 };

		var arrowWidth = 6;
		var scrollLeft = tooltipDocument.documentElement.scrollLeft;
		var absoluteLeft = windowLeft + elementLeft - scrollLeft;
		var popupWidth = tooltipPopup.outerWidth() + arrowWidth;

		if (orientation == "L")
		{
			result.left = absoluteLeft - popupWidth - offset;
			result.arrowLeft = "auto";
			result.arrowRight = 0;
		}
		else
		{
			result.left = absoluteLeft + elementWidth + arrowWidth + offset;
			result.arrowLeft = 0;
			result.arrowRight = "auto";
		}

		if (result.left < edgeL)
		{
			result.left = absoluteLeft + elementWidth + offset;
			result.orientation = "R";
			result.arrowLeft = 0;
			result.arrowRight = "auto";
		}
		else if ((result.left + popupWidth) > edgeR)
		{
			result.left = absoluteLeft - popupWidth - offset;
			result.orientation = "L";
			result.arrowLeft = "auto";
			result.arrowRight = 0;
		}

		return result;
	};

	function getSecondaryLeft(elementLeft, elementWidth, orientation, edgeL, edgeR)
	{
		var result = { left: 0, arrowLeft: 0, arrowRight: 0 };

		var scrollLeft = tooltipDocument.documentElement.scrollLeft;
		var absoluteLeft = windowLeft + elementLeft - scrollLeft;
		var popupWidth = tooltipPopup.outerWidth();
		var arrowWidth = 12;

		if (orientation == "L")
		{
			result.left = absoluteLeft;
			result.arrowLeft = arrowWidth;
		}
		else if (orientation == "R")
		{
			result.left = (absoluteLeft + elementWidth) - popupWidth;
			result.arrowLeft = popupWidth - (arrowWidth * 2);
		}
		else
		{
			result.left = absoluteLeft + ((elementWidth / 2) - popupWidth / 2);
			result.arrowLeft = (popupWidth / 2) - (arrowWidth / 2);
		}

		if (result.left < edgeL)
		{
			result.left = absoluteLeft;
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
			result.left = (absoluteLeft + elementWidth) - popupWidth;
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

	function getPrimaryTop(elementTop, elementHeight, orientation, edgeT, edgeB, offset)
	{
		var result = { orientation: orientation, top: 0, arrowTop: 0, arrowBottom: 0 };

		var arrowHeight = 6;
		var scrollTop = tooltipDocument.documentElement.scrollTop;
		var absoluteTop = windowTop + elementTop - scrollTop;
		var popupHeight = tooltipPopup.outerHeight() + arrowHeight;

		if (orientation == "T")
		{
			result.top = absoluteTop - popupHeight - offset;
			result.arrowTop = "auto";
			result.arrowBottom = 0;
		}
		else
		{
			result.top = absoluteTop + elementHeight + arrowHeight + offset;
			result.arrowTop = 0;
			result.arrowBottom = "auto";
		}

		if (result.top < edgeT)
		{
			var updatedTop = absoluteTop + elementHeight + offset;
			var correctionTop = result.top - updatedTop;
			result.top = updatedTop;
			result.orientation = "B";
			result.arrowTop = 0;
			result.arrowBottom = "auto";
		}
		else if ((result.top + popupHeight) > edgeB)
		{
			var updatedTop = absoluteTop - popupHeight - offset;
			var correctionTop = result.top - updatedTop;
			result.top = updatedTop;
			result.orientation = "T";
			result.arrowTop = "auto";
			result.arrowBottom = 0;
		}

		return result;
	};

	function getSecondaryTop(elementTop, elementHeight, orientation, edgeT, edgeB)
	{
		var result = { top: 0, arrowTop: 0, arrowBottom: 0 };

		var arrowHeight = 12;
		var scrollTop = tooltipDocument.documentElement.scrollTop;
		var absoluteTop = windowTop + elementTop - scrollTop;
		var popupHeight = tooltipPopup.outerHeight();

		if (orientation == "B")
		{
			result.top = absoluteTop;
			result.arrowTop = arrowHeight;
		}
		else if (orientation == "T")
		{
			result.top = (absoluteTop + elementHeight) - popupHeight;
			result.arrowTop = popupHeight - (arrowHeight * 2);
		}
		else
		{
			result.top = absoluteTop + ((elementHeight / 2) - popupHeight / 2);
			result.arrowTop = (popupHeight / 2) - (arrowHeight / 2);
		}

		if (result.top < edgeT)
		{
			result.top = absoluteTop;
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
			result.top = (absoluteTop + elementHeight) - popupHeight;
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

	function tooltipOnMouseOver(e)
	{
		window.clearTimeout(timeoutId);

		if (tooltipInstance && tooltipInstance != this && !tooltipInstance.settings.hideOn.contains("mouseout"))
			return;

		if (!this.settings.showOn.contains("mouseover"))
			return;

		if (this.$element.hasClass("tooltip-suspend"))
			return;

		if (this.settings.delay > 0)
			timeoutId = window.setTimeout(this.showTooltip, this.settings.delay);
		else
			this.showTooltip();
	};

	function tooltipOnMouseOut()
	{
		window.clearTimeout(timeoutId);

		if (tooltipInstance != this)
			return;

		if (!this.settings.hideOn.contains("mouseout"))
			return;

		this.hide();
	};

	function tooltipOnFocus()
	{
		if (this.settings.showOn.contains("focus"))
			this.show();
	};

	function tooltipOnBlur()
	{
		if (this.settings.hideOn.contains("blur"))
			this.hide();
	};

	function tooltipOnClick()
	{
		if (tooltipPopup.is(":visible") && this.settings.hideOn.contains("click"))
			this.hide();

		else if (!tooltipPopup.is(":visible") && this.settings.showOn.contains("click"))
			this.show();
	};

	function tooltipOnPopupClick()
	{
		if (tooltipInstance == this && this.settings.hideOn.contains("click"))
			this.hide();
	};

	function tooltipOnDocumentEvent(e)
	{
		if (!tooltipPopup.is(":visible") || tooltipInstance != this)
			return;

		if (!jQuery.contains(this.$element[0], e.target) && !jQuery.contains(tooltipPopup[0], e.target))
			this.hide();
	};

	function tooltip(settings)
	{
		return this.each(function initializeInstance(i, element)
		{
			initializeSingleElement(element, settings);
		});
	};

	function initializeCoordinates()
	{
		windowTop = 0;
		windowLeft = 0;

		var domainRestricted = false;
		try
		{
			tooltipDocument = top.document;
		}
		catch(e)
		{
			tooltipDocument = document;
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
						windowLeft += frames.eq(i).offset().left;
						windowTop += frames.eq(i).offset().top;
						foundFrame = true;
						break;
					}
				}
			}
		}
	};

	function createPopupLayer()
	{
		var hlayer = $(tooltipDocument.getElementById(POPUP_ELEMENT_ID));
		if (hlayer.length != 0)
		{
			tooltipPopup = hlayer;
		}
		else
		{
			var tooltipHtml = aeon.string.format(
				'<div id="{0}">' +
					'<div class="arrow"></div>' +
					'<div class="bg"></div>' +
					'<div class="content"></div>' +
				'</div>', POPUP_ELEMENT_ID);

			tooltipPopup = $(tooltipHtml);
			$(tooltipDocument.body).append(tooltipPopup);

			if (tooltipDocument != document)
			{
				var styleElement = $("<style id='tooltipStyles'></style>");
				$(tooltipDocument.body).append(styleElement);

				var stylesheet = styleElement[0];
				var styleText = [];

				var tooltipRules = $css.findRules("#tooltip");
				var index = 0;
				for (var selectorText in tooltipRules)
					styleText.push(tooltipRules[selectorText].cssText);

				styleElement.html(styleText.join("\n"));
			}
		}

		tooltipContent = tooltipPopup.find(".content");
		tooltipArrow = tooltipPopup.find(".arrow");
	};
});
