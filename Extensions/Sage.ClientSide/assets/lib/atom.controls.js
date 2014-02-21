/**
 * Implements a simple scroller control.
 */
atom.controls.Scroller = atom.controls.register(new function Scroller()
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
			redrawInstance(scroller.instances[i]);
	};

	/**
	 * Defines the settings of the <c>Tooltip</c> control.
	 * @constructor
	 * @extends {atom.Settings}
	 * @param {Object} data The object with initial settings.
	 * @param {Object} element The object with overriding settings. If a setting exist both in
	 * <c>data</c> and in <c>override</c> objects, the setting from <c>override</c> takes precedence.
	 */
	this.Settings = atom.Settings.extend(function (element, settings)
	{
		var sizingValue = this.getString("sizing", element, settings, "").toLowerCase();
		this.sizing = sizingValue == "fixed" ? sizing.FIXED : sizing.PROPORTIONAL;
		this.animate = this.getBoolean("animate", element, settings, true);
		this.orientation = $(element).hasClass("horizontal") ? orientation.HORIZONTAL : orientation.VERTICAL;
	});

	/**
	 * Implements a custom scroller control.
	 * @extends {atom.HtmlControl}
	 * @event scroll
	 * @event dragstart
	 * @event dragend
	 */
	this.Control = atom.HtmlControl.extend(function ScrollerControl(element, settings)
	{
		this.construct(element, "scroll", "dragstart", "dragend");

		this.settings = new scroller.Settings(element, settings);
		this.props = props[this.settings.orientation];

		this.$element.html(atom.string.format(
			"<div class='atom-scroller'><div class='atom-scrollcontent'>{0}</div></div>"+
			"<div class='atom-scrolltrack'><div class='atom-scrollgrip'/></div>",
				this.$element.html()));

		atom.controls.update(this.$element);

		this.$scroller = this.$element.find("> .atom-scroller");
		this.$content = this.$scroller.find("> .atom-scrollcontent");
		this.$track = this.$element.find("> .atom-scrolltrack");
		this.$grip = this.$track.find("> .atom-scrollgrip");

		this.$grip.on("mousedown", $.proxy(onGripMouseDown, this));
		this.$scroller.on("scroll", $.proxy(onScroll, this));

		this.onDragMove = $.proxy(onDragMove, this);
		this.onDragEnd = $.proxy(onDragEnd, this);

		this.offset = parseInt(this.$grip.css(this.props.position)) || 0;
		this.enabled = true;
		this.overflow = this.$scroller.css("overflow");

		this.switches =
		{
			HOT: { id: 0, duration: 2000 },
			ENGAGED: { id: 0, duration: 500 }
		};

		if (this.settings.orientation == orientation.HORIZONTAL)
		{
			var totalSize = 0;
			this.$content.find("> *").each(function(i, element)
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

		this.$track
			.on("mouseenter", $.proxy(onScrollTrackMouseOver, this))
			.on("mouseleave", $.proxy(onScrollTrackMouseOut, this));

		scroller.registerInstance(this);
		redrawInstance(this);

		this.enable();
	});

	this.Control.prototype.enable = function scroller$enable()
	{
		this.enabled = true;
		redrawInstance(this);
		if (this.isScrollable())
		{
			this.$scroller.css("overflow", atom.string.EMPTY);
			this.$track.show();
		}
	};

	this.Control.prototype.disable = function scroller$disable()
	{
		this.enabled = false;
		this.$track.hide();
		this.$scroller.css("overflow", "hidden");
	};

	this.Control.prototype.isScrollable = function scroller$isScrollable()
	{
		var totalLength = this.$scroller.prop(this.props.scrollSize);
		var availableLength = this.$scroller[this.props.size]();

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

	this.Control.prototype.scrollSize = function scroller$scrollSize(value)
	{
		if (!isNaN(value) && value > 0)
		{
			this.$content.css(this.props.size, value);
			redrawInstance(this);
		}

		return this.$content.prop(this.props.scrollSize);
	};

	this.Control.prototype.scrollMax = function scroller$scrollMax()
	{
		return this.scrollSize() - this.$content[this.props.size]();
	};

	this.Control.prototype.scrollPos = function scroller$scrollPos(value)
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
				scrollAreaLength = totalLength - availableLength;
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
	}

	function onGripMouseDown(e)
	{
		atom.drag.on("move", this.onDragMove);
		atom.drag.on("stop", this.onDragEnd);

		var specs = {
			moveX: this.settings.orientation == orientation.HORIZONTAL,
			moveY: this.settings.orientation == orientation.VERTICAL,
			minX: this.minScroll,
			minY: this.minScroll,
			maxX: this.maxScroll,
			maxY: this.maxScroll
		};

		atom.drag.start(e, this.$grip, specs);
		this.fireEvent("dragstart");
		return false;
	}

	function onDragMove()
	{
		scrollContentFromGripDrag(this);
	}

	function onDragEnd()
	{
		atom.drag.off("move", this.onDragMove);
		atom.drag.off("stop", this.onDragEnd);

		this.fireEvent("dragend");
	}

	function onScroll()
	{
		switchOn(this, "ENGAGED", true);
		positionGripFromScrolling(this);
		this.fireEvent("scroll");
	}

	function onScrollTrackMouseOver()
	{
		switchOn(this, "HOT");
	}

	function onScrollTrackMouseOut()
	{
		delayOff(this, "HOT");
	}

	function onMouseOver(e)
	{
		if (!this.isScrollable())
			return;

		var parent = this.$element.parent();
		while (parent.length != 0)
		{
			var instance = scroller.get(parent);
			if (instance != null)
			{
				e.stopPropagation();
				parent.removeClass(className.ENGAGED);
				parent.removeClass(className.HOVER);
				parent.removeClass(className.HOT);
			}

			parent = parent.parent();
		}

		this.$element.addClass(className.HOVER);
		this.$element.addClass(className.HOT);

		delayOff(this, "HOT");
	}

	function onMouseOut()
	{
		if (!this.isScrollable())
			return;

		this.$element.removeClass(className.ENGAGED);
		this.$element.removeClass(className.HOT);
		this.$element.removeClass(className.HOVER);
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
	}

	function positionGripFromScrolling(instance)
	{
		var scrollPos = instance.$scroller.prop(instance.props.scrollPos);
		var targetPos = Math.round(scrollPos * instance.scrollRatio);

		instance.$grip.css(instance.props.position,
			Math.min(Math.max(instance.minScroll, targetPos), instance.maxScroll));
	}

	function switchOn(instance, name, autoOff)
	{
		cancelOff(instance, name);
		instance.$element.addClass(className[name]);
		if (autoOff)
		{
			delayOff(instance, name, true);
		}
	}

	function switchOff(instance, name)
	{
		instance.$element.removeClass(className[name]);
	}

	function delayOff(instance, name)
	{
		var executeOff = function executeOff()
		{
			switchOff(instance, name);
		};

		cancelOff(instance, name);
		instance.switches[name].id = setTimeout(executeOff, instance.switches[name].duration);
	}

	function cancelOff(instance, name)
	{
		clearTimeout(instance.switches[name].id);
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
		updateControls(this);
		slider.registerInstance(this);
	});

	this.Control.prototype.reset = function slider$reset()
	{
		this.value(this.settings.defaultValue);
	};

	this.Control.prototype.value = function slider$value(value)
	{
		var pos = parseInt(this.$handle.css(this.props.pos));
		var current = this.settings.layout == layout.INVERTED
			? this.settings.minValue + ((this.maxPos - pos) * this.ratio)
			: this.settings.minValue + (pos * this.ratio);

		if (atom.type.isNumeric(value))
		{
			value = Math.max(Math.min(parseFloat(value), this.settings.maxValue), this.settings.minValue);
			if (value != current)
			{
				// this takes into account the range between min and max values
				var converted = value - this.settings.minValue;
				converted = this.settings.layout == layout.INVERTED
					? this.maxPos - converted
					: converted;

				pos = Math.round(converted / this.ratio);
				this.$handle.css(this.props.pos, pos);
			}

			updateControls(this);
			this.fireEvent("change");
		}
		else
		{
			value = current;
		}

		return this.settings.roundValues ? Math.round(value) : value;
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
			if (instance.settings.layout == layout.INVERTED)
				instance.$handle.css(instance.props.pos, instance.maxPos);

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

atom.controls.Thumbstrip = atom.controls.register(new function Thumbstrip()
{
	var MIN_DIF_MOVE = 10;
	var orientation = { VERTICAL: 0, HORIZONTAL: 1 };

	var PROPS =
	{
		"0":
		{
			scrollSize: "scrollHeight", outerSize: "outerHeight", size: "height", position: "top",
			offsetPos: "offsetTop", minPos: "minY", maxPos: "maxY", paddingStart: "paddingTop", paddingEnd: "paddingBottom",
			marginStart: "marginTop"
		},
		"1":
		{
			scrollSize: "scrollWidth", outerSize: "outerWidth", size: "width", position: "left",
			offsetPos: "offsetLeft", minPos: "minX", maxPos: "maxX", paddingStart: "paddingLeft", paddingEnd: "paddingRight",
			marginStart: "marginLeft"
		}
	};

	var thumbstrip = this;

	this.NAME = "atom.controls.Thumbstrip";

	/**
	 * This control doesn't need asynchronous initialization.
	 * @type {Boolean}
	 */
	this.async = false;

	/**
	 * The css expression of elements that should be initialized as thumbstrips.
	 * @type {String}
	 */
	this.expression = ".thumbstrip";

	/**
	 * Prepares the thumbstrip control
	 */
	this.setup = function setup()
	{
		$(window).on("resize", this.redraw);
	};

	this.redraw = function redraw()
	{
		for (var i = 0; i < thumbstrip.instances.length; i++)
			thumbstrip.instances[i].redraw();
	};

	/**
	 * Defines the settings of the <c>Thumbstrip</c> control.
	 * @constructor
	 * @extends {atom.Settings}
	 * @param {Object} data The object with initial settings.
	 * @param {Object} element The object with overriding settings. If a setting exist both in
	 * <c>data</c> and in <c>override</c> objects, the setting from <c>override</c> takes precedence.
	 */
	this.Settings = atom.Settings.extend(function (element, settings)
	{
		this.orientation = $(element).hasClass("vertical") ? orientation.VERTICAL : orientation.HORIZONTAL;
		this.extraLength = this.getNumber("extra-length", element, settings, 0);
		this.useAnimations = this.getBoolean("use-animations", element, settings, true);
		this.autoScroll = this.getBoolean("auto-scroll", element, settings, true);
		this.mouseDraggable = this.getBoolean("draggable", element, settings, false);
		this.snapToItems = this.getBoolean("snap-to-items", element, settings, false);
		this.scrollOnClick = this.getBoolean("scroll-on-click", element, settings, false);
	});

	/**
	 * Defines the settings of the <c>Thumbstrip</c> control.
	 * @constructor
	 * @extends {atom.HtmlControl}
	 */
	this.Control = atom.HtmlControl.extend(function (element, settings)
	{
		this.construct(element, "move", "stop", "select");

		this.settings = new thumbstrip.Settings(element, settings);
		this.props = PROPS[this.settings.orientation];

		this.$element.css({ position: "relative", overflow: "hidden" });
		if (this.settings.orientation == orientation.HORIZONTAL)
			this.$element.css({ whiteSpace: "nowrap" });

		this.$children = this.$element.children("*");
		this.$container = this.$element.parent();

		this.moveEngaged = false;
		this.animEngaged = false;
		this.wasDragged = false;

		this.stripLength = 0;
		this.currentIndex = 0;

		this.minMouseX = 0;
		this.minMouseY = 0;
		this.maxMouseX = this.$container.outerWidth();
		this.maxMouseY = this.$container.outerHeight();

		this.startPos = this.$element.position()[this.props.position];

		this.animSettings = {
			complete: $.proxy(stopAnimation, this),
			step: $.proxy(adjustAnimation, this),
			duration: 400,
			easing: "easeOutExpo"
		};

		this.$element.on("mousedown touchstart", $.proxy(onElementMouseDown, this));
		this.$element.on("click touchend", $.proxy(onElementClick, this));

		if (!atom.dom.isTouchDevice)
		{
			$(document).on("mousemove", $.proxy(onDocumentMouseMove, this));
		}

		this.$children.bind("click touchend", $.proxy(onChildClicked, this));
		this.$element.on("mousewheel DOMMouseScroll", $.proxy(onMouseWheel, this));
		this.redraw();
	});

	this.Control.prototype.redraw = function redraw(maintainPosition)
	{
		var containerLength = this.$container[this.props.size]();
		var stripLength =
			(parseInt(this.$element.css(this.props.paddingStart)) || 0) +
			(parseInt(this.$element.css(this.props.paddingEnd)) || 0);

		this.$children = this.$element.children("*");
		for (var i = 0; i < this.$children.length; i++)
		{
			var child = this.$children.eq(i);
			stripLength += child[this.props.outerSize](true);
		}

		this.minMouseX = 0;
		this.minMouseY = 0;
		this.maxMouseX = this.$container.outerWidth();
		this.maxMouseY = this.$container.outerHeight();

		this.containerLength = containerLength;
		this.stripLength = stripLength;

		this.maxX = 0;
		this.maxY = 0;
		this.minX = (this.containerLength - this.stripLength) - (parseInt(this.$element.css(this.props.paddingStart)) || 0);
		this.minY = this.containerLength - this.stripLength;

		var trackLength = (stripLength - containerLength)
			+ this.startPos
			+ this.settings.extraLength * 2;

		this.ratio = trackLength / containerLength;
		this.stripLength = stripLength + this.settings.extraLength;

		this.$element.css(this.props.size, stripLength);
		this.snapCoords = getSnapCoords(this);

		if (stripLength < containerLength)
		{
			this.$element.css(this.props.size, containerLength);
			this.$element.css(this.props.position, 0);
		}
		else
		{
			var current = parseInt(this.$element.css(this.props.position)) || 0;
			if (maintainPosition)
				current = -this.snapCoords[this.currentIndex];

			var minPos = this[this.props.minPos];
			var maxPos = this[this.props.maxPos];

			var target = Math.min(Math.max(current, minPos), maxPos);
			this.$element.css(this.props.position, target);
		}

	};

	function adjustAnimation(now, tween)
	{
		tween.end = this.targetPos;
	}

	function stopAnimation()
	{
		this.animEngaged = false;
	}

	function getNearestIndex(instance, accel)
	{
		if (accel < 0 && instance.currentIndex == instance.$children.length - 1)
			return instance.currentIndex;
		if (accel > 0 && instance.currentIndex == 0)
			return instance.currentIndex;

		var minDiff = Number.POSITIVE_INFINITY;
		var nearestIndex = -1;

		var elemPos = -instance.$element.position()[instance.props.position];
		for (var i = 0; i < instance.snapCoords.length; i++)
		{
			var itemDiff = Math.abs(elemPos - instance.snapCoords[i]);
			if (itemDiff < minDiff)
			{
				minDiff = itemDiff;
				nearestIndex = i;
			}
		}

		if (accel > 0 && nearestIndex >= instance.currentIndex)
			nearestIndex = instance.currentIndex - 1;

		if (accel < 0 && nearestIndex <= instance.currentIndex)
			nearestIndex = instance.currentIndex + 1;

		return nearestIndex;
	}

	function getSnapCoords(instance)
	{
		var coords = [];

		var offset = parseInt(instance.$element.css(instance.props.paddingStart));
		for (var i = 0; i < instance.$children.length; i++)
		{
			var $c = instance.$children.eq(i);
			var itemPos = instance.$children[i][instance.props.offsetPos];
			var itemSpacing = parseInt($c.css(instance.props.marginStart)) || 0;

			var finalPos = itemPos - (offset + itemSpacing);
			coords.push(finalPos);
		}

		return coords;
	}

	function onChildClicked(e)
	{
		if (this.wasDragged)
			return;

		var $el = $(e.target);
		while (!$el.parent().is(this.$element))
		{
			$el = $el.parent();
			if ($el.length == 0)
				return;
		}

		if (this.settings.scrollOnClick)
		{
			scrollToSlide(this, $el.index(), true);
		}
	}

	function onMouseWheel(e)
	{
		if (this.animEngaged)
			return;

		var amount = -e.originalEvent.wheelDelta || e.originalEvent.detail;
		if (amount > 0 && this.currentIndex < this.snapCoords.length - 1)
		{
			scrollToSlide(this, this.currentIndex + 1);
		}
		if (amount < 0 && this.currentIndex > 0)
		{
			scrollToSlide(this, this.currentIndex - 1);
		}
	}

	function onDocumentMouseMove(e)
	{
		if (atom.drag.active)
			return;

		var outerSize = this.$container[this.props.outerSize]();
		if (!this.settings.autoScroll || outerSize == 0)
			return;

		var html = $("html");
		var offset = this.$container.offset();
		var mouseX = (e.clientX - offset.left) + html.prop("scrollLeft");
		var mouseY = (e.clientY - offset.top) + html.prop("scrollTop");

		this.moveEngaged = (this.stripLength > outerSize) &&
			(mouseX >= this.minMouseX && mouseX <= this.maxMouseX &&
			 mouseY >= this.minMouseY && mouseY <= this.maxMouseY);

		if (!this.moveEngaged)
			return;

		var mousePos = this.settings.orientation == orientation.VERTICAL ? mouseY : mouseX;
		var targetPos = Math.round(-(mousePos * this.ratio) + this.settings.extraLength);
		var currentPos = parseInt(this.$element.css(this.props.position));
		var animEngaged = Math.abs(currentPos - targetPos) > MIN_DIF_MOVE;

		this.targetPos = targetPos;

		if (this.settings.useAnimations && (animEngaged && !this.animEngaged))
		{
			var props = {};
			props[this.props.position] = targetPos;

			this.$element.animate(props, this.animSettings);
			this.animEngaged = true;
		}

		if (!this.settings.useAnimations || !this.animEngaged)
		{
			this.$element.css(this.props.position, targetPos);
		}
	}

	function onElementMouseDown(e)
	{
		if (!this.settings.mouseDraggable || this.animEngaged)
			return true;

		this.wasDragged = false;

		if (this.stripLength <= this.containerLength)
			return true;

		if (atom.dom.isTouchDevice && e.originalEvent.touches.length != 1)
			return true;

		var options = {
			moveX: this.settings.orientation == orientation.HORIZONTAL,
			moveY: this.settings.orientation == orientation.VERTICAL,
			maxX: 0,
			maxY: 0,
			minX: this.minX,
			minY: this.minY
		};

		this.endListener = $.proxy(onElementDragEnd, this);
		this.moveListener = $.proxy(onElementDragMove, this);

		atom.drag.on("stop", this.endListener);
		atom.drag.on("move", this.moveListener);

		return atom.drag.start(e, this.$element, options);
	}

	function onElementClick(e)
	{
		if (this.settings.mouseDraggable && this.wasDragged)
			return atom.event.cancel(e);

		return true;
	}

	function onElementDragMove()
	{
		this.wasDragged = true;
		this.fireEvent("move");
	}

	function onElementDragEnd()
	{
		if (!this.wasDragged)
			return;

		var accel = this.settings.orientation == orientation.HORIZONTAL
			? e.data.specs.accelX
			: e.data.specs.accelY;

		if (this.settings.snapToItems)
		{
			var nearestIndex = getNearestIndex(this, accel);
			scrollToSlide(this, nearestIndex);
		}
		else
		{
			this.fireEvent("stop");
		}

		atom.drag.off("stop", this.endListener);
	}

	function scrollToSlide(instance, index, fireSelect)
	{
		if (this.animEngaged)
			return;

		var props = {};
		var minPos = instance[instance.props.minPos];
		var maxPos = instance[instance.props.maxPos];

		var targetCoord = Math.min(Math.max(-instance.snapCoords[index], minPos), maxPos);
		props[instance.props.position] = targetCoord;

		if (instance.$element.position()[instance.props.position] == targetCoord)
		{
			instance.fireEvent("select");
			return;
		}

		var settings = { duration: 500, easing: "easeOutExpo" };
		settings.done = function onSnapComplete()
		{
			instance.animEngaged = false;
			instance.currentIndex = index;
			instance.fireEvent("stop");

			if (fireSelect)
			{
				instance.fireEvent("select");
			}
		};
		settings.progress = function onAnimationProgress()
		{
			instance.fireEvent("move");
		};

		instance.$element.stop().animate(props, settings);
		instance.animEngaged = true;
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

	var tooltip = this;
	var tooltipInstance = null;
	var tooltipPopup = null;
	var tooltipArrow = null;
	var tooltipDocument = null;
	var tooltipContent = null;
	var windowTop = 0;
	var windowLeft = 0;
	var timeoutId;

	this.NAME = "atom.controls.Tooltip";

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
	this.Settings = atom.Settings.extend(function Settings(data, override)
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
	 * @param {HTMLElement} element The HTML element that shows the tooltip popup.
	 * @param {Settings} [settings] Optional settings to use with this instance.
	 */
	this.Control = atom.HtmlControl.extend(function Tooltip(element, settings)
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
		this.$element.attr("title", atom.string.EMPTY);

		var maxWidth = this.settings.maxWidth || $(tooltipDocument).width() * .75;
		tooltipPopup.css("max-width", maxWidth + "px");
		tooltipContent.html(title);

		var orientation = this.settings.orientation;
		var orientationP = orientation[0].match(/T|L|R|B/) ? orientation[0] : "T";
		var orientationS = orientation[1].match(/T|L|R|B|C/) ? orientation[1] : "L";

		var html = $("html", tooltipDocument)[0];
		var body = $("body", tooltipDocument)[0];
		var bodyWidth = html.scrollWidth || body.scrollWidth;
		var bodyHeight = html.scrollHeight || body.scrollHeight;

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
			primary = getPrimaryLeft(elementLeft, elementWidth, orientationP, 0, bodyWidth, offset);
			secondary = getSecondaryTop(elementTop, elementHeight, orientationS, 0, bodyHeight);

			specs = {
				popupLeft: primary.left,
				popupTop: secondary.top,
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
	}

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
				difference = edgeL - result.left;
				result.left += difference;
				result.arrowLeft -= difference;
			}
		}

		if (elementWidth < (arrowWidth * 3))
		{
			difference = (arrowWidth * 1.5) - elementWidth / 2;
			if (orientation == "L")
				result.left -= difference;
			else
				result.left += difference;
		}

		return result;
	}

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
			result.top = absoluteTop + elementHeight + offset;
			result.orientation = "B";
			result.arrowTop = 0;
			result.arrowBottom = "auto";
		}
		else if ((result.top + popupHeight) > edgeB)
		{
			result.top = absoluteTop - popupHeight - offset;
			result.orientation = "T";
			result.arrowTop = "auto";
			result.arrowBottom = 0;
		}

		return result;
	}

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
				difference = edgeT - result.top;
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
	}

	function tooltipOnMouseOver()
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
	}

	function tooltipOnMouseOut()
	{
		window.clearTimeout(timeoutId);

		if (tooltipInstance != this)
			return;

		if (!this.settings.hideOn.contains("mouseout"))
			return;

		this.hide();
	}

	function tooltipOnFocus()
	{
		if (this.settings.showOn.contains("focus"))
			this.show();
	}

	function tooltipOnBlur()
	{
		if (this.settings.hideOn.contains("blur"))
			this.hide();
	}

	function tooltipOnClick()
	{
		if (tooltipPopup.is(":visible") && this.settings.hideOn.contains("click"))
			this.hide();

		else if (!tooltipPopup.is(":visible") && this.settings.showOn.contains("click"))
			this.show();
	}

	function tooltipOnDocumentEvent(e)
	{
		if (!tooltipPopup.is(":visible") || tooltipInstance != this)
			return;

		if (!jQuery.contains(this.$element[0], e.target) && !jQuery.contains(tooltipPopup[0], e.target))
			this.hide();
	}

	function jqueryTooltip(settings)
	{
		return this.each(function initializeInstance(i, element)
		{
			initializeSingleElement(element, settings);
		});
	}

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
	}

	function createPopupLayer()
	{
		var hlayer = $(tooltipDocument.getElementById(POPUP_ELEMENT_ID));
		if (hlayer.length != 0)
		{
			tooltipPopup = hlayer;
		}
		else
		{
			var tooltipHtml = atom.string.format(
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

				var styleText = [];

				var tooltipRules = atom.css.findRules("#tooltip");
				for (var i = 0; i < tooltipRules.length; i++)
				{
					var rule = tooltipRules[i];
					styleText.push(rule.css.cssText);
				}

				styleElement.html(styleText.join("\n"));
			}
		}

		tooltipContent = tooltipPopup.find(".content");
		tooltipArrow = tooltipPopup.find(".arrow");
	}
});


