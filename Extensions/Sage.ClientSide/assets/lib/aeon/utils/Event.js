/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides event-related utilities.
 */
var $evt = new function ()
{
	/**
	 * Returns the cross-browser event object.
	 * @param {DOMEvent} event The event that was raised. In IE6 and lower this is null and is read from the window object.
	 * @returns {DOMEvent} The cross-browser event object.
	 */
	function evt(e)
	{
		return e || window.event;
	}

	/**
	 * Calls <c>preventDefault</c> method if the event object supports it.
	 * @param {DOMEvent} event The event that was raised. In IE6 and lower this is null and is read from the window object.
	 * @returns {Boolean} Returns false.
	 */
	evt.preventDefault = function preventDefault(e)
	{
		var e = evt(e);
		if (e.preventDefault)
			e.preventDefault();

		return false;
	};

	/**
	 * Sets the event's <c>cancelBubble</c> to <c>true</c>, <c>returnValue</c> to <c>false</c> and calls it's
	 * <c>stopPropagation</c> event.
	 * @param {DOMEvent} event The event that was raised. In IE6 and lower this is null and is read from the window object.
	 * @returns {Boolean} Returns false.
	 */
	evt.cancel = function cancel(e)
	{
		var e = evt(e);
		if (e != null)
		{
			e.cancelBubble = true;
			e.returnValue = false;

			if (e.stopPropagation)
				e.stopPropagation();
		}

		return false;
	};

	/**
	 * Creates a new <c>Event</c> object.
	 * @param {Dispatcher} source The object that fired the event.
	 * @param {String} type The event type/name.
	 * @param {Object} data Optional object that contain additional event information.
	 * @returns {Event} A new <c>Event</c> object.
	 */
	evt.create = function createEvent(source, type, data)
	{
		return new evt.Event(source, type, data);
	};

	/**
	 * Implements an object that provides information about events raised by <c>Dispatcher</c> objects.
	 * @constructor
	 * @param {Dispatcher} source The object that fired the event.
	 * @param {String} type The event type/name.
	 * @param {Object} data Optional object that contain additional event information.
	 */
	evt.Event = function Event(source, type, data)
	{
		this.source = source;
		this.type = type;
		this.name = type;
		this.cancel = false;
		this.data = new Object;

		for (var prop in data)
			this.data[prop] = data[prop];
	};

	return evt;
}
