Type.registerNamespace("aeon.controls");

/**
 * Implements a simple slider control.
 * @constructor
 * @param {HtmlElement} elem The HTMLElement that respresents this control.
 * @event onchange Occurs when the value is changed, either programmatically or by moving the slider manually.
 * @event onreset Occurs when the value is reset to the default value.
 * @event ondragcomplete Occurs at the end of a drag move operation
 */
aeon.controls.Slider = function Slider(elem, settings)
{
	$assert.isHtmlElement(elem);

	this.$super(elem, "onchange", "onreset", "ondragcomplete");

	this.settings = new aeon.controls.SliderSettings(settings, elem);

	var value = 0;
	var minValue = 0;
	var maxValue = 100;

	this.element.html(String.EMPTY);

	this.formControl = document.getElementById(this.settings.formControl);
	this.textControl = document.getElementById(this.settings.textControl);
	this.defaultValue = null;

	this.orientation = this.settings.orientation;
	this.roundValues = this.settings.roundValues;
	this.minValue = this.settings.minValue;
	this.maxValue = this.settings.maxValue;
	this.value = this.settings.value;

	this.defaultValue = this.settings.defaultValue;

	var handleElement = elem.appendChild(document.createElement("DIV"));

	this.handle = new aeon.controls.SliderHandle(handleElement, this);
	this.handle.addListener("ondblclick", Function.createDelegate(this, this.onHandleDblClick)); /**/
	this.handle.addListener("ondragmove", Function.createDelegate(this, this.onHandleDragMove)); /**/

	if (isNaN(this.defaultValue))
		this.defaultValue = null;

	if (this.formControl != null)
	{
		this.value = this.formControl.value;
		$evt.addListener(this.formControl, "onchange", Function.createDelegate(this, this.onchange)); /**/
	}

	this.addListener("onchange", Function.createDelegate(this, this.updateControlValue)); /**/
	this.addListener("onchange", Function.createDelegate(this, this.updateTextControlValue)); /**/

	this.redraw();
	this.setValue(this.value);

	$dom.makeUnselectable(this.element);
};
aeon.controls.Slider.inherits(aeon.controls.HtmlControl);

aeon.controls.Slider.setup = function Slider$setup()
{
	aeon.controls.Slider.$sliders = [];
	aeon.controls.Slider.$elements = [];

	$evt.addListener(window, "onresize", aeon.controls.Slider.redraw);
};

aeon.controls.Slider.redraw = function Slider$redraw()
{
	if (Type.isArray(aeon.controls.Slider.$sliders))
	{
		for (var i = 0; i < aeon.controls.Slider.$sliders.length; i++)
			aeon.controls.Slider.$sliders[i].redraw();
	}
};

aeon.controls.Slider.dispose = function Slider$dispose()
{
	if (Type.isArray(aeon.controls.Slider.$sliders))
		while (aeon.controls.Slider.$sliders.length)
			aeon.controls.Slider.$sliders.shift().dispose();

	if (Type.isArray(aeon.controls.Slider.$elements))
		while (aeon.controls.Slider.$elements.length)
			aeon.controls.Slider.$elements.shift();
};

aeon.controls.Slider.createElement = function Slider$createElement(settings)
{
};

aeon.controls.Slider.prototype.reset = function Slider$reset()
{
	if (this.defaultValue != null)
	{
		this.setValue(this.defaultValue);
		this.fireEvent("onchange");
		this.fireEvent("onreset");
	}
};

aeon.controls.Slider.prototype.redraw = function Slider$redraw()
{
	var sliderHeight = this.element.height();
	var sliderWidth = this.element.width();

	if (sliderHeight == 0 && sliderWidth == 0)
		return;

	var sliderBorderH =
		parseInt(this.element.css("borderLeftWidth")) || 0 +
		parseInt(this.element.css("borderRightWidth")) || 0;

	var sliderBorderV =
		parseInt(this.element.css("borderTopWidth")) || 0 +
		parseInt(this.element.css("borderBottomWidth")) || 0;

	var handleBorderH =
		parseInt(this.element.css("borderLeftWidth")) || 0 +
		parseInt(this.element.css("borderRightWidth")) || 0;

	var handleBorderV =
		parseInt(this.element.css("borderTopWidth")) || 0 +
		parseInt(this.element.css("borderRightWidth")) || 0;

	var handleHeight = this.handle.getElement().offsetHeight + handleBorderH;
	var handleWidth = this.handle.getElement().offsetWidth + handleBorderV;

	var startTop = -Math.round(handleHeight / 2);
	var startLeft = -Math.round(handleWidth / 2);

	if (this.handle.orientation == "h")
		startTop += Math.round((sliderHeight - sliderBorderV - handleBorderV) / 2);
	else
		startLeft += Math.round((sliderWidth - sliderBorderH - handleBorderH) / 2);

	this.handle.setTop(startTop);
	this.handle.setLeft(startLeft);

	this.minY = startTop;
	this.maxY = startTop;
	this.minX = startLeft;
	this.maxX = startLeft;

	if (this.handle.orientation == "h")
		this.maxX = startLeft + sliderWidth;
	else
		this.maxY = startTop + sliderHeight;

	if (this.formControl != null)
		this.setValue(this.formControl.value);
	else
		this.setValue(this.value);
};

aeon.controls.Slider.prototype.dispose = function Slider$dispose()
{
	if (this.formControl != null)
	{
		this.formControl.onchange = null;
		this.formControl = null;
	}
	this.handle.dispose();
	this.handle = null;
	this.element = null;
};

aeon.controls.Slider.prototype.setMinValue = function Slider$setMinValue(value)
{
	var current = this.getValue();

	this.minValue = value;
	this.redraw();

	if (current < this.minValue)
		this.setValue(this.minValue);
	else
		this.setValue(current);
};

aeon.controls.Slider.prototype.setMaxValue = function Slider$setMaxValue(value)
{
	var current = this.getValue();

	this.maxValue = value;
	this.redraw();

	if (current > this.maxValue)
		this.setValue(this.maxValue);
	else
		this.setValue(current);
};

/**
 * Sets the slider's handle x coordinate, relative to the value.
 * @param {Number} value The value to set
 */
aeon.controls.Slider.prototype.setValue = function Slider$setValue(value)
{
	this.value = value;

	if (this.minValue != null && value < this.minValue)
		this.value = this.minValue;
	if (this.maxValue != null && value > this.maxValue)
		this.value = this.maxValue;

	var minCoord = this.orientation == "v" ? this.minY : this.minX;
	var maxCoord = this.orientation == "v" ? this.maxY : this.maxX;
	var setCoord = Function.createDelegate(this.handle, this.orientation == "v" ? "setTop" : "setLeft"); /**/

	var steps = maxCoord - minCoord;
	var diff = this.maxValue - this.minValue;
	var unit = steps / diff;

	var zeroPos = this.minValue == 0
		? minCoord
		: this.minValue < 0
			? maxCoord - Math.round(Math.abs(this.minValue) * unit)
			: maxCoord + Math.round(Math.abs(this.minValue) * unit);

	if (this.minValue == this.maxValue)
	{
		if (this.value == 0)
		{
			setCoord(minCoord);
		}
		else
		{
			setCoord(maxCoord);
		}
	}
	else
	{
		var targetPos = zeroPos + Math.round(this.value * unit);
		setCoord(targetPos);
	}

	this.updateControlValue();
	this.updateTextControlValue();
};

/**
 * Calculates the value from the slider's handle x coordinate.
 * @return Number The calculated value
 */
aeon.controls.Slider.prototype.getValue = function Slider$getValue()
{
	if (this.offsetHeight == 0)
		return this.formControl ? this.formControl.value : this.value;

	if (this.orientation == "v")
	{
		var steps = this.maxY - this.minY;
		var diff = this.maxValue - this.minValue;
		var unit = diff / steps;

		var posTop = this.maxY - this.handle.getTop();
		var value = this.minValue + posTop * unit;
	}
	else
	{
		var steps = this.maxX - this.minX;
		var diff = this.maxValue - this.minValue;
		var unit = diff / steps;

		var posLeft = this.handle.getLeft() - this.minX;
		var value = this.minValue + posLeft * unit;
	}

	var result = this.roundValues ? Math.round(value) : value;
	return result;
};

aeon.controls.Slider.prototype.updateControlValue = function Slider$updateControlValue()
{
	if (this.formControl)
		this.formControl.value = this.getValue();
};

aeon.controls.Slider.prototype.updateTextControlValue = function Slider$updateTextControlValue()
{
	if (this.textControl != null)
		$dom.innerText(this.textControl, this.getValue());
};

aeon.controls.Slider.prototype.onHandleDblClick = function Slider$onHandleDblClick()
{
	this.reset();
};

aeon.controls.Slider.prototype.onHandleDragMove = function Slider$onHandleDragMove()
{
	var value = this.getValue();
	if (value != this.value)
	{
		this.value = value;
		this.setValue(value);
		this.fireEvent("onchange");
	}
};

aeon.controls.SliderHandle = function SliderHandle(element, slider)
{
	this.$super(element, "onmousedown", "ondblclick", "ondragmove", "ondragstop");

	this.slider = slider;
	this.orientation = slider.orientation;

	this.element.addClass(aeon.controls.SliderHandle.CLASS_NAME);
	this.element.show(this.element);

	this.element.bind("mousedown", Function.createDelegate(this, this.onMouseDown)); /**/
	this.element.bind("dblclick",  Function.createDelegate(this, this.onDblClick)); /**/
};
aeon.controls.SliderHandle.inherits(aeon.controls.HtmlControl);

aeon.controls.SliderHandle.CLASS_NAME = "sliderhandle";

aeon.controls.SliderHandle.prototype.dispose = function SliderHandle$dispose()
{
	this.slider = null;
};

aeon.controls.SliderHandle.prototype.getTop = function SliderHandle$getTop()
{
	return this.element.position().top;
};

aeon.controls.SliderHandle.prototype.getLeft = function SliderHandle$getLeft()
{
	return this.element.position().left;
};

aeon.controls.SliderHandle.prototype.setTop = function SliderHandle$setTop(value)
{
	this.element.css("top", value);
};

aeon.controls.SliderHandle.prototype.setLeft = function SliderHandle$setLeft(value)
{
	this.element.css("left", value);
};

aeon.controls.SliderHandle.prototype.onDblClick = function SliderHandle$onDblClick(e)
{
	this.fireEvent("ondblclick");
};

aeon.controls.SliderHandle.prototype.onMouseDown = function SliderHandle$onMouseDown(e)
{
	var dragV = this.orientation == "v";
	var dragH = this.orientation == "h";

	this.element.addClass("active");

	$drag.addListener("onbeforedragmove", Function.createDelegate(this, this.onBeforeDragMove)); /**/
	$drag.addListener("ondragmove", Function.createDelegate(this, this.onDragMove)); /**/
	$drag.addListener("ondragstop", Function.createDelegate(this, this.onDragStop)); /**/

	$drag.start(e, this.element, dragH, dragV);

	this.fireEvent("onmousedown");
	return $evt.cancel(e);
};

aeon.controls.SliderHandle.prototype.onDragStop = function SliderHandle$onDragStop(e)
{
	$drag.removeListener("onbeforedragmove", Function.getDelegate(this, this.onBeforeDragMove)); /**/
	$drag.removeListener("ondragmove", Function.getDelegate(this, this.onDragMove)); /**/
	$drag.removeListener("ondragstop", Function.getDelegate(this, this.onDragStop)); /**/

	this.element.removeClass("active");

	this.fireEvent("ondragstop");
};

aeon.controls.SliderHandle.prototype.onBeforeDragMove = function SliderHandle$onBeforeDragMove(e)
{
	if (this.orientation == "v")
	{
		var targetY = e.data.targetY;

		if (targetY < this.slider.minY)
			e.data.targetY = this.slider.minY;
		if (targetY > this.slider.maxY)
			e.data.targetY = this.slider.maxY;
	}
	else
	{
		var targetX = e.data.targetX;

		if (targetX < this.slider.minX)
			e.data.targetX = this.slider.minX;
		if (targetX > this.slider.maxX)
			e.data.targetX = this.slider.maxX;
	}
};

aeon.controls.SliderHandle.prototype.onDragMove = function SliderHandle$onDragMove(e)
{
	this.fireEvent("ondragmove");
};

/**
 * @class
 */
aeon.controls.SliderSettings = function SliderSettings(data, override)
{
	this.orientation = this.getString("orientation", data, override, "h");
	this.roundValues = this.getBoolean("roundValues", data, override, true);
	this.minValue = this.getNumber("minValue", data, override, 0);
	this.maxValue = this.getNumber("maxValue", data, override, 100);
	this.value = this.getNumber("value", data, override, 0);
	this.defaultValue = this.getNumber("defaultValue", data, override, 0);
	this.formControl = this.getString("formControl", data, override, null);
	this.textControl = this.getString("textControl", data, override, null);
};
aeon.controls.SliderSettings.inherits(aeon.Settings);

aeon.controls.ControlRegistry.registerControl(aeon.controls.Slider, ".slider");
