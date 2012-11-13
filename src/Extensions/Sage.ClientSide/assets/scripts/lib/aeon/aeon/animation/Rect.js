Type.registerNamespace("aeon.animation");

/**
 * Specifies a rectangle.
 * @constructor
 */
aeon.animation.Rect = function Rect(top, right, bottom, left)
{
	/**
	 * The top value.
	 * @type {Number}
	 */
	this.top = Type.isNumeric(top) ? parseFloat(top) : null;
	/**
	 * The right value.
	 * @type {Number}
	 */
	this.right =  Type.isNumeric(right) ? parseFloat(right) : null;
	/**
	 * The bottom value.
	 * @type {Number}
	 */
	this.bottom =  Type.isNumeric(bottom) ? parseFloat(bottom) : null;
	/**
	 * The left value.
	 * @type {Number}
	 */
	this.left =  Type.isNumeric(left) ? parseFloat(left) : null;
}

/**
 * Parses the specifies value and returns a new <c>aeon.animation.Rect</c>.
 * The <c>value</c> can be one of the following (where 'w' is element.width and 'h' is element.height):
 * <ul><li>An instance of previously created <c>Rect</c> object</li>
 * <li>A string in following format: '(\w+) (\w+) (\w+) (\w+)' => Rect($1, $2, $3, $4)</li>
 * <li>A string in following format: '(\w+) (\w+)' => Rect($1, $2, $1, $2)</li>
 * <li>An array with 4 elements: => ClipRect(value[0], value[1], value[2], value[3])</li>
 * <li>An array with 2 elements: => ClipRect(value[0], value[1], value[0], value[1])</li>
 * </ul>
 * If none of these options is valid, the result <c>Rect</c> will be Rect(0, 0, 0, 0).
 * @param {Object} value The value to parse.
 * @returns {Rect} The parsed rectangle.
 */
aeon.animation.Rect.parse = function ClipRect$parse(value)
{
	$assert.isNotNull(value);

	if (Type.instanceOf(value, $anim.Rect))
		return new $anim.Rect(value.top, value.right, value.bottom, value.left);

	if ($anim.Rect.MATCH_4 == null)
	{
		$anim.Rect.MATCH_2 = /(\d+)(?:px)?\W+(\d+)(?:px)?/;
		$anim.Rect.MATCH_4 = /(\d+)(?:px)?\W+(\d+)(?:px)?\W+(\d+)(?:px)?\W+(\d+)/;
	}

	value = String(value).trim();
	if (value.match($anim.Rect.MATCH_4))
	{
		return new $anim.Rect($1, $2, $3, $4);
	}
	if (value.match($anim.Rect.MATCH_2))
	{
		return new $anim.Rect($1, $2, $1, $2);
	}

	return new $anim.Rect(0, 0, 0, 0);
}

