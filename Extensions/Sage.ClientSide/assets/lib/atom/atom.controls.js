/**
 * Implements a simple scroller control.
 */
atom.controls.Scroller = atom.controls.register(new function Scroller()
{
	var scroller = this;
	var scrollers = [];
	var sizing = { FIXED: 0, PROPORTIONAL: 1 };
	var lineHeight = 20;
	var scrollbarSize = null;

	var Orientation = { VERTICAL: 1, HORIZONTAL: 2 };
	var PROPS =
	{
		"1": {
			scrollSize: "scrollHeight",
			scrollPos: "scrollTop",
			offsetPos: "offsetY",
			size: "height",
			outerSize: "outerHeight",
			position: "top",
			scrollPadding: "paddingRight",
			scrollPaddingSize: "width"
		},
		"2": {
			scrollSize: "scrollWidth",
			scrollPos: "scrollLeft",
			offsetPos: "offsetX",
			size: "width",
			outerSize: "outerWidth",
			position: "left",
			scrollPadding: "paddingBottom",
			scrollPaddingSize: "height"
		}
	};

	var className = {
		HOVER: "hover",
		HOT: "hot",
		ENGAGED: "engaged"
	};

	this.NAME = "atom.controls.Scroller";

	/**
	 * This control doesn't need asynchronous initialization.
	 * @type {Boolean}
	 */
	this.async = false;

	/**
	 * The css expression of elements that should be initialized as scrollers.
	 * @type {String}
	 */
	this.expression = ".scrollable";

	/**
	 * Prepares the scroller control
	 */
	this.setup = function setup()
	{
		// retrieve the line height (in pixels)
		lineHeight = parseInt($("body").css("line-height")) || 20;

		$(window).on("resize", this.redraw);
	};

	this.redraw = function redraw()
	{
		for (var i = 0; i < scroller.instances.length; i++)
			scroller.instances[i].redraw();
	};

	/**
	 * Defines the settings of the <c>Tooltip</c> control.
	 * @extends {atom.Settings}
	 * @param {Object} data The object with initial settings.
	 * @param {Object} element The object with overriding settings. If a setting exist both in
	 * <c>data</c> and in <c>override</c> objects, the setting from <c>override</c> takes precedence.
	 */
	this.Settings = atom.Settings.extend(function ScrollerSettings(element, settings)
	{
		var sizingValue = this.getString("sizing", element, settings, "").toLowerCase();
		this.sizing = sizingValue == "fixed" ? sizing.FIXED : sizing.PROPORTIONAL;
		this.animate = this.getBoolean("animate", element, settings, true);
	});

	/**
	 * Implements a custom scroller control.
	 * @extends {atom.HtmlControl}
	 * @event scroll
	 * @event dragstart
	 * @event dragend
	 */
	this.Control = function ScrollerControl(element, settings)
	{
		this.construct(element, "scroll", "dragstart", "dragend");

		this.settings = new scroller.Settings(element, settings);
		this.mouseInRange = false;

		if (scrollbarSize == null)
			scrollbarSize = measureScrollbarSize();

		// if the target element already has the markup that looks like what we
		// need, assume that the element has already been setup and proceed without
		// creating the wrapping elements. it is up to the author of element's html
		// to ensure that the markup is setup correctly.
		if (this.$element.find("> .atom-scroller").length == 0)
		{
			this.$element.html(atom.string.format(
				"<div class='atom-scroller'><div class='atom-scrollcontent'>{0}</div></div>"+
				"<div class='atom-scrolltrack vertical'><div class='atom-scrollgrip'/></div>" +
				"<div class='atom-scrolltrack horizontal'><div class='atom-scrollgrip'/></div>",
					this.$element.html()));

			atom.controls.update(this.$element);
		}

		this.$scroller = this.$element.find("> .atom-scroller");
		this.$content = this.$scroller.find("> .atom-scrollcontent");

		this.$trackVertical = this.$element.find("> .atom-scrolltrack.vertical");
		this.$gripVertical = this.$trackVertical.find("> .atom-scrollgrip");
		this.$trackHorizontal = this.$element.find("> .atom-scrolltrack.horizontal");
		this.$gripHorizontal = this.$trackHorizontal.find("> .atom-scrollgrip");
		this.$content.css("padding", this.$element.css("padding"));
		this.$content.data("_paddingRight", parseInt(this.$content.css("paddingRight")) || 0);
		this.$content.data("_paddingBottom", parseInt(this.$content.css("paddingBottom")) || 0);

		this.$scroller.css({
			right: -scrollbarSize.width,
			bottom: -scrollbarSize.height
		});

		this.scrollRatio = {};

		this.$scroller.on("scroll", $.proxy(onScroll, this));

		this.onDragMove = $.proxy(onDragMove, this);
		this.onDragEnd = $.proxy(onDragEnd, this);

		this.offsetVertical = parseInt(this.$gripVertical.css("top")) || 0;
		this.offsetHorizontal = parseInt(this.$gripHorizontal.css("left")) || 0;

		this.scrollOrientation = null;
		this.enabled = true;
		this.overflow = this.$scroller.css("overflow");

		this.switches =
		{
			HOVER: { id: 0, duration: 2000 },
			ENGAGED: { id: 0, duration: 500 },
			HOT: { id: 0, duration: 1500 }
		};

		if (!atom.dom.isTouchDevice())
		{
			$(document).on("mousemove", $.proxy(onDocumentMouseMove, this));
		}

		this.$gripVertical
			.data("orientation", Orientation.VERTICAL)
			.on("mousedown", $.proxy(onGripMouseDown, this))
			.on("click", atom.event.cancel);
		this.$gripHorizontal
			.data("orientation", Orientation.HORIZONTAL)
			.on("mousedown", $.proxy(onGripMouseDown, this))
			.on("click", atom.event.cancel);

		this.$trackVertical
			.on("mouseenter", $.proxy(onScrollTrackMouseOver, this))
			.on("mouseleave", $.proxy(onScrollTrackMouseOut, this))
			.on("click", $.proxy(onScrollTrackClick, this));

		this.$trackHorizontal
			.on("mouseenter", $.proxy(onScrollTrackMouseOver, this))
			.on("mouseleave", $.proxy(onScrollTrackMouseOut, this))
			.on("click", $.proxy(onScrollTrackClick, this));

		this.$scroller
			.on("mousewheel DOMMouseScroll", $.proxy(onMouseWheel, this));

		scroller.registerInstance(this);
		this.redraw();
	};

	this.Control.inherits(atom.HtmlControl);

	this.Control.prototype.enable = function scroller$enable()
	{
		this.enabled = true;
		this.redraw();
		if (this.isScrollable())
		{
			this.$scroller.css("overflow", atom.string.EMPTY);
			this.$trackVertical.show();
		}
	};

	this.Control.prototype.disable = function scroller$disable()
	{
		this.enabled = false;
		this.$trackVertical.hide();
		this.$scroller.css("overflow", "hidden");
	};

	this.Control.prototype.isScrollable = function scroller$isScrollable()
	{
		return this.isScrollableVertically() || this.isScrollableHorizontally();
	};

	this.Control.prototype.isScrollableVertically = function scroller$isScrollableVertically()
	{
		var totalLength = this.$scroller.prop("scrollHeight");
		var availableLength = this.$scroller["outerHeight"]();

		return totalLength > availableLength;
	};

	this.Control.prototype.isScrollableHorizontally = function scroller$isScrollableHorizontally()
	{
		var totalLength = this.$scroller.prop("scrollWidth");
		var availableLength = this.$scroller["outerWidth"]();

		return totalLength > availableLength;
	};

	this.Control.prototype.scrollLeft = function scroller$scrollLeft(value)
	{
		if (!isNaN(value) && value >= 0)
			this.$scroller.prop("scrollLeft", value);

		return this.$scroller.prop("scrollLeft");
	};

	this.Control.prototype.scrollTop = function scroller$scrollTop(value)
	{
		if (!isNaN(value) && value >= 0)
			this.$scroller.prop("scrollTop", value);

		return this.$scroller.prop("scrollTop");
	};

	this.Control.prototype.scrollHeight = function scroller$scrollHeight(value)
	{
		if (!isNaN(value) && value > 0)
		{
			this.$content.css("height", value);
			this.redraw();
		}

		return this.$content.prop("scrollHeight");
	};

	this.Control.prototype.scrollWidth = function scroller$scrollWidth(value)
	{
		if (!isNaN(value) && value > 0)
		{
			this.$content.css("width", value);
			this.redraw();
		}

		return this.$content.prop("scrollWidth");
	};

	this.Control.prototype.scrollMaxLeft = function scroller$scrollMaxLeft()
	{
		return this.scrollWidth() - this.$content.width();
	};

	this.Control.prototype.scrollMaxTop = function scroller$scrollMaxTop()
	{
		return this.scrollHeight() - this.$content.height();
	};

	this.Control.prototype.scrollLeft = function scroller$scrollLeft(value)
	{
		if (this.enabled && !isNaN(value))
			this.$scroller.prop("scrollLeft", value);

		return this.$scroller.prop("scrollLeft");
	};

	this.Control.prototype.scrollTop = function scroller$scrollTop(value)
	{
		if (this.enabled && !isNaN(value))
			this.$scroller.prop("scrollTop", value);

		return this.$scroller.prop("scrollTop");
	};

	this.Control.prototype.redraw = function scroller$redraw()
	{
		redrawAxis.call(this, Orientation.VERTICAL);
		redrawAxis.call(this, Orientation.HORIZONTAL);

		updateClasses.call(this);
	};

	function updateClasses()
	{
		if (this.enabled)
		{
			this.$element.toggleClass("atom-hscrollable", this.isScrollableHorizontally());
			this.$element.toggleClass("atom-vscrollable", this.isScrollableVertically());
		}
		else
		{
			this.$element.removeClass("atom-hscrollable atom-vscrollable");
		}
	}

	function toggleScrolling()
	{
		this.$trackVertical.toggle();
	}

	function redrawAxis(side)
	{
		var props = PROPS[side];
		var $track = side == Orientation.HORIZONTAL ? this.$trackHorizontal : this.$trackVertical;
		var $grip = side == Orientation.HORIZONTAL ? this.$gripHorizontal : this.$gripVertical;

		var gripOffset1, gripOffset2;
		if (side == Orientation.HORIZONTAL)
		{
			gripOffset1 = parseInt(this.$gripVertical.css("top")) || 0;
			gripOffset2 = parseInt(this.$gripVertical.css("bottom")) || 0;
		}
		else
		{
			gripOffset1 = parseInt(this.$gripVertical.css("left")) || 0;
			gripOffset2 = parseInt(this.$gripVertical.css("right")) || 0;
		}

		var totalLength = this[props.scrollSize]();
		var availableLength = this.$element[props.size]();

		var trackSize = $track[props.size]();
		var gripSize = $grip[props.size]();

		if (totalLength > availableLength)
		{
			var trackPadSize = parseInt($track.css(props.scrollPaddingSize));
			this.$content.css(props.scrollPadding, this.$content.data("_" + props.scrollPadding) + trackPadSize);

			totalLength = this[props.scrollSize]();
			var scrollAreaLength = trackSize - (gripOffset1 + gripOffset2);
			this.scrollRatio[side] = scrollAreaLength / totalLength;

			gripSize = Math.round(availableLength * this.scrollRatio[side]);
			$grip.css(props.size, gripSize);

			$grip.data("minScroll", gripOffset1);
			$grip.data("maxScroll", trackSize - gripSize - gripOffset2);

		}
		else
		{
			this.$content.css(props.scrollPadding, this.$content.data("_" + props.scrollPadding));
		}
	}

	function scrollContentFromGripDrag()
	{
		var $grip = this.$scrollGrip;
		var orientation = $grip.data("orientation");
		var props = PROPS[orientation];
		var gripPos = $grip.position()[props.position];
		var targetPos = Math.round(gripPos / this.scrollRatio[orientation]);
		this.$scroller.prop(props.scrollPos, targetPos);
	}

	function positionGripsFromScrolling()
	{
		var scrollTop = this.$scroller.prop("scrollTop");
		var scrollLeft = this.$scroller.prop("scrollLeft");
		var targetTop = Math.round(scrollTop * this.scrollRatio[Orientation.VERTICAL]);
		var targetLeft = Math.round(scrollLeft * this.scrollRatio[Orientation.HORIZONTAL]);

		this.$gripVertical.css("top",
			Math.min(
				Math.max(
					this.$gripVertical.data("minScroll"), targetTop),
					this.$gripVertical.data("maxScroll")));

		this.$gripHorizontal.css("left",
			Math.min(
				Math.max(
					this.$gripHorizontal.data("minScroll"), targetLeft),
					this.$gripHorizontal.data("maxScroll")));
	}

	function switchOn(name, autoOff)
	{
		cancelOff.call(this, name);
		this.$element.addClass(className[name]);
		if (autoOff)
		{
			delayOff.call(this, name, true);
		}
	}

	function switchOff(name)
	{
		this.$element.removeClass(className[name]);
	}

	function delayOff(name)
	{
		cancelOff.call(this, name);
		this.switches[name].id = setTimeout($.proxy(switchOff, this, name), this.switches[name].duration);
	}

	function cancelOff(name)
	{
		if (this.switches[name])
			clearTimeout(this.switches[name].id);
	}

	function onGripMouseDown(e)
	{
		var $grip = $(e.currentTarget);
		var $track = $grip.closest(".atom-scrolltrack");

		atom.drag.on("move", this.onDragMove);
		atom.drag.on("stop", this.onDragEnd);

		var vertical = $grip.data("orientation") == Orientation.VERTICAL;

		var specs = {
			moveX: !vertical,
			moveY: vertical,
			minX: this.$gripHorizontal.data("minScroll"),
			maxX: this.$gripHorizontal.data("maxScroll"),
			minY: this.$gripVertical.data("minScroll"),
			maxY: this.$gripVertical.data("maxScroll")
		};

		atom.drag.start(e, e.currentTarget, specs);
		this.fireEvent("dragstart");
		$track.addClass("active");

		this.$scrollGrip = $grip;
		this.scrollOrientation = vertical ? Orientation.VERTICAL : Orientation.HORIZONTAL;

		return false;
	}

	function onDragMove()
	{
		scrollContentFromGripDrag.call(this);
	}

	function onDragEnd()
	{
		atom.drag.off("move", this.onDragMove);
		atom.drag.off("stop", this.onDragEnd);

		var $track = this.$scrollGrip.closest(".atom-scrolltrack");
		$track.removeClass("active");

		this.scrollOrientation = null;
		this.fireEvent("dragend");
		this.$trackVertical.removeClass("active");
	}

	function onScroll()
	{
		switchOn.call(this, "ENGAGED", true);
		positionGripsFromScrolling.call(this);
		this.fireEvent("scroll");
	}

	function onScrollTrackMouseOver(e)
	{
		e.stopPropagation();
	}

	function onScrollTrackMouseOut(e)
	{
		e.stopPropagation();
	}

	function onScrollTrackClick(e)
	{
		var $track = $(e.currentTarget);
		var $grip = $track.find(".atom-scrollgrip");
		var props = PROPS[$grip.data("orientation")];

		var gripStart = parseInt($grip.css(props.position));
		var gripEnd = parseInt(gripStart + $grip[props.size]());

		var eventPos = e[props.offsetPos];
		if (eventPos < gripStart || eventPos > gripEnd)
		{
			var pageSize = this.$scroller[props.size]();
			var scrollPos = this.$scroller.prop(props.scrollPos);
			if (eventPos < gripStart)
			{
				console.log("Moving from click to: " + (scrollPos - pageSize));
				this.$scroller.prop(props.scrollPos, scrollPos - pageSize);
			}
			else
			{
				console.log("Moving from click to: " + (scrollPos + pageSize));
				this.$scroller.prop(props.scrollPos, scrollPos + pageSize);
			}
		}
	}

	function onDocumentMouseMove(e)
	{
		if (atom.event.isMouseInRange(e, this.$element))
		{
			if (!this.mouseInRange)
			{
				switchOn.call(this, "HOVER");
				this.mouseInRange = true;
				onMouseOver.call(this, e);
			}
		}
		if (!atom.event.isMouseInRange(e, this.$element))
		{
			if (this.mouseInRange)
			{
				switchOff.call(this, "HOVER");
				this.mouseInRange = false;
				onMouseOut.call(this, e);
			}
		}
	}

	function onMouseOver(e)
	{
		if (!this.isScrollable() || !this.enabled)
			return;

		switchOn.call(this, "HOT", true);
		e.stopPropagation();

		updateClasses.call(this);

		var parent = this.$element.parent();
		while (parent.length != 0)
		{
			var instance = scroller.get(parent);
			if (instance != null)
			{
				toggleScrolling.call(this);
			}

			parent = parent.parent();
		}
	}

	function onMouseOut(e)
	{
		if (!this.isScrollable() || !this.enabled)
			return;

		switchOff.call(this, "HOT");
		var parent = this.$element.parent();
		while (parent.length != 0)
		{
			var instance = scroller.get(parent);
			if (instance != null)
			{
				toggleScrolling.call(this);
			}

			parent = parent.parent();
		}
	}

	/**
	 * Converts a regular scroll to a horizontal scroll automatically as required
	 * @param e
	 * @returns {boolean}
	 */
	function onMouseWheel(e)
	{
		if (!this.enabled || !this.isScrollable())
			return true;

		if (this.isScrollableVertically() || e.shiftKey)
			return true;

		var delta = this.$scroller.wheelDelta(e);
		var scrollLeft = this.scrollLeft();
		this.scrollLeft(scrollLeft - (delta.deltaY * 30));
	}

	function measureScrollbarSize()
	{
    var $c = $("<div style='position: absolute; top:-1000px; left:-1000px; width:100px; height:100px; overflow:scroll;'></div>").appendTo("body");
    var dim = {
      width: $c.width() - $c[0].clientWidth,
      height: $c.height() - $c[0].clientHeight
    };
    return dim;
  }
});

/**
 * Implements a simple slider control.
 * @constructor
 * @param {HtmlElement} elem The HTMLElement that represents this control.
 * @event onchange Occurs when the value is changed, either programmatically or by moving the slider manually.
 * @event onreset Occurs when the value is reset to the default value.
 * @event ondragcomplete Occurs at the end of a drag move operation
 */
atom.controls.Slider = atom.controls.register(new function Slider()
{
	var slider = this;
	var orientation = { HORIZONTAL: 0, VERTICAL: 1 };
	var layout = { NORMAL: 0, INVERTED: 1 };

	var props =
	{
		"0": { pos: "left", min: "minX", max: "maxX", size: "width", outerSize: "outerWidth" },
		"1": { pos: "top", min: "minY", max: "maxY", size: "height", outerSize: "outerHeight" }
	};

	this.NAME = "atom.controls.Slider";

	/**
	 * This control doesn't need asynchronous initialization.
	 * @type {Boolean}
	 */
	this.async = false;

	/**
	 * The css expression of elements that should be initialized as tooltips.
	 * @type {String}
	 */
	this.expression = ".slider";

	/**
	 * Prepares the slider control
	 */
	this.setup = function setup()
	{
		$(window).on("resize", this.redraw);
	};

	/**
	 * @class
	 */
	this.Settings = atom.Settings.extend(function SliderSettings(element, settings)
	{
		this.orientation = $(element).hasClass("vertical") ? orientation.VERTICAL : orientation.HORIZONTAL;
		this.layout = this.orientation == orientation.VERTICAL ? layout.INVERTED : layout.NORMAL;

		this.roundValues = this.getBoolean("roundValues", element, settings, true);
		this.minValue = this.getNumber("minValue", element, settings, 0);
		this.maxValue = this.getNumber("maxValue", element, settings, 100);
		this.value = this.getNumber("value", element, settings);
		this.defaultValue = this.getNumber("defaultValue", element, settings, this.minValue);
		this.textElement = this.getString("textElement", element, settings, atom.string.EMPTY).trim();
		this.controlElement = this.getString("controlElement", element, settings, atom.string.EMPTY).trim();

		if (this.maxValue < this.minValue)
		{
			this.maxValue = this.minValue;
		}
	});

	/**
	 * Implements a custom slider control.
	 * @event change
	 * @event reset
	 * @event dragcomplete
	 */
	this.Control = atom.HtmlControl.extend(function Slider(element, settings)
	{
		this.construct(element, "change", "reset");

		this.settings = new slider.Settings(element, settings);
		this.props = props[this.settings.orientation];

		this.ready = false;
		this.$handle = $("<div class='handle'/>");
		this.$element.html(atom.string.EMPTY);
		this.$element.append(this.$handle);
		this.$text = findElement(this.$element, this.settings.textElement);
		this.$control = findElement(this.$element, this.settings.controlElement);

		this.$handle.on("mousedown", $.proxy(onHandleMouseDown, this));
		this.$handle.on("dblclick", $.proxy(this.reset, this));

		this.onDragMove = $.proxy(onDragMove, this);
		this.onDragEnd = $.proxy(onDragEnd, this);

		this.minPos = 0;
		this.maxPos = 0;
		this.ratio = 0;

		redrawInstance(this);
		updateControls(this, true);

		this.initValue = parseFloat(this.$element.attr("data-value")) || 0;
		this.value(this.initValue);

		slider.registerInstance(this);
	});

	this.Control.prototype.reset = function slider$reset()
	{
		this.value(this.settings.defaultValue);
	};

	this.Control.prototype.value = function slider$value(value)
	{
		if (!this.$element.is(":visible"))
		{
			if (value)
				this._value = value;

			return this._value != null ? this._value : this.initValue;
		}

		var pos = parseInt(this.$handle.css(this.props.pos));
		var current = this.settings.layout == layout.INVERTED
			? this.settings.minValue + ((this.maxPos - pos) * this.ratio)
			: this.settings.minValue + (pos * this.ratio);

		if (atom.type.isNumeric(value))
		{
			var newValue = Math.max(Math.min(parseFloat(value), this.settings.maxValue), this.settings.minValue);
			var changed = newValue != this._value;

			this._value = newValue;
			if (this._value != current)
			{
				// this takes into account the range between min and max values
				var converted = this._value - this.settings.minValue;
				converted = this.settings.layout == layout.INVERTED
					? this.maxPos - converted
					: converted;

				pos = Math.round(converted / this.ratio);
				this.$handle.css(this.props.pos, pos);
			}

			updateControls(this);
			if (changed)
				this.fireEvent("change");
		}
		else
		{
			this._value = current;
		}

		if (this.settings.roundValues)
			this._value = Math.round(this._value);

		return this._value;
	};

	function findElement(element, expression)
	{
		if (expression)
		{
			if (expression.indexOf("this::") == 0)
				return element.find(expression.replace(/^this::/, atom.string.EMPTY));
			else
				return $(expression);
		}

		return null;
	}

	function redrawInstance(instance)
	{
		if (!instance.$element.is(":visible"))
			return;

		var sizeElement = instance.$element[instance.props.size]();
		var sizeHandle = instance.$handle[instance.props.outerSize]();

		instance.minPos = 0;
		instance.maxPos = sizeElement - sizeHandle;

		var minv = instance.settings.minValue;
		var maxv = instance.settings.maxValue;
		var range = maxv - minv;

		if (minv < 0 && maxv > 0)
			range = maxv + Math.abs(minv);

		instance.ratio = range / instance.maxPos;
		instance.range = range;

		if (!instance.ready)
		{
			instance.value(instance._value);
			instance.ready = true;
		}
	}

	function onHandleMouseDown(e)
	{
		atom.drag.on("move", this.onDragMove);
		atom.drag.on("stop", this.onDragEnd);

		var specs = {
			moveX: this.settings.orientation == orientation.HORIZONTAL,
			moveY: this.settings.orientation == orientation.VERTICAL,
			minX: this.minPos, maxX: this.maxPos,
			minY: this.minPos, maxY: this.maxPos
		};

		atom.drag.start(e, this.$handle, specs);
		return false;
	}

	function onDragMove()
	{
		updateControls(this);
		this.fireEvent("change");
	}

	function onDragEnd()
	{
		atom.drag.off("move", this.onDragMove);
		atom.drag.off("stop", this.onDragEnd);
	}

	function updateControls(instance)
	{
		if (instance.$text || instance.$control)
		{
			var value = instance.value();

			if (instance.$text)
				instance.$text.text(value);

			if (instance.$control)
				instance.$control.val(value);
		}
	}

	this.redraw = function redrawAll()
	{
		for (var i = 0; i < slider.instances.length; i++)
			redrawInstance(slider.instances[i]);
	};

});

atom.controls.XContainer = atom.controls.register(new function XContainerModule()
{
	var MIN_DIFF_MOVE = 10;
	var DEFAULT_SPEED = 400;

	var PROPS =
	{
		VERTICAL:
		{
			scrollSize: "scrollHeight", outerSize: "outerHeight", size: "height", position: "top",
			offsetPos: "offsetTop", minPos: "minY", maxPos: "maxY", paddingStart: "paddingTop", paddingEnd: "paddingBottom",
			marginStart: "marginTop", eventPos: "eventY"
		},
		HORIZONTAL:
		{
			scrollSize: "scrollWidth", outerSize: "outerWidth", size: "width", position: "left",
			offsetPos: "offsetLeft", minPos: "minX", maxPos: "maxX", paddingStart: "paddingLeft", paddingEnd: "paddingRight",
			marginStart: "marginLeft", eventPos: "eventX"
		}
	};

	var xcontainer = this;

	this.NAME = "atom.controls.XContainer";

	/**
	 * This control doesn't need asynchronous initialization.
	 * @type {Boolean}
	 */
	this.async = false;

	/**
	 * The css expression of elements that should be initialized as xcontainers.
	 * @type {String}
	 */
	this.expression = ".xcontainer";

	/**
	 * Prepares the xcontainer control
	 */
	this.setup = function setup()
	{
		$(window).on("resize", this.redraw);
	};

	this.redraw = function redraw()
	{
		for (var i = 0; i < xcontainer.instances.length; i++)
			xcontainer.instances[i].redraw();
	};

	/**
	 * Defines the settings of the <c>XContainer</c> control.
	 * @extends {atom.Settings}
	 * @param {Object} data The object with initial settings.
	 * @param {Object} element The object with overriding settings. If a setting exist both in
	 * <c>data</c> and in <c>override</c> objects, the setting from <c>override</c> takes precedence.
	 */
	this.Settings = atom.Settings.extend(function XContainerSettings(element, settings)
	{
		this.extraLength = this.getNumber("extra-length", element, settings, 0);
		this.useAnimations = this.getBoolean("use-animations", element, settings, true);
		this.mouseDraggable = this.getBoolean("mouse-draggable", element, settings, true);
		this.autoScroll = this.getBoolean("auto-scroll", element, settings, true);
		this.snapToItems = this.getBoolean("snap-to-items", element, settings, false);
		this.snapDistance = this.getNumber("snap-distance", element, settings, 35);
		this.autoCenter = this.getBoolean("center", element, settings, false);
		this.forceSnapPoints = this.getBoolean("force-snap-points", element, settings, false);
		this.snapItemsExpression = this.getString("snap-item-expression", element, settings, "> *");
		this.scrollOnClick = this.getBoolean("scroll-on-click", element, settings, false);
		this.arrowsOnHover = this.getBoolean("arrows-on-hover", element, settings, false);
		this.animSpeed = this.getNumber("animation-speed", element, settings, DEFAULT_SPEED);
		this.vertical = this.getBoolean("vertical", element, settings, false);
		this.useCentering = this.getBoolean("center", element, settings, false);
	});

	/**
	 * Defines the settings of the <c>XContainer</c> control.
	 * @extends {atom.HtmlControl}
	 */
	this.Control = atom.HtmlControl.extend(function XContainer(element, settings)
	{
		this.construct("move", "stop", "select");

		this.settings = new xcontainer.Settings(element, settings);

		this.$element = $(element);
		this.$slides = this.$element.find(this.settings.snapItemsExpression);
		this.$container = this.$element.parent();

		this.props = this.settings.vertical ? PROPS.VERTICAL : PROPS.HORIZONTAL;

		this.$element.css({ position: "relative", display: "block", overflow: "hidden" });
		if (!this.settings.vertical)
			this.$element.css({ whiteSpace: "nowrap" });
		else
			this.$element.addClass("vertical");

		this.mouseWithinRange = false;
		this.animEngaged = false;
		this.wasDragged = false;

		this.transitioning = false;
		this.stripLength = 0;
		this.slideAutoIndex = 0;
		this.slideSelectedIndex = 0;
		this.scrollMoveSize = 0;
		this.slidesFrozen = false;

		this.startPos = 0;
		this.startEventPos = 0;

		this.minMouseX = 0;
		this.minMouseY = 0;
		this.maxMouseX = this.$container.outerWidth();
		this.maxMouseY = this.$container.outerHeight();

		this.initPos = this.$element.position()[this.props.position];
		this.targetPos = 0;
		this.targetDir = 0;

		this.wheelMultiplier = 1;
		this.wheelAnimActive = false;
		this.wheelDirection = 0;

		// this should be false on IOS natural scroll devices
		this.wheelAccellerationOn = true;

		// these settings are used with jQuery animation (mouse move)
		this.animSettings =
		{
			easing: "easeOutExpo",
			step: $.proxy(onAnimationUpdate, this),
			start: $.proxy(onAnimationStart, this),
			complete: $.proxy(onAnimationComplete, this),
			duration: this.settings.animSpeed
		};

		this.$element.on(atom.dom.mouseDownEvent, $.proxy(onMouseDown, this));
		this.$element.on(atom.dom.mouseUpEvent, $.proxy(onMouseUp, this));
		this.$slides.on(atom.dom.mouseUpEvent, $.proxy(onSlideMouseUp, this));
		this.$slides.on("click", $.proxy(onSlideClick, this));
		this.$container.on("mousewheel DOMMouseScroll", $.proxy(onMouseWheel, this));

		this.elementDragEndListener = $.proxy(onDragEnd, this);
		this.elementDragMoveListener = $.proxy(onDragMove, this);

		$(document).on("keydown", $.proxy(onDocumentKeyDown, this));
		$(window).on("resize", $.proxy(onWindowResize, this));
		$(document).on("mousemove", $.proxy(onDocumentMouseMove, this));

		if (!atom.dom.isTouchDevice())
		{
			if (this.settings.arrowsOnHover)
			{
				this.$arrowBack = $('<div class="xcontainer-arrow prev {0}"></div>'.format(this.settings.vertical ? "vertical" : ""))
						.appendTo(this.$container);
				this.$arrowNext = $('<div class="xcontainer-arrow next {0}"></div>'.format(this.settings.vertical ? "vertical" : ""))
						.appendTo(this.$container);

				this.$arrowBack.on("click", $.proxy(onBackArrowClicked, this));
				this.$arrowNext.on("click", $.proxy(onNextArrowClicked, this));
			}
		}

		this.redraw();
		this.$element.addClass("ready");
	});

	this.Control.prototype.redraw = function redraw(maintainPosition)
	{
		var containerLength = this.$container[this.props.size]();
		if (containerLength == 0)
			return;

		var stripLength =
			(parseInt(this.$element.css(this.props.paddingStart)) || 0) +
			(parseInt(this.$element.css(this.props.paddingEnd)) || 0);

		var $children = this.$element.find("> *");
		for (var i = 0; i < $children.length; i++)
		{
			var child = $children.eq(i);
			stripLength += child[this.props.outerSize](true);
		}

		this.minMouseX = 0;
		this.minMouseY = 0;
		this.maxMouseX = this.$container.outerWidth();
		this.maxMouseY = this.$container.outerHeight();

		this.containerLength = containerLength;
		this.stripLength = stripLength;
		this.scrollMoveSize = Math.min(
			$children.eq(0).outerWidth(true),
			$children.eq(0).outerHeight(true));

		this.maxX = 0;
		this.maxY = 0;
		this.minX = (this.containerLength - this.stripLength) - (parseInt(this.$element.css(this.props.paddingStart)) || 0);
		this.minY = this.containerLength - this.stripLength;

		var trackLength = (stripLength - containerLength)
			+ this.initPos
			+ this.settings.extraLength * 2;

		this.ratio = trackLength / containerLength;
		this.stripLength = stripLength + this.settings.extraLength;

		this.$slides = this.$element.find(this.settings.snapItemsExpression);

		// the extra one pixel ensures there is always enough size for floating children
		this.$element.css(this.props.size, Math.ceil(stripLength) + 1);
		this.snapCoords = getSnapCoords.call(this);

		if (stripLength < containerLength)
		{
			if (this.settings.useCentering)
			{
				var center = Math.round(containerLength / 2 - stripLength / 2);
				this.$element.css(this.props.position, center);
			}
			else
			{
				this.$element.css(this.props.size, containerLength);
				this.$element.css(this.props.position, 0);
			}
			var targetPos = this.settings.autoCenter ? Math.round((containerLength / 2) - (stripLength / 2)) : 0;
			this.$element.css(this.props.position, targetPos);
		}
		else
		{
			var current = parseInt(this.$element.css(this.props.position)) || 0;
			if (maintainPosition)
			{
				current = -this.snapCoords[this.slideIndex()];
			}

			var minPos = this[this.props.minPos];
			var maxPos = this[this.props.maxPos];

			var target = Math.min(Math.max(current, minPos), maxPos);
			this.$element.css(this.props.position, target);
			this.slideAutoIndex = getNearestIndex.call(this, 0);
		}
	};

	this.Control.prototype.scrollToSlide = function scrollToSlide(index)
	{
		if (index >= 0 && index < this.snapCoords.length)
			scrollToSlide.call(this, index, false);
	};

	this.Control.prototype.slideIndex = function slideIndex()
	{
		return this.transitioning ? this.slideSelectedIndex : this.slideAutoIndex;
	};

	this.Control.prototype.setSlidesFrozen = function setSlidesFrozen(frozen)
	{
		this.slidesFrozen = !!frozen;
		if (this.slidesFrozen)
			this.hideArrows();
	};

	this.Control.prototype.hideArrows = function hideArrows()
	{
		updateArrowVisibility.call(this, true);
	};

	function getNearestIndex(accel)
	{
		if (accel < 0 && this.slideAutoIndex == this.$slides.length - 1)
			return this.slideAutoIndex;

		if (accel > 0 && this.slideAutoIndex == 0)
			return this.slideAutoIndex;

		var maxWidth = Number.NEGATIVE_INFINITY;
		var nearestIndex = -1;

		var viewPort = { start: this.$container.offset()[this.props.position] };
		viewPort.end = viewPort.start + this.$container[this.props.size]();

		var viewPortSize = viewPort.end - viewPort.start;
		for (var i = 0; i < this.$slides.length; i++)
		{
			var $child = this.$slides.eq(i);
			var offset = $child.offset()[this.props.position];
			var size = $child[this.props.size]();
			var visibleWidth = viewPortSize -
				Math.abs(viewPort.end - (offset + size)) -
				Math.abs(viewPort.start - offset);

			if (visibleWidth > maxWidth)
			{
				maxWidth = visibleWidth;
				nearestIndex = i;
			}
		}

		if (accel > 0 && nearestIndex >= this.slideAutoIndex)
			nearestIndex = this.slideAutoIndex - 1;

		if (accel < 0 && nearestIndex <= this.slideAutoIndex)
			nearestIndex = this.slideAutoIndex + 1;

		return nearestIndex;
	}

	function getSnapCoords()
	{
		var coords = [];

		var totalWidth = 0;
		var finalPos;
		var offset = parseInt(this.$element.css(this.props.paddingStart));
		for (var i = 0; i < this.$slides.length; i++)
		{
			var $c = this.$slides.eq(i);
			var itemPos = $c.prop(this.props.offsetPos);
			var itemSpacing = parseInt($c.css(this.props.marginStart)) || 0;
			var itemWidth = $c.width();

			finalPos = itemPos - (offset + itemSpacing);
			coords.push(finalPos);

			totalWidth = finalPos + itemWidth;
		}

		if (totalWidth < this.stripLength)
		{
			var diff = this.stripLength - totalWidth;
			var additionalSteps = Math.ceil(diff / this.containerLength);
			var maxLeft = this.stripLength - this.containerLength;
			for (var i = 0; i < additionalSteps; i++)
			{
				var proposedCoord = totalWidth + i * additionalSteps;
				coords.push(Math.max(proposedCoord, maxLeft));
			}
		}

		return coords;
	}

	function getNearestSnapPoint(coordinate)
	{
		var nearest = Number.MAX_VALUE;
		for (var i = 0; i < this.snapCoords.length; i++)
		{
			var point = -this.snapCoords[i];
			if (Math.abs(point - coordinate) < (nearest - coordinate))
				nearest = point;
		}

		return nearest;
	}

	function getPosition(which)
	{
		var position = this.$element.css("position");
		var coordinates = this.$element.position();
		var marginTop = parseInt(this.$element.css("marginTop")) || 0;
		var marginLeft = parseInt(this.$element.css("marginLeft")) || 0;
		var paddingTop = parseInt(this.$element.parent().css("paddingTop")) || 0;
		var paddingLeft = parseInt(this.$element.parent().css("paddingLeft")) || 0;

		if (position != "absolute")
			coordinates.left -= (paddingLeft + marginLeft);

		if (position != "absolute")
			coordinates.top -= (paddingTop + marginTop);

		coordinates.right = coordinates.left + this.$element.width();
		coordinates.bottom = coordinates.top + this.$element.height();

		return which ? coordinates[which] : coordinates;
	}

	function getCoordinateWithinBounds(position)
	{
		var minPos = this[this.props.minPos];
		var maxPos = this[this.props.maxPos];

		return Math.min(Math.max(position, minPos), maxPos);
	}

	function getNormalizedWheelDelta(e)
	{
		var f;
		var o = e.originalEvent || e;
		var d = o.detail;
		var w = o.wheelDelta;
		var n = 225, n1 = n-1;

		// Normalize delta
		d = d ? w && (f = w/d) ? d/f : -d/0.15 : w/120;

		// Quadratic scale if |d| > 1
		d = d < 1 ? d < -1 ? (-Math.pow(d, 2) - n1) / n : d : (Math.pow(d, 2) + n1) / n;

		// Delta *should* not be greater than 2...
		return Math.min(Math.max(d / 2, -1), 1);
	}

	function smoothStop()
	{
		var currPos = parseInt(this.$element.css(this.props.position));
		this.wasThrown = currPos >= this[this.props.minPos] && currPos <= this[this.props.maxPos];

		var targetPos = currPos + (8 * this.accel);
		var nearestSnap = getNearestSnapPoint.call(this, targetPos);

		if (Math.abs(nearestSnap - targetPos) <= this.settings.snapDistance)
			targetPos = nearestSnap;

		if (!this.wasThrown)
			targetPos = getCoordinateWithinBounds.call(this, targetPos);

		if (currPos == targetPos)
		{
			onAnimationComplete.call(this);
			return;
		}

		var animProps = {};
		animProps[this.props.position] = targetPos;

		this.$element.velocity("stop");
		this.$element.velocity(animProps, this.animSettings);
	}

	function scrollToSlide(index, fireSelect)
	{
		var props = {};
		var targetCoord = getCoordinateWithinBounds.call(this, -this.snapCoords[index]);
		props[this.props.position] = targetCoord;

		this.slideAutoIndex = index;
		this.slideSelectedIndex = index;
		if (fireSelect && parseInt(this.$element.css(this.props.position)) == targetCoord)
		{
			this.fireEvent("select");
			return;
		}

		var instance = this;
		this.$element.velocity("stop");
		this.$element.velocity(props, $.extend({}, this.animSettings,
		{
			complete: function ()
			{
				instance.animSettings.complete.apply(instance, arguments);
				if (fireSelect)
					instance.fireEvent("select");
			}
		}));
	}

	function scrollToPrevSlide(fireSelect)
	{
		if (this.slidesFrozen)
			return;

		var slideIndex = this.slideIndex();
		var targetIndex = slideIndex == 0 ? 0 : slideIndex - 1;
		scrollToSlide.call(this, targetIndex, fireSelect);
	}

	function scrollToNextSlide(fireSelect)
	{
		if (this.slidesFrozen)
			return;

		var slideIndex = this.slideIndex();
		var targetIndex = slideIndex == this.snapCoords.length - 1 ? slideIndex : slideIndex + 1;

		scrollToSlide.call(this, targetIndex, fireSelect);
	}

	function updateArrowVisibility(forceHide)
	{
		if (atom.dom.isTouchDevice() || !this.settings.arrowsOnHover)
			return;

		if (forceHide || !this.mouseWithinRange)
		{
			this.$arrowBack.removeClass("visible");
			this.$arrowNext.removeClass("visible");
		}
		else
		{
			if (this.slidesFrozen)
				return;

			var position = getPosition.call(this)[this.props.position];

			this.$arrowBack.toggleClass("visible", position < this.maxX);
			this.$arrowNext.toggleClass("visible", position > this.minX);
		}
	}

	function getEventInfo(e)
	{
		e = e.originalEvent || e;

		var html = $("html");
		var offset = this.$container.offset();

		var result = {
			eventX: e.pageX || e.clientX || 0,
			eventY: e.pageY || e.clientY || 0
		};

		if (e.touches && e.touches.length)
		{
			result.eventX = e.touches[0].clientX;
			result.eventY = e.touches[0].clientY;
			result.touches = e.touches;
		}

		var mouseX = (result.eventX - offset.left) + html.prop("scrollLeft");
		var mouseY = (result.eventY - offset.top) + html.prop("scrollTop");

		result.inRange =
			mouseX >= this.minMouseX && mouseX <= this.maxMouseX &&
			mouseY >= this.minMouseY && mouseY <= this.maxMouseY;

		return result;
	}

	function keepWithinBounds()
	{
		var currentPos = this.$element.position()[this.props.position];
		if (currentPos < this[this.props.minPos] || currentPos > this[this.props.maxPos])
		{
			this.wasCancelled = true;

			var animPros = {};
			animPros[this.props.position] = currentPos < this[this.props.minPos]
				? this[this.props.minPos]
				: this[this.props.maxPos];

			this.$element.velocity("stop");
			this.$element.velocity(animPros, $.extend({}, this.animSettings,
			{
				duration: this.settings.animSpeed / 2
			}));
		}
	}

	function snapToNearest()
	{
		if (!this.settings.snapToItems)
			return false;

		var currentPos = this.$element.position()[this.props.position];
		var nearestSnap = getNearestSnapPoint.call(this, currentPos);

		if (Math.abs(nearestSnap - currentPos) <= this.settings.snapDistance)
		{
			if (this.move != null)
			{
				this.move.kill();
				this.move = null;
			}

			this.wasCancelled = true;

			var animPros = {};
			animPros[this.props.position] = nearestSnap;

			this.$element.velocity(animPros, $.extend({}, this.animSettings,
			{
				duration: this.settings.animSpeed / 2
			}));

			return true;
		}

		return false;
}

	function onAnimationStart()
	{
		this.animEngaged = true;
	}

	function onAnimationUpdate(now, tween)
	{
		if (tween)
		{
			tween.end = this.targetPos;
		}

		var currentPosition = parseInt(this.$element.css(this.props.position));

		this.fireEvent("stop", {
			position: currentPosition,
			start: currentPosition == this[this.props.maxPos],
			end: currentPosition == this[this.props.minPos]
		});

		this.slideAutoIndex = getNearestIndex.call(this, 0);
		this.fireEvent("move", {
			atStart: currentPosition == this[this.props.maxPos],
			atEnd: currentPosition == this[this.props.minPos]
		});

		if (this.wasCancelled)
			return;
		if (!this.wasThrown && !this.wasScrolled)
			return;

		var snapped = snapToNearest.call(this);
		if (!snapped)
			keepWithinBounds.call(this);
	}

	function onAnimationComplete()
	{
		this.targetDir = 0;
		this.animEngaged = false;
		this.wasThrown = false;
		this.wasScrolled = false;
		this.wasCancelled = false;

		var currentPosition = parseInt(this.$element.css(this.props.position));

		this.fireEvent("stop", {
			position: currentPosition,
			atStart: currentPosition == this[this.props.maxPos],
			atEnd: currentPosition == this[this.props.minPos]
		});

		updateArrowVisibility.call(this);
	}

	function onDocumentMouseMove(e)
	{
		if (this.wasDragged)
			return true;

		var outerSize = this.$container[this.props.outerSize]();
		if (outerSize == 0)
			return true;

		var html = $("html");
		var offset = this.$container.offset();
		var mouseX = (e.clientX - offset.left) + html.prop("scrollLeft");
		var mouseY = (e.clientY - offset.top) + html.prop("scrollTop");

		var eventInfo = getEventInfo.call(this, e);
		this.mouseWithinRange = (this.stripLength > outerSize) && eventInfo.inRange;

		if (this.mouseWithinRange && this.settings.autoScroll)
		{
			var mousePos = this.settings.vertical ? mouseY : mouseX;
			var targetPos = Math.round(-(mousePos * this.ratio) + this.settings.extraLength);
			var currentPos = parseInt(this.$element.css(this.props.position));
			var animEngaged = Math.abs(currentPos - targetPos) > MIN_DIFF_MOVE;

			this.targetPos = targetPos;

			if (this.settings.useAnimations && (animEngaged && !this.animEngaged))
			{
				var props = {};
				props[this.props.position] = targetPos;

				this.$element.velocity("stop");
				this.$element.stop().animate(props, this.animSettings);
			}

			if (!this.settings.useAnimations || !this.animEngaged)
			{
				this.$element.css(this.props.position, targetPos);
			}
		}

		updateArrowVisibility.call(this);

		return true;
	}

	function onBackArrowClicked(e)
	{
		scrollToPrevSlide.call(this);
		e.stopPropagation();
		return false;
	}

	function onNextArrowClicked(e)
	{
		scrollToNextSlide.call(this);
		e.stopPropagation();
		return false;
	}

	function onSlideMouseUp(e)
	{
		if (this.wasDragged)
			return true;

		if (e.type == "mouseup" && e.which != 1)
			return true;

		// retrieve the first child of slidestrip that is parent of event source
		// so that it's index can be used.
		var $el = $(e.target);
		while (!$el.parent().is(this.$element))
		{
			$el = $el.parent();
			if ($el.length == 0)
				return true;
		}

		var slideIndex = this.$slides.index($el);
		if (this.settings.scrollOnClick)
		{
			scrollToSlide.call(this, slideIndex, true);
			return false;
		}

		this.slideAutoIndex =
		this.slideSelectedIndex = slideIndex;

		var event = atom.event.create(this, "select");
		this.fireEvent(event);
		return event.cancel !== true;
	}

	function onSlideClick()
	{
		return !this.wasDragged;
	}

	function onMouseWheel(e)
	{
		if (!this.settings.mouseDraggable || !this.mouseWithinRange || this.transitioning || this.slidesFrozen)
			return true;

		var event = e.originalEvent || e;
		var amount = -event.wheelDelta || event.detail;

		var currentPosition = getPosition.call(this, this.props.position);
		if (amount > 0 && currentPosition == this[this.props.minPos])
			return;

		if (amount < 0 && currentPosition == this[this.props.maxPos])
			return;

		if (this.settings.forceSnapPoints)
		{
			if (amount > 0)
				scrollToNextSlide.call(this);

			else if (amount < 0)
				scrollToPrevSlide.call(this);
		}
		else
		{
			var hasWheelDelta = event.wheelDelta != null;

			var delta = getNormalizedWheelDelta(e);

			this.accel = (hasWheelDelta ? delta * 4 : amount);

			// different routines for with and w/o wheel delta
			var nextPos = hasWheelDelta
				? getCoordinateWithinBounds.call(this, currentPosition + (delta * this.scrollMoveSize))
				: getCoordinateWithinBounds.call(this, currentPosition - (amount * (this.scrollMoveSize / 25)));

			// for some reason, in ff the wheel amount goes up and down, causing a jerky, back and forth movement
			// the following construct fixes this problem by allowing movement in one direction only
			if (this.animEngaged && this.targetDir != 0)
			{
				if (this.targetDir == -1 && nextPos < this.targetPos)
					nextPos = this.targetPos;

				if (this.targetDir == 1 && nextPos > this.targetPos)
					nextPos = this.targetPos;
			}

			this.targetPos = nextPos;
			this.targetDir = amount > 0 ? 1 : -1;

			if (this.wheelAccellerationOn)
			{
				if (this.wheelAnimActive)
				{
					// reset multiplier when direction of scrolling changes
					if (this.wheelMultiplier != 1 && this.wheelDirection != this.targetDir)
						this.wheelMultiplier = 1;

					this.wheelMultiplier += .05;
					this.wheelDirection = this.targetDir;
				}

				var diff = (this.targetPos * this.wheelMultiplier) - this.targetPos;

				this.targetPos =
					getCoordinateWithinBounds.call(this, this.targetPos + (this.targetDir * diff));
			}

			var animProps = {};
			animProps[this.props.position] = this.targetPos;

			var self = this;
			this.wasScrolled = true;
			this.wheelAnimActive = true;

			this.$element.velocity("stop");
			this.$element.velocity(animProps, $.extend({}, this.animSettings,
			{
				complete: function ()
				{
					self.wheelAnimActive = false;
					self.wheelMultiplier = 1;
					self.animSettings.complete.apply(this, arguments);
				}
			}));
		}

		atom.event.cancel(e);
		return false;
	}

	function onWindowResize(e)
	{
		this.redraw(true);
	}

	function onDocumentKeyDown(e)
	{
		if (e.target.tagName.match(/^input|textarea|select$/i))
			return true;

		if (this.settings.vertical)
		{
			if (e.keyCode == atom.const.Key.UP)
			{
				scrollToPrevSlide.call(this);
				return false;
			}

			if (e.keyCode == atom.const.Key.DOWN)
			{
				scrollToNextSlide.call(this);
				return false;
			}
		}
		else
		{
			if (e.keyCode == atom.const.Key.LEFT)
			{
				scrollToPrevSlide.call(this);
				return false;
			}

			if (e.keyCode == atom.const.Key.RIGHT)
			{
				scrollToNextSlide.call(this);
				return false;
			}
		}

		return true;
	}

	function onMouseDown(e)
	{
		if (!this.settings.mouseDraggable)
			return true;

		if (e.type == "mousedown" && e.which != 1)
			return true;

		if (this.stripLength <= this.containerLength)
			return true;

		var event = getEventInfo.call(this, e);
		if (atom.dom.isTouchDevice() && event.touches && event.touches.length != 1)
			return true;

		if (this.animEngaged)
			this.$element.stop();

		this.wasDragged = false;

		this.startPos = getPosition.call(this, this.props.position);
		this.startEventPos = event[this.props.eventPos];

		// start dragging
		document.addEventListener("mouseup", this.elementDragEndListener, true);
		document.addEventListener("touchend", this.elementDragEndListener, true);
		document.addEventListener("mousemove", this.elementDragMoveListener, true);
		document.addEventListener("touchmove", this.elementDragMoveListener, true);

		return atom.event.cancel(e);
	}

	function onDragMove(e)
	{
		var eventInfo = getEventInfo.call(this, e);
		if (!eventInfo.inRange)
		{
			onDragEnd.call(this, e);
			return true;
		}

		if (this.move != null)
		{
			this.move.kill();
			this.move = null;
		}

		var eventPos = eventInfo[this.props.eventPos];

		var diff = eventPos - this.startEventPos;
		var minPos = this[this.props.minPos];
		var maxPos = this[this.props.maxPos];

		var targetPos = this.startPos + diff;
		if (targetPos > maxPos)
		{
			var excess = targetPos - maxPos;
			var ratio = Math.max(0, 1 - excess / this.stripLength);
			diff = Math.round(diff * ratio);

			targetPos = Math.round(this.startPos + diff);
		}
		else if (targetPos < minPos)
		{
			excess = minPos - targetPos;
			ratio = Math.max(0, 1 - excess / this.stripLength);
			diff = Math.round(diff * ratio);

			targetPos = Math.round(this.startPos + diff);
		}

		if (this.settings.snapToItems)
		{
			var snapPoint = getNearestSnapPoint.call(this, targetPos);
			if (Math.abs(snapPoint - targetPos) <= this.settings.snapDistance)
				targetPos = snapPoint;
		}

		var coord1 = getPosition.call(this, this.props.position);
		this.$element.css(this.props.position, targetPos);
		var coord2 = getPosition.call(this, this.props.position);

		this.accel = (coord2 - coord1) * 1.5;

		if (this.accel != 0)
			this.wasDragged = true;

		this.slideAutoIndex = getNearestIndex.call(this, 0);
		this.fireEvent("move");

		return atom.event.cancel(e);
	}

	function onDragEnd(e)
	{
		if (this.wasDragged)
		{
			if (this.settings.forceSnapPoints)
			{
				var nearestIndex = getNearestIndex.call(this, this.accel);
				scrollToSlide.call(this, nearestIndex);
			}
			else
			{
				smoothStop.call(this);
			}
		}

		document.removeEventListener("mouseup", this.elementDragEndListener, true);
		document.removeEventListener("touchend", this.elementDragEndListener, true);
		document.removeEventListener("mousemove", this.elementDragMoveListener, true);
		document.removeEventListener("touchmove", this.elementDragMoveListener, true);

		this.wasDragged = false;
	}

	function onMouseUp(e)
	{
		if (this.settings.mouseDraggable && this.wasDragged)
			return atom.event.cancel(e);

		return true;
	}

});

/**
 * Implements the tooltip control and all of its public and private code.
 */
atom.controls.Tooltip = atom.controls.register(new function Tooltip()
{
	/**
	 * @const
	 * @type {string}
	 */
	var POPUP_ELEMENT_ID = "tooltip_popup";

	var timeoutId;

	var tooltip = this;
	tooltip.instance = null;
	tooltip.$popup = null;
	tooltip.$arrow = null;
	tooltip.document = null;
	tooltip.$content = null;
	tooltip.offset = { left: 0, top: 0 };

	tooltip.NAME = "atom.controls.Tooltip";

	/**
	 * This control doesn't need asynchronous initialization.
	 * @type {Boolean}
	 */
	tooltip.async = false;

	/**
	 * The css expression of elements that should be initialized as tooltips.
	 * @type {String}
	 */
	tooltip.expression = ".tooltip";

	/**
	 * Prepares the tooltip control
	 */
	tooltip.setup = function setup()
	{
		if (tooltip.$popup != null)
			return;

		initializeCoordinates();
		createPopupLayer();

		$.fn.tooltip = jqueryTooltip;
		$(window).resize(initializeCoordinates);
	};

	/**
	 * Defines the settings of the <c>Tooltip</c> control.
	 * @extends {atom.Settings}
	 * @param {Object} data The object with initial settings.
	 * @param {Object} element The object with overriding settings. If a setting exist both in
	 * <c>data</c> and in <c>override</c> objects, the setting from <c>override</c> takes precedence.
	 */
	tooltip.Settings = atom.Settings.extend(function Settings(data, override)
	{
		this.construct(data, override);

		this.className = this.getString("className", data, override, null);
		this.followMouse = this.getBoolean("followMouse", data, override, true);
		this.maxWidth = this.getString("maxWidth", data, override, null);
		this.showOn = this.getString("showOn", data, override, "mouseover").split(/(?:\s*,\s*)|(?:\s+)/);
		this.hideOn = this.getString("hideOn", data, override, "mouseout").split(/(?:\s*,\s*)|(?:\s+)/);
		this.orientation = this.getString("orientation", data, override, "TC").toUpperCase();
		this.offset = this.getNumber("offset", data, override, 3);
		this.showDelay = this.getNumber("show-delay", data, override, 200);
		this.hideDelay = this.getNumber("hide-delay", data, override, 0);
		this.obscureOnly = this.getBoolean("obscureOnly", data, override, false);
		this.useFades = this.getBoolean("useFades", data, override, false);
		this.showArrow = this.getBoolean("showArrow", data, override, true);
		this.arrowOffset = this.getNumber("arrow-offset", data, override, 5);
		this.content = this.getString("content", data, override, null);
	});

	/**
	 * Defines the <c>Tooltip</c> control.
	 * @param {HTMLElement} element The HTML element that shows the tooltip popup.
	 * @param {Settings} [settings] Optional settings to use with this instance.
	 */
	tooltip.Control = atom.HtmlControl.extend(function Tooltip(element, settings)
	{
		this.construct(element);

		this.settings = new tooltip.Settings(element, settings);
		this.$element
			.bind("mouseenter", $.proxy(onElementMouseOver, this))
			.bind("mouseleave", $.proxy(onElementMouseOut, this))
			.bind("focus", $.proxy(onElementFocus, this))
			.bind("blur", $.proxy(onElementBlur, this))
			.bind("click", $.proxy(onElementClick, this));

		var title = this.settings.content
			? this.$element.find(this.settings.content).html()
			: this.$element.data("tooltip") || this.$element.attr("title") || this.$element.attr("data-title");

		this.$element.data("tooltip", title);
		this.$element.attr("data-title", title);
		this.$element.attr("title", atom.string.EMPTY);

		this.showTooltip = $.proxy(this.show, this);
		this.hideTooltip = $.proxy(this.hide, this);

		$(tooltip.document)
			.on("keydown click", $.proxy(onTooltipDocumentEvent, this))
			.on("scroll", $.proxy(onWindowScroll, this));

		tooltip.$popup
			.on("click", $.proxy(onElementClick, this))
			.on("mouseenter", $.proxy(onTooltipMouseOver, this))
			.on("mouseleave", $.proxy(onTooltipMouseOut, this));

		tooltip.registerInstance(this);
	});

	/**
	 * Hides the tooltip
	 */
	tooltip.Control.prototype.hide = function Tooltip$hide()
	{
		tooltip.$popup.hide();
		tooltip.$popup.removeClass(tooltip.$popup.attr("class-added"));

		tooltip.instance = null;
	};

	/**
	 * Shows the tooltip
	 */
	tooltip.Control.prototype.show = function Tooltip$show()
	{
		if (this.$element.hasClass("tooltip-suspend") || this.$element.hasClass("disabled"))
			return;

		if (this.settings.obscureOnly)
		{
			var elem = this.$element[0];
			if (elem.scrollWidth <= elem.offsetWidth && elem.scrollHeight <= elem.offsetHeight)
				return;
		}

		var title = this.$element.data("tooltip");
		if (!title)
			return;

		tooltip.$content.html(title);

		var orientation = this.settings.orientation;
		var orientationP = orientation[0].match(/T|L|R|B/) ? orientation[0] : "T";
		var orientationS = orientation[1].match(/T|L|R|B|C/) ? orientation[1] : "L";

		var html = $("html", tooltip.document)[0];
		var body = $("body", tooltip.document)[0];
		var maxWidth = html.offsetWidth || body.offsetWidth;
		var maxHeight = html.offsetHeight || body.offsetHeight;

		tooltip.$popup.css({ left: 0, top: -1000, display: "block" }).removeClass("t l r b -t -l -r -b");
		tooltip.$popup.addClass(this.settings.className).attr("class-added", this.settings.className);
		tooltip.$arrow.toggle(this.settings.showArrow);

		var primary, secondary;

		if (orientationP.match(/T|B/))
		{
			primary = getPrimaryCoord.call(this, orientationP, 0, maxHeight);
			secondary = getSecondaryCoord.call(this, orientationP, orientationS, 0, maxWidth);

			var specs = {
				popupLeft: secondary.left,
				popupTop: primary.top,
				arrowLeft: secondary.arrow.left,
				arrowTop: primary.arrow.top,
				orientation: primary.orientation,
				orientationS: secondary.orientation
			};
		}
		else
		{
			primary = getPrimaryCoord.call(this, orientationP, 0, maxWidth);
			secondary = getSecondaryCoord.call(this, orientationP, orientationS, 0, maxHeight);

			specs = {
				popupLeft: primary.left,
				popupTop: secondary.top,
				arrowLeft: primary.arrow.left,
				arrowTop: secondary.arrow.top,
				orientation: primary.orientation,
				orientationS: secondary.orientation
			};
		}

		tooltip.$popup.css({ left: specs.popupLeft, top: specs.popupTop });
		tooltip.$arrow.css({ left: specs.arrowLeft, top: specs.arrowTop });

		if (this.settings.showArrow)
		{
			switch(orientationP)
			{
				case "T": showArrow.call(this, "B"); break;
				case "B": showArrow.call(this, "T"); break;
				case "L": showArrow.call(this, "R"); break;
				case "R": showArrow.call(this, "L"); break;
			}
		}

		tooltip.$popup.addClass(specs.orientation.toLowerCase());
		tooltip.$popup.addClass("-" + specs.orientationS.toLowerCase());

		tooltip.instance = this;

		tooltip.$popup.css({ display: "block", opacity: 1 });
	};

	/**
	 * Sets on this instance's <c>Settings</c> values.
	 * @param {String} name The name of the setting to set.
	 * @param {Object} value The value of the setting to set.
	 * @returns {Object} The current value of the setting with the specified <c>name</c>.
	 */
	tooltip.Control.prototype.set = function Tooltip$set(name, value)
	{
		if (value && (name == "showOn" || name == "hideOn"))
			value = value.split(/(?:\s*,\s*)|(?:\s+)/);

		if (this.settings[name] != undefined)
			this.settings[name] = value;

		return this.settings[name];
	};

	function getPrimaryCoord(orientation, edgeA, edgeB)
	{
		var result = { orientation: orientation, arrow: { left: 0, top: 0 } };

		var vertical = orientation.match(/T|B/) != null;
		var orientationRev = "B";
		switch (orientation)
		{
			case "T": orientationRev = "B"; break;
			case "B": orientationRev = "T"; break;
			case "L": orientationRev = "R"; break;
			case "R": orientationRev = "L"; break;
		}
		var props = vertical
			? atom.HtmlControl.PROPS.VERTICAL
			: atom.HtmlControl.PROPS.HORIZONTAL;

		var otherArrowPos = props.position == "top" ? "bottom" : "right";

		var arrowSize = this.settings.showArrow ? tooltip.$arrow[props.size]() : 0;
		if (vertical)
			arrowSize /= 2;

		var elementOffset = this.$element.offset();
		var scrollPos = $(tooltip.document)[props.scrollPos]();
		var absolutePos = tooltip.offset[props.position] + elementOffset[props.position] - scrollPos;
		var popupSize = tooltip.$popup[props.outerSize]() + arrowSize;
		var elementSize = this.$element[props.outerSize]();

		if (orientation.match(/T|L/))
		{
			result[props.position] = absolutePos - popupSize - this.settings.offset;
			result.arrow[props.position] = "auto";
			result.arrow[otherArrowPos] = 0;
		}
		else
		{
			result[props.position] = absolutePos + elementSize + this.settings.offset;
			result.arrow[props.position] = 0;
			result.arrow[otherArrowPos] = "auto";
		}

		var isOversized = popupSize > (edgeB - edgeA);
		if (!isOversized && result[props.position] < edgeA)
			return getPrimaryCoord.call(this, orientationRev, edgeA, edgeB);

		if (!isOversized && (result[props.position] + popupSize) > edgeB)
			return getPrimaryCoord.call(this, orientationRev, edgeA, edgeB);

		return result;
	}

	function getSecondaryCoord(orientationP, orientationS, edgeA, edgeB)
	{
		var result = { orientation: orientationS, arrow: { left: 1, top: 1 } };

		var vertical = orientationS.match(/T|B/) != null;
		if (orientationS == "C")
			vertical = orientationP.match(/L|R/) != null;

		var orientationRev = "B";
		switch (orientationS)
		{
			case "T": orientationRev = "B"; break;
			case "B": orientationRev = "T"; break;
			case "L": orientationRev = "R"; break;
			case "R": orientationRev = "L"; break;
			default:
				orientationRev = vertical ? "B" : "R"; break;
		}
		var props = vertical
			? atom.HtmlControl.PROPS.VERTICAL
			: atom.HtmlControl.PROPS.HORIZONTAL;

		var arrowSize = this.settings.showArrow ? tooltip.$arrow[props.size]() : 0;

		var elementOffset = this.$element.offset();

		var scrollPos = $(tooltip.document)[props.scrollPos]();
		var absolutePos = tooltip.offset[props.position] + elementOffset[props.position] - scrollPos;
		var popupSize = tooltip.$popup[props.outerSize]();
		var elementSize = this.$element[props.outerSize]();

		if (orientationS.match(/T|L/))
		{
			result[props.position] = absolutePos;
			result.arrow[props.position] = this.settings.arrowOffset;
//			if (popupSize > elementSize && this.settings.arrowOffset != 0)
//			{
//				result.arrow[props.position] += (elementSize / 2) - (this.settings.arrowOffset + arrowSize / 2);
//			}
		}
		else if (orientationS.match(/B|R/))
		{
			result[props.position] = (absolutePos + elementSize) - popupSize;
			result.arrow[props.position] = popupSize - arrowSize - this.settings.arrowOffset;
//			if (popupSize > elementSize && this.settings.arrowOffset != 0)
//			{
//				result.arrow[props.position] -= (elementSize / 2) - (this.settings.arrowOffset + arrowSize / 2);
//			}
		}
		else
		{
			result[props.position] = absolutePos + ((elementSize / 2) - popupSize / 2);
			result.arrow[props.position] = (popupSize / 2) - (arrowSize / 2);
		}

		result.oversized = popupSize > (edgeB - edgeA);
		result.beyondFirstEdge = result[props.position] < edgeA;
		result.beyondSecondEdge = (result[props.position] + popupSize) > edgeB;
		result.orientationRev = orientationRev;

		if (!result.oversized && arguments.callee.caller != arguments.callee)
		{
			if (result.beyondFirstEdge || result.beyondSecondEdge)
			{
				var result2 = getSecondaryCoord.call(this, orientationP, orientationRev, edgeA, edgeB);

				// when the secondary coordinate is (C)enter we dont know which side
				// would be opposite side, so in that case we make an exception and
				// make one more call if the result fell out of range
				if (orientationS == "C")
				{
					if (result2.beyondFirstEdge || result2.beyondSecondEdge)
						result2 = getSecondaryCoord.call(this, orientationP, result2.orientationRev, edgeA, edgeB);
				}

				return result2;
			}
		}

		return result;
	}

	function onTooltipMouseOver()
	{
		window.clearTimeout(timeoutId);
	}

	function onTooltipMouseOut()
	{
		this.hideTooltip();
	}

	function onElementMouseOver()
	{
		window.clearTimeout(timeoutId);

		if (tooltip.instance && tooltip.instance != this && !tooltip.instance.settings.hideOn.contains("mouseout"))
			return;

		if (!this.settings.showOn.contains("mouseover"))
			return;

		if (this.$element.hasClass("tooltip-suspend"))
			return;

		if (this.settings.showDelay > 0)
			timeoutId = window.setTimeout(this.showTooltip, this.settings.showDelay);
		else
			this.showTooltip();
	}

	function onElementMouseOut()
	{
		window.clearTimeout(timeoutId);

		if (tooltip.instance != this)
			return;

		if (!this.settings.hideOn.contains("mouseout"))
			return;

		if (this.settings.hideDelay > 0)
			timeoutId = window.setTimeout(this.hideTooltip, this.settings.hideDelay);
		else
			this.hideTooltip();
	}

	function onElementFocus()
	{
		if (this.settings.showOn.contains("focus"))
			this.show();
	}

	function onElementBlur()
	{
		if (this.settings.hideOn.contains("blur"))
			this.hide();
	}

	function onElementClick()
	{
		if (tooltip.$popup.is(":visible") && this.settings.hideOn.contains("click"))
			this.hide();

		else if (!tooltip.$popup.is(":visible") && this.settings.showOn.contains("click"))
			this.show();
	}

	function onTooltipDocumentEvent(e)
	{
		if (!tooltip.$popup.is(":visible") || tooltip.instance != this)
			return;

		if (!jQuery.contains(this.$element[0], e.target) && !jQuery.contains(tooltip.$popup[0], e.target))
			this.hide();
	}

	function onWindowScroll()
	{
		this.hide();
	}

	function jqueryTooltip(settings)
	{
		return this.each(function initializeInstance(i, element)
		{
			new tooltip.Control(element, settings);
		});
	}

	function initializeCoordinates()
	{
		tooltip.offset.top = 0;
		tooltip.offset.left = 0;

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
						tooltip.offset.left += frames.eq(i).offset().left;
						tooltip.offset.top += frames.eq(i).offset().top;
						foundFrame = true;
						break;
					}
				}
			}
		}
	}

	function createPopupLayer()
	{
		var hlayer = $(tooltip.document.getElementById(POPUP_ELEMENT_ID));
		if (hlayer.length != 0)
		{
			tooltip.$popup = hlayer;
		}
		else
		{
			var tooltipHtml = atom.string.format(
				'<div class="atomtooltippopup" id="{0}">' +
					'<div class="arrow"></div>' +
					'<div class="bg"></div>' +
					'<div class="content"></div>' +
				'</div>', POPUP_ELEMENT_ID);

			tooltip.$popup = $(tooltipHtml);
			$(tooltip.document.body).append(tooltip.$popup);

			if (tooltip.document != document)
			{
				var styleElement = $("<style id='tooltipStyles'></style>");
				$(tooltip.document.body).append(styleElement);

				var styleText = [];

				var tooltipRules = atom.css.findRules(".atomtooltippopup");
				for (var i = 0; i < tooltipRules.length; i++)
				{
					var rule = tooltipRules[i];
					styleText.push(rule.css.cssText);
				}

				styleElement.html(styleText.join("\n"));
			}
		}

		tooltip.$content = tooltip.$popup.find(".content");
		tooltip.$arrow = tooltip.$popup.find(".arrow");

		var tw = tooltip.$arrow.width();
		var th = tooltip.$arrow.height();
		var c1 = $('<canvas class="b" width="{0}" height="{1}"/>'.format(tw, th / 2)).appendTo(tooltip.$arrow)[0];
		var c2 = $('<canvas class="t" width="{0}" height="{1}"/>'.format(tw, th / 2)).appendTo(tooltip.$arrow)[0];
		var c3 = $('<canvas class="l" width="{0}" height="{1}"/>'.format(tw / 2, th)).appendTo(tooltip.$arrow)[0];
		var c4 = $('<canvas class="r" width="{0}" height="{1}"/>'.format(tw / 2, th)).appendTo(tooltip.$arrow)[0];

		tooltip.cB = c1;
		tooltip.cT = c2;
		tooltip.cL = c3;
		tooltip.cR = c4;
	}

	function showArrow(side)
	{
		var tw = tooltip.$arrow.width();
		var th = tooltip.$arrow.height();
		var $bg = tooltip.$popup.find(".bg");

		var borderColor = $bg.css("border-bottom-color");
		var borderWidth = parseInt($bg.css("border-bottom-width"));
		var backgroundColor = $bg.css("background-color");

		switch (side)
		{
			case "L":
				drawArrow(tooltip.cL, backgroundColor, borderColor, borderWidth, function draw(ctx)
				{
				  ctx.moveTo(tw / 2, 0);
					ctx.lineTo(0, th / 2);
					ctx.lineTo(tw / 2, th);
				});
				break;

			case "R":
				drawArrow(tooltip.cR, backgroundColor, borderColor, borderWidth, function draw(ctx)
				{
				  ctx.moveTo(0, 0);
					ctx.lineTo(tw / 2, th / 2);
					ctx.lineTo(0, th);
				});
				break;

			case "B":
				drawArrow(tooltip.cB, backgroundColor, borderColor, borderWidth, function draw(ctx)
				{
				  ctx.moveTo(0, 0);
					ctx.lineTo(tw / 2, th / 2);
					ctx.lineTo(tw, 0);
				});
				break;

			case "T":
				drawArrow(tooltip.cT, backgroundColor, borderColor, borderWidth, function draw(ctx)
				{
				  ctx.moveTo(0, th / 2);
					ctx.lineTo(tw / 2, 0);
					ctx.lineTo(tw, th / 2);
				});
				break;
		}
	}

	function drawArrow(canvas, bgColor, fgColor, stroke, draw)
	{
		var ctx = canvas.getContext("2d");
		ctx.clearRect(0, 0, canvas.width, canvas.height);
	  ctx.fillStyle = bgColor;
	  ctx.strokeStyle = fgColor;
	  ctx.lineWidth = stroke;
	  ctx.beginPath();
		draw(ctx);
	  ctx.fill();

		if (stroke)
		  ctx.stroke();

	}

	return tooltip;
});


