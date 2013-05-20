/**
 * Implements a simple scroller control.
 */
aeon.controls.Scroller = aeon.controls.register(new function Scroller(elem, settings)
{
	var scroller = this;
	var scrollers = [];
	var orientation = { VERTICAL: 0, HORIZONTAL: 1};
	var sizing = { FIXED: 0, PROPORTIONAL: 1 };
	var lineHeight = 20;

	var props =
	{
		"0": { scrollSize: "scrollHeight", scrollPos: "scrollTop", size: "height", position: "top" },
		"1": { scrollSize: "scrollWidth", scrollPos: "scrollLeft", size: "width", position: "left" }
	};

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
			redrawInstance(scroller.instances[i]);
	};

	/**
	 * Defines the settings of the <c>Tooltip</c> control.
	 * @constructor
	 * @param {Object} data The object with initial settings.
	 * @param {Object} element The object with overriding settings. If a setting exist both in
	 * <c>data</c> and in <c>override</c> objects, the setting from <c>override</c> takes precedence.
	 */
	this.Settings = aeon.Settings.extend(function ScrollerSettings(element, settings)
	{
		var sizingValue = this.getString("sizing", element, settings, "").toLowerCase();
		this.sizing = sizingValue == "fixed" ? sizing.FIXED : sizing.PROPORTIONAL;
		this.animate = this.getBoolean("animate", element, settings, true);
		this.orientation = $(element).hasClass("horizontal") ? orientation.HORIZONTAL : orientation.VERTICAL;
	});

	/**
	 * Implements a custom scroller control.
	 * @event scroll
	 * @event dragstart
	 * @event dragend
	 */
	this.Control = aeon.HtmlControl.extend(function ScrollerControl(element, settings)
	{
		this.construct(element, "scroll", "dragstart", "dragend");

		this.settings = new scroller.Settings(element, settings);
		this.props = props[this.settings.orientation];

		this.$element.html(aeon.string.format(
			"<div class='aeon-scroller'><div class='aeon-scrollcontent'>{0}</div></div>"+
			"<div class='aeon-scrolltrack'><div class='aeon-scrollgrip'/></div>",
				this.$element.html()));

		aeon.controls.update(this.$element);

		this.$scroller = this.$element.find("> .aeon-scroller");
		this.$content = this.$scroller.find("> .aeon-scrollcontent");
		this.$track = this.$element.find("> .aeon-scrolltrack");
		this.$grip = this.$track.find("> .aeon-scrollgrip");

		this.$grip.on("mousedown", $.proxy(onGripMouseDown, this));
		this.$scroller.on("scroll", $.proxy(onScroll, this));

		this.onDragMove = $.proxy(onDragMove, this);
		this.onDragEnd = $.proxy(onDragEnd, this);

		this.offset = parseInt(this.$grip.css(this.props.position)) || 0;
		this.enabled = true;
		this.overflow = this.$scroller.css("overflow");

		if (this.settings.orientation == orientation.HORIZONTAL)
		{
			var totalSize = 0;
			this.$content.find("> *").each(function addSize(i, element)
			{
				totalSize += $(element).outerWidth(true);
			});

			this.$content.css("width", totalSize);
			this.$scroller
				.on("mousewheel", $.proxy(onMouseWheel, this))
				.on("DOMMouseScroll", $.proxy(onDOMMouseScroll, this));
		}

		this.$element
			.on("mouseenter", $.proxy(onMouseOver, this))
			.on("mouseleave", $.proxy(onMouseOut, this));

		scroller.registerInstance(this);
		redrawInstance(this);
	});

	this.Control.prototype.enable = function ScrollerControl$enable()
	{
		this.enabled = true;
		redrawInstance(this);
		if (this.isScrollable())
		{
			this.$scroller.css("overflow", "");
			this.$track.show();
		}
	};

	this.Control.prototype.disable = function ScrollerControl$disable()
	{
		this.enabled = false;
		this.$track.hide();
		this.$scroller.css("overflow", "hidden");
	};

	this.Control.prototype.isScrollable = function ScrollerControl$isScrollable()
	{
		var totalLength = this.$scroller.prop(this.props.scrollSize);
		var availableLength = this.$scroller[this.props.size]();

		return totalLength > availableLength;
	};

	this.Control.prototype.scrollLeft = function ScrollerControl$scrollLeft(value)
	{
		if (!isNaN(value) && value >= 0)
			this.$scroller.prop("scrollLeft", value);

		return this.$scroller.prop("scrollLeft");
	};

	this.Control.prototype.scrollTop = function ScrollerControl$scrollTop(value)
	{
		if (!isNaN(value) && value >= 0)
			this.$scroller.prop("scrollTop", value);

		return this.$scroller.prop("scrollTop");
	};

	this.Control.prototype.scrollSize = function ScrollerControl$scrollSize(value)
	{
		if (!isNaN(value) && value > 0)
		{
			this.$content.css(this.props.size, value);
			redrawInstance(this);
		}

		return this.$content.prop(this.props.scrollSize);
	};

	this.Control.prototype.scrollMax = function ScrollerControl$scrollMax()
	{
		return this.scrollSize() - this.$content[this.props.size]();
	};

	this.Control.prototype.scrollPos = function ScrollerControl$scrollPos(value)
	{
		if (this.enabled && !isNaN(value))
			this.$scroller.prop(this.props.scrollPos, value);

		return this.$scroller.prop(this.props.scrollPos);
	};

	function redrawInstance(instance)
	{
		var totalLength = instance.$scroller.prop(instance.props.scrollSize);
		var availableLength = instance.$scroller[instance.props.size]();

		var trackSize = instance.$track[instance.props.size]();
		var gripSize = instance.$grip[instance.props.size]();

		if (totalLength > availableLength)
		{
			if (instance.settings.sizing == sizing.PROPORTIONAL)
			{
				var scrollAreaLength = trackSize - (2 * instance.offset);
				instance.scrollRatio = scrollAreaLength / totalLength;

				gripSize = Math.round(availableLength * instance.scrollRatio);
				instance.$grip.css(instance.props.size, gripSize);
			}
			else
			{
				var scrollAreaLength = totalLength - availableLength;
				instance.scrollRatio = (availableLength - gripSize) / scrollAreaLength;
			}

			instance.minScroll = instance.offset;
			instance.maxScroll = trackSize - gripSize - instance.offset;
			if (instance.enabled)
			{
				instance.$track.show();
			}
		}
		else
		{
			instance.$track.hide();
		}
	};

	function onGripMouseDown(e)
	{
		aeon.drag.on("move", this.onDragMove);
		aeon.drag.on("stop", this.onDragEnd);

		var specs = {
			moveX: this.settings.orientation == orientation.HORIZONTAL,
			moveY: this.settings.orientation == orientation.VERTICAL,
			minX: this.minScroll,
			minY: this.minScroll,
			maxX: this.maxScroll,
			maxY: this.maxScroll
		};

		this.$element.addClass("engaged");

		aeon.drag.start(e, this.$grip, specs);
		this.fireEvent("dragstart");
		return false;
	};

	function onDragMove(event)
	{
		scrollContentFromGripDrag(this);
	};

	function onDragEnd(event)
	{
		aeon.drag.off("move", this.onDragMove);
		aeon.drag.off("stop", this.onDragEnd);

		this.$element.removeClass("engaged");
		this.fireEvent("dragend");
	};

	function onScroll(e)
	{
		positionGripFromScrolling(this);
		this.fireEvent("scroll");
	};

	function onMouseOver(e)
	{
		for (var i = 0; i < scrollers.length; i++)
		{
			scrollers[i].$element.removeClass("hover");
		}
		this.$element.addClass("hover");
	}

	function onMouseOut(e)
	{
		this.$element.removeClass("hover");
	}

	function onMouseWheel(e)
	{
		if (!this.enabled)
			return;

		var lines = -e.originalEvent.wheelDelta / 40;
		scrollBy(this, lines);
		positionGripFromScrolling(this);
	}

	function onDOMMouseScroll(e)
	{
		if (!this.enabled)
			return;

		var lines = e.originalEvent.detail;
		scrollBy(this, lines);
		positionGripFromScrolling(this);
	}

	function scrollBy(instance, countLines)
	{
		var scrollPos = instance.$scroller.prop(instance.props.scrollPos);

		if (instance.settings.animate)
		{
			countLines *= 3;
			scrollPos += countLines * lineHeight;

			var props = {};
			props[instance.props.scrollPos] = scrollPos;
			instance.$scroller.stop().animate(props, { easing: "easeOutExpo" });
		}
		else
		{
			scrollPos += countLines * lineHeight;
			instance.$scroller.prop(instance.props.scrollPos, scrollPos);
		}

		instance.fireEvent("scroll");
	}

	function scrollContentFromGripDrag(instance)
	{
		var gripPos = instance.$grip.position()[instance.props.position];
		var targetPos = Math.round(gripPos / instance.scrollRatio);
		instance.$scroller.prop(instance.props.scrollPos, targetPos);
	};

	function positionGripFromScrolling(instance)
	{
		var scrollPos = instance.$scroller.prop(instance.props.scrollPos);
		var targetPos = Math.round(scrollPos * instance.scrollRatio);

		instance.$grip.css(instance.props.position,
			Math.min(Math.max(instance.minScroll, targetPos), instance.maxScroll));
	};

});