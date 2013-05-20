aeon.controls.register(new function Thumbstrip()
{
	var MIN_DIF_MOVE = 10;
	var orientation = { VERTICAL: 0, HORIZONTAL: 1 };

	var PROPS =
	{
		"0": { scrollSize: "scrollHeight", outerSize: "outerHeight", size: "height", position: "top", touchPos: "pageY" },
		"1": { scrollSize: "scrollWidth", outerSize: "outerWidth", size: "width", position: "left", touchPos: "pageX" }
	};

	var isTouchDevice = ('ontouchstart' in window) || navigator.msMaxTouchPoints;
	var thumbstrip = this;

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
	 * @param {Object} data The object with initial settings.
	 * @param {Object} element The object with overriding settings. If a setting exist both in
	 * <c>data</c> and in <c>override</c> objects, the setting from <c>override</c> takes precedence.
	 */
	this.Settings = aeon.Settings.extend(function ThumbstripSettings(element, settings)
	{
		this.orientation = $(element).hasClass("vertical") ? orientation.VERTICAL : orientation.HORIZONTAL;
		this.extraLength = this.getNumber("extra-length", element, settings, 10);
		this.useAnimations = this.getBoolean("use-animations", element, settings, true);
	});

	this.Control = aeon.HtmlControl.extend(function ThumbstripControl(element, settings)
	{
		this.construct(element);

		this.settings = new thumbstrip.Settings(element, settings);
		this.props = PROPS[this.settings.orientation];

		this.$element.css({ position: "relative", overflow: "hidden" });
		if (this.settings.orientation == orientation.HORIZONTAL)
			this.$element.css({ whiteSpace: "nowrap" });

		this.$container = this.$element.parent();

		this.moveEngaged = false;
		this.animEngaged = false;

		this.stripLength = 0;
		this.trackPos = 0;

		this.minMouseX = 0;
		this.minMouseY = 0;
		this.maxMouseX = this.$container.outerWidth();
		this.maxMouseY = this.$container.outerHeight();

		this.startPos = this.$element.position()[this.props.position];
		this.touchPos = 0;

		this.animSettings = {
			complete: $.proxy(stopAnimation, this),
			step: $.proxy(adjustAnimation, this),
			duration: 400,
			easing: "easeOutExpo"
		};

		if (!isTouchDevice)
			$(document).on("mousemove", $.proxy(onDocumentMouseMove, this));
		else
			this.$element
				.on("touchstart", $.proxy(onTouchStart, this))
				.on("touchmove", $.proxy(onTouchMove, this))
				.on("touchend", $.proxy(onTouchEnd, this));
	});

	this.Control.prototype.redraw = function ThumbstripControl$redraw()
	{
		var containerLength = this.$container[this.props.outerSize]();
		var stripLength = 0;

		var children = this.$element.children();
		for (var i = 0; i < children.length; i++)
			stripLength += children.eq(i)[this.props.outerSize](true);

		this.minMouseX = 0;
		this.minMouseY = 0;
		this.maxMouseX = this.$container.outerWidth();
		this.maxMouseY = this.$container.outerHeight();

		this.containerLength = containerLength;
		this.stripLength = stripLength;

		var trackLength = (stripLength - containerLength)
			+ this.startPos
			+ this.settings.extraLength * 2;

		this.ratio = trackLength / containerLength;
		this.stripLength = stripLength + this.settings.extraLength;

		this.$element.css(this.props.size, stripLength);

		if (stripLength < containerLength)
			this.$element.css(this.props.position, (containerLength / 2) - (stripLength / 2));
		else
			this.$element.css(this.props.position, 0);
	}

	function adjustAnimation(now, tween)
	{
		tween.end = this.targetPos;
	}

	function stopAnimation()
	{
		this.animEngaged = false;
	}

	function onDocumentMouseMove(e)
	{
		var outerSize = this.$container[this.props.outerSize]();
		if (aeon.drag.active || outerSize == 0)
			return false;

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

	function onTouchStart(e)
	{
		if (e.originalEvent.touches.length != 1)
			return;

		this.prevTouchPos = e.originalEvent.touches[0][this.props.touchPos];
		this.movePossible = this.stripLength > this.containerLength;
		this.maxPosition = this.containerLength - this.stripLength;
	}

	function onTouchMove(e)
	{
		if (e.originalEvent.touches.length != 1)
			return;

		if (!this.movePossible)
			return;

		var touchPos = e.originalEvent.touches[0][this.props.touchPos];
		var touchDiff = touchPos - this.prevTouchPos;

		if (!this.moveEngaged && Math.abs(touchDiff) >= MIN_DIF_MOVE)
		{
			this.moveEngaged = true;
		}

		if (this.moveEngaged)
		{
			var currentPos = parseInt(this.$element.css(this.props.position));
			var targetPos = currentPos + touchDiff;
			if (targetPos >= this.maxPosition && targetPos <= 0)
				this.$element.css(this.props.position, targetPos);

			this.prevTouchPos = touchPos;
		}

		return false;
	}

	function onTouchEnd(e)
	{
		if (e.originalEvent.touches.length != 1)
			return;

		this.moveEngaged = false;
	}

});
