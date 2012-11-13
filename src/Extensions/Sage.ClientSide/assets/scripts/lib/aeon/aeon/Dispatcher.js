Type.registerNamespace("aeon");

/**
 * Dispatches (custom) events to event listeners.
 * All arguments to the constructor will be interpreted as names of events to register.
 * @constructor
 */
aeon.Dispatcher = function Dispatcher()
{
	/**
	 * Object's defined event types
	 * @type {Array}
	 */
	this.$events = new Array;

	/**
	 * Object listeners, by event type
	 * @type {Object}
	 */
	this.$listeners = new Object;

	var events = Array.fromArguments(arguments);
	for (var i = 0; i < events.length; i++)
		this.addEvent(events[i]);
};

/**
 * Registers an event with the specified name.
 * @arguments {String} [0-n] The names of the events to register.
 */
aeon.Dispatcher.prototype.addEvent = function Dispatcher$addEvent()
{
	for (var i = 0; i < arguments.length; i++)
	{
		var eventName = arguments[i];
		if (!this.hasEvent(eventName))
		{
			this.$events.push(eventName);
			this.$listeners[eventName] = new Array;
		}
	}
};

/**
 * Returns true if the specified event already exists.
 * @param {String} eventName The name of the event.
 * @return {Boolean} True if the specified event type exists.
 */
aeon.Dispatcher.prototype.hasEvent = function Dispatcher$hasEvent(eventName)
{
	return this.$events.contains(eventName);
};

/**
 * Adds an event listener for the specified event type.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 */
aeon.Dispatcher.prototype.addListener = function Dispatcher$addListener(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		if (!this.hasListener(eventName, eventListener))
			this.$listeners[eventName].push(eventListener);
	}
	else
	{
		$log.error("Did not add event '{0}' because the current object only supports '{1}'",
			eventName, this.$events.join(", "));
	}
};

/**
 * Removes the specified event listener.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 */
aeon.Dispatcher.prototype.removeListener = function Dispatcher$removeListener(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		for (var i = 0; i < this.$listeners[eventName].length; i++)
		{
			if (this.$listeners[eventName][i] == eventListener)
			{
				this.$listeners[eventName].splice(i, 1);
				return;
			}
		}
	}
};

/**
 * Returns true if the supplied eventType event listener exists.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 * @return {Boolean} True if the specified listener exists.
 */
aeon.Dispatcher.prototype.hasListener = function Dispatcher$hasListener(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		for (var i = 0; i < this.$listeners[eventName].length; i++)
		{
			if (this.$listeners[eventName][i] == eventListener)
				return true;
		}
	}
	return false;
};

/**
 * Fires the specified event.
 * @param {Object} event The event or the name of the event to fire.
 * @param {Object} eventData The event data.
 */
aeon.Dispatcher.prototype.fireEvent = function Dispatcher$fireEvent(event, eventData)
{
	var eventObj = null;
	var eventType = null;

	if (event instanceof aeon.Event)
	{
		eventType = event.type;
		eventObj = event;
	}
	else
	{
		if (eventData == null)
			eventData = this.createEventData();

		eventType = event;
		eventObj = new aeon.Event(this, eventType, eventData);
	}

	var listeners = this.$listeners[eventType];
	if (listeners != null && listeners.length != 0)
	{
		for (var i = 0; i < listeners.length; i++)
		{
			if (Type.isFunction(listeners[i]))
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

/**
 * Create an object with information about the raised event.
 * This is the default method, it returns an empty object. Override this method in child
 * dispatcher to add actual information.
 * @return {Object} Object with information about the raised event.
 */
aeon.Dispatcher.prototype.createEventData = function Dispatcher$createEventData()
{
	return {};
};

aeon.Dispatcher.prototype.toString = function Dispatcher$toString()
{
	var output = [];
	for (var eventType in this.$listeners)
	{
		output.push("{0}: {1} listeners".format(eventType, this.$listeners[eventType].length));
	}
	return "Dispatcher: {{0}}".format(output.join(", "));
};

