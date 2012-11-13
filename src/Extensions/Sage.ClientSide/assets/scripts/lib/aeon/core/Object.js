Object.serialize = function Object$serialize(object)
{
	if (Type.isNumber(object))
	{
		return object;
	}
	else if (Type.isString(object))
	{
		return Object.serializeString(object, true);
	}
	else if (Type.isArray(object))
	{
		return Object.serializeArray(object);
	}
	else if (Type.isDate(object))
	{
		return Object.serializeDate(object);
	}
	else if (Type.isFunction(object))
	{
		return Object.serializeFunction(object);
	}
	else if (Type.isObject(object))
	{
		return Object.serializeObject(object);
	}
	
	return String(object);
};

Object.serializeDate = function Object$serializeDate(value)
{
	if (value == null)
		return "null";

	return String.format("new Date({0})", value.getTime());
};

Object.serializeFunction = function Object$serializeFunction(value)
{
	return value.toString();
};

Object.serializeArray = function Object$serializeArray(value)
{
	if (value == null)
		return "null";

	var result = [];
	result.push("[");

	var values = [];
	for (var i = 0; i < value.length; i++)
		values.push(Object.serialize(value[i]));

	result.push(values.join(", "));
	result.push("]");

	return result.join(String.EMPTY);
};

Object.serializeObject = function Object$serializeObject(object)
{
	if (object == null)
		return "null";

	var result = [];
	result.push("{");

	var members = [];
	for (var name in object)
	{
		var member = [];
		member.push(Object.serializeString(name, true));
		member.push(": ");
		member.push(Object.serialize(object[name]));

		members.push(member.join(String.EMPTY));
	}
	result.push(members.join(", "));
	result.push("}");

	return result.join(String.EMPTY);
};

Object.serializeString = function Object$serializeString(value, quote)
{
	if (value == null)
		return "null";

	value = String(value).replace(
		/(^|[^\\])\"/g, "$1\\\"").replace(
		/(^|[^\\])\'/g, "$1\\'");

	return (quote ? '"' + value + '"' : value);
};

