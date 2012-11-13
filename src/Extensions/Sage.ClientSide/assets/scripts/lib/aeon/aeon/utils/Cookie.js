Type.registerNamespace("aeon.utils");

/**
 * Provides utility methods for working with cookies.
 */
aeon.utils.Cookie = new function Cookie()
{
};

/**
 * Sets/adds a cookie to the document.cookies collection
 * @param {String} name The name of the cookie. Required.
 * @param {String} value The value of the cookie. Required.
 * @param {String} expires An expression, signifying the expires property of the cookie. See <code>aeon.utils.Cookie.dateCalc</code>. Optional.
 * @param {String} domain The domain property of the cookie. Optional.
 * @param {Boolean} secure The secure property of the cookie. Optional.
 * @return {String} The value of the cookie that has just been set.
 */
aeon.utils.Cookie.set = function Cookie$set(name, value, expires, domain, path, secure)
{
	var cookie = name + "=" + escape(value);

	if (expires) cookie += "; expires=" + $cookie.dateCalc(expires);
	if (domain)  cookie += "; domain="  + domain;
	if (path)    cookie += "; path="    + path;
	if (secure)  cookie += "; secure=true";

	document.cookie = cookie;

	return $cookie.get(name);
};

/**
 * Returns a cookie with the specified name from the document.cookies collection, if present,
 * and a <code>null</code> if not.
 * @param {String} name The name of the cookie. Required.
 * @return {String|null} The value of the cookie, if found, and a <code>null</code> otherwise.
 */
aeon.utils.Cookie.get = function Cookie$get(name)
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
 * Deletes a cookie with the specified name from the document.cookies collection, by setting its expires property
 * to the current date [expire now].
 * @param {String} name The name of the cookie. Required.
 */
aeon.utils.Cookie.remove = function Cookie$remove(name)
{
	var dateObj = new Date();
	document.cookie = name + "=null; expires=" + dateObj.toGMTString();
};

aeon.utils.Cookie.dateCalc = function Cookie$dateCalc(offsetString)
{
	var dateObj = new Date();
	var multip = new Object();
	var offsetTime;
	multip['s'] = 1000*1;
	multip['m'] = 1000*60;
	multip['h'] = 1000*60*60;
	multip['d'] = 1000*60*60*24;
	multip['M'] = 1000*60*60*24*30;
	multip['y'] = 1000*60*60*24*365;

	if (!offsetString || (offsetString == '') || (offsetString.toLowerCase() == 'now'))
	{ // this will set the time calc to now
		offsetTime = 0;
	}
	else if ( matches = offsetString.match( /^([+-]?(\d+|\d*\.\d*))([mhdMy]?)/ ) )
	{ // perform calculation if offsetString matches specification
		offsetTime = multip[matches[3]] * matches[2];
	}
	else
	{ // otherwise assume that we're providing the date ourselves so just return original
		return offsetString;
	}
	dateObj.setTime(dateObj.getTime() + offsetTime);
	return (dateObj.toGMTString());
};

/**
 * Global alias to <c>aeon.utils.Cookie</c>
 * @type {aeon.utils.Cookie}
 */
var $cookie = aeon.utils.Cookie;
