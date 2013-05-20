/**
 * Provides a base class for HTML controls.
 * @param {HTMLElement} element The HTML element that this control wraps.
 * @arguments {String} [1-n] Any events that this control dispatches.
 */
var HtmlControl = Dispatcher.extend(function HtmlControl(element)
{
	this.construct($array.fromArguments(arguments, 1));

	/**
	 * @type {jQuery}
	 */
	this.$element = $(element);
});

/**
 * Gets or sets the id of the element that this control uses.
 * @param {String} id The new id top to set on the element. Optional.
 * @returns {String} The current id of the element.
 */
HtmlControl.prototype.id = function HtmlControl$id()
{
	if ($type.isString(arguments[0]))
		this.$element.attr("id", arguments[0]);

	return this.$element.attr("id");
};

/**
 * Gets the HTML element that this control uses.
 * @returns {HTMLElement} The HTML element that this control uses.
 */
HtmlControl.prototype.element = function HtmlControl$element()
{
	return this.$element;
};

/**
 * Gets a string that represents this element.
 * @returns {String} A string that represents this element.
 */
HtmlControl.prototype.toString = function HtmlControl$toString()
{
	var name = Function.getName(this.constructor);
	if (this.$element.length == 0)
		return $string.format("{0}(null)", name);

	var attrId = this.id();
	var attrClass = this.$element.attr("class");
	var tagName = String(this.$element.prop("tagName")).toLowerCase();
	return $string.format("{0}(\"{1}{2}{3}\")", name, tagName,
		attrClass ? "." + attrClass.replace(/\s+/, ".") : $string.EMPTY,
		attrId ? "#" + attrId : $string.EMPTY);
};
