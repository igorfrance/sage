Type.registerNamespace("aeon.utils");

/**
 * Provides utility methods for working with events.
 */
aeon.utils.Event = function Event(e)
{
	if (e != null)
		return e.domEvent || e;

	return window.event;
};

aeon.utils.Event.srcElement = function Event$srcElement(e)
{
	var e = $evt(e);
	if (e == null)
		return null;

	if (e.srcElement)
		return e.srcElement;

	else if (e.target)
	{
		if (e.target.nodeType == 1 || e.target.nodeType == 9)
			return e.target;
		else
			return e.target.parentNode;
	}

	return null;
};

/**
 * Adds an event listener to the supplied target.
 * The target should be a DOM object, and the event one of the events supported by it.
 * @param {Object} target The DOM element to add the event listener to.
 * @param {String} eventName The name of the event to listen to.
 * @param {Function} eventHandler The event listener.
 * @param {Boolean} insertBefore If true, the listener will be added at the beginning of
 * the array of listeners.
 */
aeon.utils.Event.addListener = function Event$addListener(target, eventName, eventHandler, insertBefore)
{
	if (target == null)
		return;

	var elems = Array.fromArguments(arguments, 0, 0);
	for (var i = 0; i < elems.length; i++)
	{
		target = elems[i];

		if (target.$listeners == null)
			target.$listeners = {};

		if (eventName.indexOf("on") != 0)
			eventName = "on" + eventName;

		if ((target == window || target == document) && eventName == "onready")
		{
			$(window).ready(eventHandler);
			return;
		}

		if (target.$listeners[eventName] == null)
		{
			target.$listeners[eventName] = new Array;
			if (target[eventName] != null)
				target.$listeners[eventName].push(target[eventName]);
		}

		var handlerExists = false;
		for (var j = 0; j < target.$listeners[eventName].length; j++)
			if (target.$listeners[eventName][j] == eventHandler)
				handlerExists = true;

		// only assign an event handler once
		if (handlerExists == false)
		{
			if (insertBefore == true)
				target.$listeners[eventName].unshift(eventHandler);
			else
				target.$listeners[eventName].push(eventHandler);
		}

		target[eventName] = function onEvent(e) /**/
		{
			var target = this;
			var event = $evt.wrapEvent(e, target);
			return $evt.fireEvent(target, event);
		};
		target[eventName].target = target;
	}
};

/**
 * Creates an object that contains cross-browser information abut an event, as well as the event itself.
 * @param {Object} e The standard DOMEvent instance (non IE)
 */
aeon.utils.Event.wrapEvent = function Event$wrapEvent(e, target)
{
	var domEvent = $evt(e);
	var result = new Object();

	for (var name in domEvent)
		result[name] = domEvent[name];

	result.domEvent = domEvent;
	result.target = $evt.srcElement(domEvent);
	result.srcElement = result.target;
	result.currentTarget = target;

	return result;
};

/**
 * Handles the firing of events added with <code>$evt.addListener</code>.
 * @param {Object} target The DOM element that fired the event.
 * @param {Object} eventHandler The standard DOMEvent instance
 * @param {String} eventName The name of the event that was fired.
 */
aeon.utils.Event.fireEvent = function Event$fireEvent(target, eventObj, eventName)
{
	if (eventObj != null)
		eventName = eventObj.type;

	if (target != null && target.$listeners != null && eventName != null)
	{
		if (eventName.indexOf("on") != 0)
			eventName = "on" + eventName;

		var finalResult = null;
		var currentResult = null;

		for (var i = 0; i < target.$listeners[eventName].length; i++)
		{
			try
			{
				currentResult = target.$listeners[eventName][i].call(target, eventObj);
			}
			catch(e)
			{
				$log.error("Firing event {0} on {1} failed: {2}", eventName, target, e.message);
			}
			if (currentResult != undefined)
				finalResult = currentResult;
		}

		return finalResult;
	}
};

aeon.utils.Event.removeListener = function Event$removeListener(targetElem, eventName, eventHandler)
{
	if (!targetElem || !targetElem.nodeName)
		return;

	if (eventName.indexOf("on") != 0)
		eventName = "on" + eventName;

	if (targetElem.$listeners != null && targetElem.$listeners[eventName] != null)
	{
		for (var i = 0; i < targetElem.$listeners[eventName].length; i++)
			if (targetElem.$listeners[eventName][i] == eventHandler)
				targetElem.$listeners[eventName].splice(i, 1);
	}
};

aeon.utils.Event.preventDefault = function Event$preventDefault(e)
{
	var e = $evt(e);
	if (e.preventDefault)
		e.preventDefault();

	return false;
};

aeon.utils.Event.cancel = function Event$cancel(e)
{
	var e = $evt(e);
	e.cancelBubble = true;

	if (e.stopPropagation)
		e.stopPropagation();

	e.returnValue = false;
	return false;
};

/**
 * Global alias to <c>aeon.utils.Evt</c>
 * @type {aeon.utils.Evt}
 */
var $evt = aeon.utils.Event;
