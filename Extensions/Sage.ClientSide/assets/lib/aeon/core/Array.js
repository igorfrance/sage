/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides several array utilities and extensions.
 */
var $array = new function()
{
	/**
	 * Returns an array of arguments.
	 * The first argument is the array of arguments that was passed to the calling function. The second argument is the start index
	 * at which to check for an element that is itself an array, or from which to start copying elements into the resulting list. If the
	 * <c>start</c> argument is omitted, it will default to zero. Therefore, if the Nth element in the list is itself an array, it will
	 * be that element that will be returned.
	 * @example var args = $array.fromArguments(arguments);
	 * @example var args = $array.fromArguments(arguments, 2);
	 * @param {Array} list The array of arguments from which to return a list
	 * @param {Number} start The index from which to start copying. Optional (Default is 0).
	 * @param {Number} stop The index at which to stop copying. Optional (Default is list.length).
	 * @return {Array} The array of arguments.
	 */
	this.fromArguments = function fromArguments(list, start, stop)
	{
		if (list == null || undefined)
			return [];

		if (!$type.isArray(list))
			list = [list];

		if (list.length == 1 && $type.isArray(list[0]))
			list = list[0];

		if (isNaN(parseInt(start)))
			start = 0;

		if (isNaN(parseInt(stop)))
			stop = list.length - 1;

		var result = [];
		for (var i = start; i <= stop; i++)
			result.push(list[i]);

		if (result.length == 1 && $type.isArray(result[0]))
			result = result[0];

		return result;
	};

	/**
	 * Returns all supplied arguments flattened into a single array.
	 * @returns {Array}
	 */
	this.flatten = function flatten()
	{
		var result = [];
		for (var i = 0; i < arguments.length; i++)
		{
			var current = arguments[i];
			if (!$type.isArray(current))
			{
				result.push(current);
				continue;
			}

			for (var j = 0; j < current.length; j++)
			{
				if (!$type.isArray(current[j]))
				{
					result.push(current[j]);
				}
				else
				{
					result.push.apply(result, this.flatten(current[j]));
				}
			}
		}

		return result;
	};


	/**
	 * Provides a shim for older browsers that do not implement array extras.
	 */
	var extras =
	{
		/**
		 * Checks whether the array contains the specified element.
		 * @returns {Boolean} <c>true</c> if the array contains the specified element; otherwise <c>false</c>.
		 */
		contains: function contains(element)
		{
			return this.indexOf(element) >= 0;
		},

		/**
		 * Returns the first index at which a given element can be found in the array, or -1 if it is not present.
		 * @param {Object} element Element to locate in the array.
		 * @param {Number} fromIndex The index at which to begin the search. Defaults to 0, i.e.
		 * the whole array will be searched. If the index is greater than or equal to the length of the
		 * array, -1 is returned, i.e. the array will not be searched.
		 * @returns {Number} The index of the specified <c>element</c>, or -1 if it ws not found.
		 */
		indexOf: function indexOf(element, fromIndex)
		{
			var start = isNaN(fromIndex) ? 0 : fromIndex;
			if (start < 0 || start > this.length - 1)
				return -1;

			for (var i = start; i < this.length; i++)
				if (this[i] == element)
					return i;

			return -1;
		},

		/**
		 * Returns the last index at which a given element can be found in the array, or -1 if it is not present..
		 * @param {Object} element Element to locate in the array.
		 * @param {Number} fromIndex The index at which to start searching backwards. Defaults to the array's
		 * length, i.e. the whole array will be searched. If the index is greater than or equal to the length
		 * of the array, the whole array will be searched.
		 * @returns {Number} The index of the specified <c>element</c>, or -1 if it ws not found.
		 */
		lastIndexOf: function lastIndexOf(element, fromIndex)
		{
			var end = isNaN(fromIndex) ? this.length - 1 : fromIndex;
			if (end < 0 || end > this.length - 1)
				return -1;

			for (var i = end; i >= 0; i--)
				if (this[i] == element)
					return i;

			return -1;
		},

		/**
		 * Tests whether all elements in the array pass the test implemented by the provided function.
		 * <p><c>every</c> executes the provided callback function once for each element present in the array
		 * until it finds one where callback returns a false value. If such an element is found, the every
		 * method immediately returns false. Otherwise, if callback returned a true value for all elements,
		 * every will return true. callback is invoked only for indexes of the array which have assigned values;
		 * it is not invoked for indexes which have been deleted or which have never been assigned values.</p>
		 * <c>callback</c> is invoked with three arguments: the value of the element, the index of the element,
		 * and the Array object being traversed.
		 * @param {Function} callback Function to test for each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 * @returns {Boolean} <c>true</c> if all elements in the array pass the test; otherwise <c>false</c>.
		 */
		every: function every(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			context = context || this;

			for (var i = 0; i < this.length; i++)
			{
				var result = callback.call(context, this[i], i, this);
				if (!result)
					return false;
			}

			return true;
		},

		/**
		 * Tests whether some element in the array passes the test implemented by the provided function.
		 * <p>some executes the callback function once for each element present in the array until it finds one where callback returns a true value. If such an element is found, some immediately returns true. Otherwise, some returns false. callback is invoked only for indexes of the array which have assigned values; it is not invoked for indexes which have been deleted or which have never been assigned values.</p>
		 * <c>callback</c> is invoked with three arguments: the value of the element, the index of the element,
		 * and the Array object being traversed.
		 * @param {Function} callback Function to test for each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 * @returns {Boolean} <c>true</c> if all elements in the array pass the test; otherwise <c>false</c>.
		 */
		some: function some(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			context = context || this;

			for (var i = 0; i < this.length; i++)
			{
				var result = callback.call(context, this[i], i, this);
				if (result)
					return true;
			}

			return false;
		},

		/**
		 * Creates a new array with all elements that pass the test implemented by the provided function.
		 * <p><c>filter</c> calls a provided <c>callback</c> function once for each element in an array, and constructs a new array of all
		 * the values for which <c>callback</c> returns a true value. <c>callback</c> is invoked only for indexes of the
		 * array which have assigned values; it is not invoked for indexes which have been deleted or which
		 * have never been assigned values. Array elements which do not pass the <c>callback</c> test are simply
		 * skipped, and are not included in the new array.</p>
		 * <p><c>callback</c> is invoked with three arguments: the value of the element, the index of the
		 * element, and the Array object being traversed.
		 * @param {Function} callback Function to test for each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 * @returns {Array} A new array, consisting of elements that passes the <c>callback</c> filtering.
		 */
		filter: function filter(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			var result = [];
			context = context || this;

			for (var i = 0; i < this.length; i++)
			{
				var pass = callback.call(context, this[i], i, this);
				if (pass)
					result.push(this[i]);
			}

			return result;
		},

		/**
		 * Executes the provided function once on each array element.
		 * <p><c>callback</c> is invoked only for indexes of the array which have assigned values;
		 * it is not invoked for indexes which have been deleted or which have never been assigned values.</p>
		 * <p><c>callback</c> is invoked with three arguments: the value of the element, the index of the
		 * element, and the Array object being traversed.
		 * @param {Function} callback Function to execute on each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 */
		forEach: function forEach(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			context = context || this;
			for (var i = 0; i < this.length; i++)
			{
				if (this[i] === undefined)
					continue;

				callback.call(context, this[i], i, this);
			}
		},

		/**
		 * Creates a new array with the results of calling a provided function on every element in this array.
		 * <p><c>map</c> calls the provided <c>callback</c> function once for each element in an array,
		 * in order, and constructs a new array from the results. <c>callback</c> is invoked only for indexes
		 * of the array which have assigned values; it is not invoked for indexes which have been deleted or
		 * which have never been assigned values.</p>
		 * <p><c>callback</c> is invoked with three arguments: the value of the element, the index of the
		 * element, and the Array object being traversed.
		 * @param {Function} callback Function to execute on each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 */
		map: function map(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			var result = [];
			context = context || this;

			for (var i = 0; i < this.length; i++)
			{
				if (this[i] === undefined)
					continue;

				result.push(callback.call(context, this[i], i, this));
			}

			return result;
		}
	};

	$type.extend(Array.prototype, extras);
};
