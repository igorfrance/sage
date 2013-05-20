/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Dispatches (custom) events to event listeners.
 * All arguments to the constructor will be interpreted as names of events to register.
 * @constructor
 */
var Dispatcher = Prototype.extend(function Dispatcher()
{
	this.__events = new Array;
	this.__listeners = new Object;

	var events = $array.fromArguments(arguments);
	for (var i = 0; i < arguments.length; i++)
		this.registerEvent(arguments[i]);

});

/**
 * Registers an event with the specified name.
 * @arguments {String} [0-n] The names of the events to register.
 */
Dispatcher.prototype.registerEvent = function Dispatcher$registerEvent()
{
	for (var i = 0; i < arguments.length; i++)
	{
		var eventName = arguments[i];
		if (!this.hasEvent(eventName))
		{
			this.__events.push(eventName);
			this.__listeners[eventName] = new Array;
		}
	}
};

/**
 * Returns true if the specified event already exists.
 * @param {String} eventName The name of the event.
 * @return {Boolean} True if the specified event type exists.
 */
Dispatcher.prototype.hasEvent = function Dispatcher$hasEvent(eventName)
{
	return this.__events.contains(eventName);
};

/**
 * Returns true if the supplied eventType event listener exists.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 * @return {Boolean} True if the specified listener exists.
 */
Dispatcher.prototype.hasListener = function Dispatcher$hasListener(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		for (var i = 0; i < this.__listeners[eventName].length; i++)
		{
			if (this.__listeners[eventName][i] == eventListener)
				return true;
		}
	}
	return false;
};

/**
 * Adds an event listener for the specified event $type.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 * @returns {Dispatcher} The current object.
 */
Dispatcher.prototype.addListener = function Dispatcher$on(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		if (!this.hasListener(eventName, eventListener))
			this.__listeners[eventName].push(eventListener);
	}
	else
	{
		$log.error("Did not add event '{0}' because the current object only supports '{1}'",
			eventName, this.__events.join(", "));
	}

	return this;
};

Dispatcher.prototype.on = Dispatcher.prototype.addListener;

/**
 * Removes the specified event listener.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 * @returns {Dispatcher} The current object.
 */
Dispatcher.prototype.removeListener = function Dispatcher$removeListener(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		if ($type.isFunction(eventListener))
		{
			for (var i = 0; i < this.__listeners[eventName].length; i++)
			{
				if (this.__listeners[eventName][i] == eventListener)
				{
					this.__listeners[eventName].splice(i, 1);
					return;
				}
			}
		}
		else
			this.__listeners[eventName] = new Array;
	}

	return this;
};

Dispatcher.prototype.off = Dispatcher.prototype.removeListener;

/**
 * Fires the specified event.
 * @param {Object} event The event or the name of the event to fire.
 * @param {Object} eventData The event data.
 * @returns {Event} The event that was fired.
 */
Dispatcher.prototype.fireEvent = function Dispatcher$fireEvent(event, eventData)
{
	var eventObj = null;
	var eventType = null;

	if ($type.instanceOf(event, $evt.Event))
	{
		eventType = event.type;
		eventObj = event;
	}
	else
	{
		if (eventData == null)
			eventData = this.createEventData();

		eventType = event;
		eventObj = new $evt.Event(this, eventType, eventData);
	}

	var listeners = this.__listeners[eventType];
	if (listeners != null && listeners.length != 0)
	{
		for (var i = 0; i < listeners.length; i++)
		{
			if ($type.isFunction(listeners[i]))
			{
				listeners[i](eventObj);
				if (eventObj.cancel)
					break;
			}
		}

		return eventObj;
	}

	return null;
};

Dispatcher.prototype.fire = Dispatcher.prototype.fireEvent;

/**
 * Create an object with information about the raised event.
 * This is the default method, it returns an empty object. Override this method in child
 * dispatcher to add actual information.
 * @param {String} eventType The type (name) of the event that occurred.
 * @return {Object} Object with information about the raised event.
 */
Dispatcher.prototype.createEventData = function Dispatcher$createEventData(eventType)
{
	return {};
};

Dispatcher.prototype.toString = function Dispatcher$toString()
{
	var output = [];
	for (var eventType in this.__listeners)
	{
		output.push("{0}: {1} listeners".format(eventType, this.__listeners[eventType].length));
	}
	return "Dispatcher: {{0}}".format(output.join(", "));
};

