Type.registerNamespace("aeon.animation");

/**
 * Specifies a clip rectangle.
 * Css 'clip' property specifies which part of an HTML element is visible. The specification is expressed using the
 * top, right, bottom and left properties, which specify the extent to which that element is 'clipped' on that side.
 * @constructor
 */
aeon.animation.ClipRect = function ClipRect(top, right, bottom, left)
{
	/**
	 * The clip top value.
	 * @type {Number}
	 */
	this.top = parseInt(top);
	/**
	 * The clip right value.
	 * @type {Number}
	 */
	this.right = parseInt(right);
	/**
	 * The clip bottom value.
	 * @type {Number}
	 */
	this.bottom = parseInt(bottom);
	/**
	 * The clip left value.
	 * @type {Number}
	 */
	this.left = parseInt(left);
}

aeon.animation.ClipRect.prototype.toString = function ClipRect$toString()
{
	return String.format("rect({0}px {1}px {2}px {3}px)",
		this.top, this.right, this.bottom, this.left
	);
}


/**
 * Parses the specifies value and returns a new <c>aeon.animation.ClipRect</c>.
 * The <c>value</c> can be one of the following (where 'w' is element.width and 'h' is element.height):
 * <ul><li>An instance of previously created <c>ClipRect</c> object</li>
 * <li>A string in following format: '(\w+) (\w+) (\w+) (\w+)' => ClipRect($1, $2, $3, $4)</li>
 * <li>A string in following format: '(\w+) (\w+)' => ClipRect($1, $2, $1, $2)</li>
 * <li>An array with 4 elements: => ClipRect(value[0], value[1], value[2], value[3])</li>
 * <li>An array with 2 elements: => ClipRect(value[0], value[1], value[0], value[1])</li>
 * <li>A constant string: 'T' => ClipRect(0, w, 0, 0)</li>
 * <li>A constant string: 'L' => ClipRect(0, 0, h, 0)</li>
 * <li>A constant string: 'B' => ClipRect(h, w, h, 0)</li>
 * <li>A constant string: 'R' => ClipRect(0, w, h, 0)</li>
 * <li>A constant string: 'TL' => ClipRect(0, 0, 0, 0)</li>
 * <li>A constant string: 'TR' => ClipRect(0, w, 0, w)</li>
 * <li>A constant string: 'BL' => ClipRect(h, 0, h, 0)</li>
 * <li>A constant string: 'BR' => ClipRect(h, w, h, w)</li>
 * </ul>
 * If none of these options is valid, the result <c>ClipRect</c> will be ClipRect(0, w, h, 0), basically leaving the whole
 * element visible.
 * @param {Object} value The value to parse.
 * @param {HTMLElement} element The element for which to parse a new <c>ClipRect</c>.
 * @returns {ClipRect} The parsed clip rectangle.
 */
aeon.animation.ClipRect.parse = function ClipRect$parse(value, element)
{
	$assert.isHtmlElement(element);

	if (Type.instanceOf(value, $anim.ClipRect))
		return new $anim.ClipRect(value.top, value.right, value.bottom, value.left);

	if ($anim.ClipRect.MATCH_4 == null)
	{
		$anim.ClipRect.MATCH_2 = /(\d+)(?:px)?\W+(\d+)(?:px)?/;
		$anim.ClipRect.MATCH_4 = /(\d+)(?:px)?\W+(\d+)(?:px)?\W+(\d+)(?:px)?\W+(\d+)/;
	}

	value = String(value).trim();
	if (value.match($anim.ClipRect.MATCH_4))
	{
		return new $anim.ClipRect(RegExp.$1, RegExp.$2, RegExp.$3, RegExp.$4);
	}

	if (value.match($anim.ClipRect.MATCH_2))
	{
		return new $anim.ClipRect(RegExp.$1, RegExp.$2, RegExp.$1, RegExp.$2);
	}

	var w = element.offsetWidth;
	var h = element.offsetHeight;
	switch (value)
	{
		case "T":
			return new $anim.ClipRect(0, w, 0, 0);
		case "L":
			return new $anim.ClipRect(0, 0, h, 0);
		case "B":
			return new $anim.ClipRect(h, w, h, 0);
		case "R":
			return new $anim.ClipRect(0, w, h, w);
		case "TL":
			return new $anim.ClipRect(0, 0, 0, 0);
		case "TR":
			return new $anim.ClipRect(0, w, 0, w);
		case "BL":
			return new $anim.ClipRect(h, 0, h, 0);
		case "BR":
			return new $anim.ClipRect(h, w, h, w);
	}

	return new $anim.ClipRect(0, w, h, 0);
}

