Type.registerNamespace("aeon.utils");

/**
 * Provides both a wrapper around a url parsed from a string, and a set of static methods for working with the browser's location object.
 * @class
 * @param {String} url The string value represents this url.
 */
aeon.utils.Url = function Url(url)
{
	if (Type.isString(url))
	{
		this.parse(url);
	}
	else
	{
		this.parse(location.href);
	}
};

/**
 * Defines the regular expression that matches different parts of a URL.
 * The different parts of the URL that this expression matches are:
 * <ul>
 * <li>authority<ul>
 *     <li>userInfo<ul>
 *         <li>user</li>
 *         <li>password</li></ul></li>
 *     <li>host</li>
 *     <li>port</li></ul></li>
 * <li>relative<ul>
 *     <li>path<ul>
 *         <li>directory</li>
 *         <li>file</li></ul></li>
 *     <li>query</li>
 *     <li>anchor (fragment)</li></ul></li>
 * </ul>
 * @type {RegExp}
 */
aeon.utils.Url.URL_EXPRESSION =
	/^(?:(?![^:@]+:[^:@\/]*@)([^:\/?#.]+):)?(?:\/\/)?((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/;

/**
 * Provides an array of names corresponding to the submatch indexes of the regular expression <c>URL_EXPRESSION</c>.
 */
aeon.utils.Url.MATCH_NAMES =
	["source", "protocol", "authority", "userInfo", "user", "password", "host", "port", "relative", "path", "directory", "file", "query", "hash"];

/**
 * Specifies the protocol separator string ("://").
 * @type {String}
 */
aeon.utils.Url.PROTOCOL_SEPARATOR = "://";

/**
 * Specifies the string to use to indicate an empty hash string and to interpret a hash string as empty.
 * When manipulating hash parameters of the browser's location object by using <c>Url.setHash</c>, <c>Url.setHashParam</c>
 * and <c>Url.removeHashParam</c> methods, and the resulting hash is an empty string, the browser's location will contain
 * a hash character ("#") without any value following. Setting the location hash to '#' will cause the browser to
 * scroll to the top of the page, which is most of the time not the desired effect. To prevent that, this class uses
 * the string specified with this constant to indicate an empty string. In other words, if the hash contains just this
 * character it is considered empty, and similarly, if the resulting hash to set is an empty string, the value that
 * will be set will be this character.
 * @type {String}
 */
aeon.utils.Url.EMPTY_HASH = "!";

/**
 * Combines the parent <c>folderUrl</c> with the child <c>fileUrl</c> into a single URL.
 * @param {String} folderUrl The parent URL to combine, e.g. <c>my/directory/to/files</c>.
 * @param {String} fileUrl The child URL to combine, e.g. <c>../file2.txt</c>
 * @return {String} The combined URL.
 * @example
 * // the following returns "my/directory/to/file2.txt":
 * var combined = $url.combine("my/directory/to/files", "../file2.txt");
 */
aeon.utils.Url.combine = function Url$combine(folderUrl, fileUrl)
{
	var filePath = String.concat(folderUrl, "/", fileUrl);
	while (filePath.match(/[^\/]+\/\.\.\//))
	{
		filePath = filePath.replace(/[^\/]+\/\.\.\//, "");
	}
	return filePath.replace(/\/{2,}/g, "/");
};

/**
 * Gets a query string parameter with the specified name from the current document's location string.
 * @param {String} name The name of the parameter to get
 * @param {String} defaultValue The value to return in case the current query string doesn't contain a parameter with the
 * specified <c>name</c>.
 * @return {String} The vaue of the specified parameter, or the <c>defaultValue</c> if specified and <c>name</c> is missing
 * or an emppty string.
 */
aeon.utils.Url.getQueryParam = function Url$getQueryParam(name, defaultValue)
{
	return new $url().getQueryParam(name) || defaultValue;
};

aeon.utils.Url.getHashParam = function Url$getHashParam(name, defaultValue)
{
	return new $url().getHashParam(name) || defaultValue;
};

aeon.utils.Url.getHash = function Url$getHash()
{
	return String(location.hash).substring(1);
};

aeon.utils.Url.getQuery = function Url$getQuery()
{
	return String(location.search).substring(1);
};

/**
 * Sets the value of the specified hash parameter.
 * If a single argument is passed as an object, it will be treated as a name/value collection. If two values are
 * passed, they will be treated a single key/value pair.
 * @example $url.setHashParam("color", "red");
 * @example $url.setHashParam({ color: "red", size: "xlarge" });
 */
aeon.utils.Url.setHashParam = function Url$setHashParam()
{
	var url = new $url(location);
	if (arguments.length == 2)
	{
		url.setHashParam(arguments[0], arguments[1]);
	}
	else if (arguments.length == 1)
	{
		if (Type.isArray(arguments[0]))
			for (var name in arguments[0])
				url.setHashParam(name, arguments[0][name]);
		else
			url.setHashParam(arguments[0], String.EMPTY);
	}
	else
	{
		return;
	}

	aeon.utils.Url.setHash(url.getHash());
};

aeon.utils.Url.setHash = function Url$setHash(hash)
{
	// ensure that setting the location hash to empty doesn't cause the page to scroll to the top
	if (hash == String.EMPTY)
		hash = aeon.utils.Url.EMPTY_HASH;

  location.hash = hash;
};

aeon.utils.Url.removeHashParam = function Url$removeHashParam(name)
{
	var url = new $url(location);
	url.removeHashParam(name);

	aeon.utils.Url.setHash(url.getHash());
};

aeon.utils.Url.getFileExtension = function Url$getFileExtension(file)
{
	var dotIndex = file ? file.lastIndexOf(".") : -1;
	return (dotIndex > -1 ? file.slice(dotIndex + 1) : "");
};

aeon.utils.Url.getFileName = function Url$getFileName(file)
{
	return file && file.match(/[^\\\/]*$/)[0];
};

aeon.utils.Url.parseQuery = function Url$parseQuery(value)
{
	if (!value)
	{
		return {};
	}

	var param = String(value).split("&");
	var query = {};
	for (var i = 0; i < param.length; i++)
	{
		if (param[i].length == 0)
			continue;

		var pair = param[i].split("=");
		var itemKey = pair[0];
		var itemValue = pair[1] || String.EMPTY;

		if (query[itemKey] != null)
		{
			if (!Type.isArray(query[itemKey]))
			{
				query[itemKey] = [query[itemKey]];
			}

			query[itemKey].push(itemValue);
		}
		else
			query[itemKey] = itemValue;
	}

	return query;
};

aeon.utils.Url.serializeParams = function Url$serializeParams(params, prefix, encode)
{
	var result = [];
	for (var name in params)
	{
		var itemName = encode ? encodeURIComponent(name) : name;
		var itemValue = encode ? encodeURIComponent(params[name]  || String.EMPTY) : params[name];
		if (itemValue)
			result.push(itemName + "=" + itemValue);
		else
			result.push(itemName);
	}
	var value = result.join("&");
	if (prefix && value.length)
	{
		return String.concat(prefix, value);
	}
	return value;
};

aeon.utils.Url.prototype.init = function Url$init()
{
	this.components = {};
	this.hashParam = {};
	this.queryParam = {};
};

aeon.utils.Url.prototype.getHash = function Url$getHash(includeHashSymbol)
{
	var result = [];
	for (var name in this.hashParam)
		result.push(name + "=" + this.hashParam[name]);

	var value = result.join("&");
	if (value != String.EMPTY && includeHashSymbol)
		return "#" + value;

	return value;
};

aeon.utils.Url.prototype.getQuery = function Url$getQuery(includeQuestionMark)
{
	var result = [];
	for (var name in this.queryParam)
		result.push(name + "=" + this.queryParam[name]);

	var value = result.join("&");
	if (value != String.EMPTY && includeQuestionMark)
		return "?" + value;

	return value;
};

aeon.utils.Url.prototype.getQueryParam = function Url$getQueryParam(itemKey)
{
	return this.queryParam[itemKey];
};

aeon.utils.Url.prototype.getHashParam = function Url$getHashParam(itemKey)
{
	return this.hashParam[itemKey];
};

aeon.utils.Url.prototype.setQueryParam = function Url$setQueryParam(itemKey, itemValue)
{
	this.queryParam[itemKey] = itemValue;
};

aeon.utils.Url.prototype.setHashParam = function Url$setHashParam(itemKey, itemValue)
{
	return this.hashParam[itemKey] = itemValue;
};

aeon.utils.Url.prototype.removeQueryParam = function Url$removeQueryParam(itemKey)
{
	delete this.queryParam[itemKey];
};

aeon.utils.Url.prototype.removeHashParam = function Url$removeHashParam(itemKey)
{
	delete this.hashParam[itemKey];
};

aeon.utils.Url.prototype.parse = function Url$parse(url)
{
	if (url == null)
		return;

	this.init();

	var matches = $url.URL_EXPRESSION.exec(url);
	var names = $url.MATCH_NAMES;

	for (var i = names.length - 1; i > 0; i--)
		this.components[names[i]] = matches[i] || String.EMPTY;

	var hash = this.components.hash;
	var query = this.components.query;

	if (hash == aeon.utils.Url.EMPTY_HASH)
		hash = String.EMPTY;

	this.hashParam = $url.parseQuery(hash);
	this.queryParam = $url.parseQuery(query);
};

aeon.utils.Url.prototype.toString = function Url$toString()
{
	var result = String.concat(
		this.components.protocol,
		$url.PROTOCOL_SEPARATOR,
		this.components.authority,
		this.components.path,
		$url.serializeParams(this.queryParam, "?", true),
		$url.serializeParams(this.hashParam, "#", false)
	);

	return result;
};

var $url = aeon.utils.Url;
