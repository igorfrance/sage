/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Gets or sets a cookie, or gets a raw cookie string, depending on the number of arguments.
 * If no arguments are provided, it returns the raw browser cookie string. If one argument
 * is provided, it is considered the name of the cookie to get. The value returned is the value
 * of that cookie (@see {Cookie.get}). If more arguments are provided, the cookie is set (@see {Cookie.set})
 */
var $cookie = new function cookie()
{
	/**
	 * Sets/adds a cookie to the document.cookies collection
	 * @param {String} name The name of the cookie. Required.
	 * @param {String} value The value of the cookie. Required.
	 * @param {String} expires An expression, signifying the expires property of the cookie. See <code>MakeDate</code>. Optional.
	 * @param {String} domain The domain property of the cookie. Optional.
	 * @param {Boolean} secure The secure property of the cookie. Optional.
	 * @return {String} The value of the cookie that has just been set.
	 */
	function set(name, value, expires, domain, path, secure)
	{
		var cookie = name + "=" + escape(value);

		if (expires) cookie += "; expires=" + MakeDate(expires).toGMTString();
		if (domain)  cookie += "; domain="  + domain;
		if (path)    cookie += "; path="    + path;
		if (secure)  cookie += "; secure=true";

		document.cookie = cookie;

		return get(name);
	};

	/**
	 * Returns a cookie with the specified name from the document.cookies collection, if present,
	 * and a <code>null</code> if not.
	 * @param {String} name The name of the cookie. Required.
	 * @return {String|null} The value of the cookie, if found, and a <code>null</code> otherwise.
	 */
	function get(name)
	{
		var cookies = document.cookie.split("; ");
		for (var i = 0; i < cookies.length; i++)
		{
			var cookieName  = cookies[i].substring(0, cookies[i].indexOf("="));
			var cookieValue = cookies[i].substring(cookies[i].indexOf("=") + 1, cookies[i].length);
			if (cookieName == name)
			{
				if (cookieValue.indexOf("&") != -1)
				{
					var pairs  = strValue.split("&");
					var cookie = new Object();
					for (var i in pairs)
					{
						var arrTemp = pairs[i].split("=");
						cookie[arrTemp[0]] = arrTemp[1];
					}
					return cookie;
				}
				else
					return unescape(cookieValue);
			}
		}
		return null;
	};

	/**
	 * Deletes a cookie with the specified name from the document.cookies collection.
	 * @param {String} name The name of the cookie. Required.
	 */
	function remove(name)
	{
		var cookieValue = name ? name + "=null;" : "null;"
		document.cookie = cookieValue + "expires=" + new Date().toGMTString();
	};

	function cookie()
	{
		if (arguments.length == 0)
			return String(document.cookie);

		if (arguments.length == 1)
			return Cookie.get.apply(this, arguments);

		return set.apply(this, arguments);
	}

	cookie.get = get;
	cookie.set = set;
	cookie.remove = remove;

	return cookie;
};

