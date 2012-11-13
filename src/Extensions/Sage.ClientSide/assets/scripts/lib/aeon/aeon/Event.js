Type.registerNamespace("aeon");

/**
 * Represents the event that was fired.
 * @constructor
 * @param {Object} eventSource The element that is the source of the event.
 * @param {String} eventType The type/name of the event.
 * @param {Object} eventData Additional data to pass within the event's .data property.
 */
aeon.Event = function Event(eventSource, eventType, eventData)
{
	this.source = eventSource;
	this.type = eventType;
	this.name = eventType;
	this.data = new Object;
	this.cancel = false;

	for (var prop in eventData)
		this.data[prop] = eventData[prop];
};

