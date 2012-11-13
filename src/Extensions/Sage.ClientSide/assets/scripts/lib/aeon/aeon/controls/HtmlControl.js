Type.registerNamespace("aeon.controls");

/**
 * Provides the base class for html controls.
 * @param {HTMLElement} element The element that represents this control.
 * @arguments {String} [1-n] The names of the events to register with the current instance.
 * @class
 */
aeon.controls.HtmlControl = function HtmlControl(element)
{
	this.$super(Array.fromArguments(arguments, 1));

	/**
	 * @type {jQuery}
	 */
	this.element = $(element);

	/**
	 * @type {String}
	 */
	this.id = this.element.attr("id");
};
aeon.controls.HtmlControl.inherits(aeon.Dispatcher);

aeon.controls.HtmlControl.interface = { setId: 1, getId: 1, getElement: 1 };

aeon.controls.HtmlControl.prototype.setId = function HtmlControl$setId(id)
{
	this.id = id;
	this.element.attr("id", id);
};

aeon.controls.HtmlControl.prototype.getId = function HtmlControl$getId()
{
	return this.id;
};

aeon.controls.HtmlControl.prototype.getElement = function HtmlControl$getElement()
{
	return this.element;
};
