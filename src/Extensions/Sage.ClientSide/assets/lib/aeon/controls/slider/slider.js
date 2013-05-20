/**
 * Implements a simple slider control.
 * @constructor
 * @param {HtmlElement} elem The HTMLElement that respresents this control.
 * @event onchange Occurs when the value is changed, either programmatically or by moving the slider manually.
 * @event onreset Occurs when the value is reset to the default value.
 * @event ondragcomplete Occurs at the end of a drag move operation
 */
aeon.controls.Slider = aeon.controls.register(new function Slider()
{
	var slider = this;
	var orientation = { HORIZONTAL: 0, VERTICAL: 1 };
	var layout = { NORMAL: 0, INVERTED: 1 };

	var props =
	{
		"0": { pos: "left", min: "minX", max: "maxX", size: "width", outerSize: "outerWidth" },
		"1": { pos: "top", min: "minY", max: "maxY", size: "height", outerSize: "outerHeight" },
	}

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
	this.Settings = aeon.Settings.extend(function SliderSettings(element, settings)
	{
		this.orientation = $(element).hasClass("vertical") ? orientation.VERTICAL : orientation.HORIZONTAL;
		this.layout = this.orientation == orientation.VERTICAL ? layout.INVERTED : layout.NORMAL;

		this.roundValues = this.getBoolean("roundValues", element, settings, true);
		this.minValue = this.getNumber("minValue", element, settings, 0);
		this.maxValue = this.getNumber("maxValue", element, settings, 100);
		this.value = this.getNumber("value", element, settings);
		this.defaultValue = this.getNumber("defaultValue", element, settings, this.minValue);
		this.textElement = this.getString("textElement", element, settings, aeon.string.EMPTY).trim();
		this.controlElement = this.getString("controlElement", element, settings, aeon.string.EMPTY).trim();

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
	this.Control = aeon.HtmlControl.extend(function Slider(element, settings)
	{
		this.construct(element, "change", "reset");

		this.settings = new slider.Settings(element, settings);
		this.props = props[this.settings.orientation];

		this.ready = false;
		this.$handle = $("<div class='handle'/>");
		this.$element.html(aeon.string.EMPTY);
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

	this.Control.prototype.reset = function Slider$reset()
	{
		this.value(this.settings.defaultValue);
	};

	this.Control.prototype.value = function Slider$value(value)
	{
		var pos = parseInt(this.$handle.css(this.props.pos));
		var current = this.settings.layout == layout.INVERTED
			? this.settings.minValue + ((this.maxPos - pos) * this.ratio)
			: this.settings.minValue + (pos * this.ratio);

		if (aeon.type.isNumeric(value))
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
				return element.find(expression.replace(/^this::/, aeon.string.EMPTY));
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
	};

	function onHandleMouseDown(e)
	{
		aeon.drag.on("move", this.onDragMove);
		aeon.drag.on("stop", this.onDragEnd);

		var specs = {
			moveX: this.settings.orientation == orientation.HORIZONTAL,
			moveY: this.settings.orientation == orientation.VERTICAL,
			minX: this.minPos, maxX: this.maxPos,
			minY: this.minPos, maxY: this.maxPos
		};

		aeon.drag.start(e, this.$handle, specs);
		return false;
	}

	function onDragMove(event)
	{
		updateControls(this);
		this.fireEvent("change");
	};

	function onDragEnd(event)
	{
		aeon.drag.off("move", this.onDragMove);
		aeon.drag.off("stop", this.onDragEnd);
	};

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
