/**
 * Property that helps with identifying arrays when doing type checking.
 * @type {Boolean}
 */
Array.prototype.isArray = true;

/**
 * Returns an array of arguments.
 * The first argument is the array of arguments that was passed to the calling function. The second argument is the start index
 * at which to check for an element that is itself an array, or from which to start copying elements into the resulting list. If the
 * <c>start</c> argument is omitted, it will default to zero. Therefore, if the Nth element in the list is itself an array, it will
 * be that element that will be returned.
 * @example var args = Array.fromArguments(arguments);
 * @example var args = Array.fromArguments(arguments, 2);
 * @param {Array} list The array of arguments from which to return a list
 * @param {Number} start The index from which to start copying. Optional (Default is 0).
 * @param {Number} stop The index at which to stop copying. Optional (Default is list.length).
 * @return {Array} The array of arguments.
 */
Array.fromArguments = function Array$fromArguments(list, start, stop)
{
	if (isNaN(parseInt(start)))
		start = 0;

	var result = [];
	if (list && Type.isArray(list[start]))
	{
		result = list[start];
	}
	else if (list)
	{
		if (isNaN(parseInt(stop)))
			stop = list.length - 1;

		for (var i = start; i <= stop; i++)
		{
			result.push(list[i]);
		}
	}

	return result;
};

/**
 * Returns a copy of the current array, with any arguments appended.
 */
Array.prototype.append = function Array$append()
{
	var copy = new Array();
	for (var i = 0; i < this.length; i++)
		copy.push(this[i]);

	var args = Array.fromArguments(arguments);
	for (var i = 0; i < args.length; i++)
		copy.push(args[i]);

	return copy;
};

/**
 * Returns a value indicating whether the specified value exists in the current array.
 * @param {Object} value The value to find
 * @returns {Boolean} <c>true</c> if the value was found, otherwise <c>false</c>.
 */
Array.prototype.contains = function Array$contains(value)
{
	return this.indexOf(value) != -1;
};

/**
 * Returns the index of the specifid value. If the value wasn't found, returns -1.
 * @param {Object} value The value to find
 * @returns {Number} The index of the specified value within this array.
 */
Array.prototype.indexOf = function Array$indexOf(value)
{
	var result = -1;
	for (var i = 0; i < this.length; i++)
	{
		if (this[i] === value)
		{
			result = i;
			break;
		}
	}
	return result;
};

/**
 * Returns a new array with the specified values removed from it
 * @param {String} value The values to remove (can be any number or arguments)
 * @return {Array} A new array with the value removed from it
 */
Array.prototype.remove = function Array$remove()
{
	var list = new Array;
	var remove = false;
	for (var i = this.length - 1; i >= 0; i--)
	{
		remove = false;
		for (var j = 0; j < arguments.length; j++)
		{
			if (this[i] == arguments[j])
			{
				remove = true;
				break;
			}
		}
		if (remove == true)
			list.push(i);
	}
	for (var i = 0; i < list.length; i++)
	{
		this.splice(list[i], 1);
	}

	return this;
};

/**
 * Returns a sum of all elements in the array.
 * @return {Number} The sum of all values in the array.
 */
Array.prototype.sum = function Array$sum()
{
	var result = 0;
	for (var i = 0; i < this.length; i++)
		result += Number(this[i]) || 0;

	return result;
};

/**
 * Calls the supplied function on each of the elements in the array, optionally setting the element value to the value returned from the function.
 * The function receives three arguments with each call:
 * <ul><li><c>elem</c> - the current array element</li>
 * <li><c>index</c> - the current array index</li>
 * <li><c>array</c> - the array itself</li></ul>
 * To set the value of the array element, make sure the function returns value other than <c>undefined</c>.
 * @param {Function} fx The function to call for each element of the array.
 */
Array.prototype.each = function Array$each(fx)
{
	for (var i = 0; i < this.length; i++)
	{
		var r = fx.call(this, i, this[i], this);
		if (r != undefined)
			this[i] = r;
	}

	return this;
};

