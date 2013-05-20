/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides a proxy object that can be used across different browsers to log to console.
 * @type {Object}
 */
var $url = new function url()
{
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
	URL_EXPRESSION =
		/^(?:(?![^:@]+:[^:@\/]*@)([^:\/?#.]+):)?(?:\/\/)?((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/;

	/**
	 * Provides an array of names corresponding to the submatch indexes of the regular expression <c>URL_EXPRESSION</c>.
	 */
	MATCH_NAMES =
		["source", "protocol", "authority", "userInfo", "user", "password", "host", "port", "relative", "path", "directory", "file", "query", "hash"];

	/**
	 * Specifies the protocol separator string ("://").
	 * @type {String}
	 */
	PROTOCOL_SEPARATOR = "://";

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
	EMPTY_HASH = "!";

	/**
	 * Provides a wrapper around a url parsed from a string, and a set of static methods for working
	 * with the browser's location object.
	 * @class
	 * @param {String} value The string value that represents this url. If ommitted, window.location
	 * is used instead.
	 */
	function Url(value)
	{
		if ($type.isString(value))
		{
			this.parse(value);
		}
		else
		{
			this.parse(location.href);
		}
	}

	Url.prototype.init = function Url$init()
	{
		this.components = {};
		this.hashParam = {};
		this.queryParam = {};
	};

	Url.prototype.getHash = function Url$getHash(includeHashSymbol)
	{
		var result = [];
		for (var name in this.hashParam)
			result.push(name + "=" + this.hashParam[name]);

		var value = result.join("&");
		if (value != $string.EMPTY && includeHashSymbol)
			return "#" + value;

		return value;
	};

	Url.prototype.getQuery = function Url$getQuery(includeQuestionMark)
	{
		var result = [];
		for (var name in this.queryParam)
			result.push(name + "=" + this.queryParam[name]);

		var value = result.join("&");
		if (value != $string.EMPTY && includeQuestionMark)
			return "?" + value;

		return value;
	};

	Url.prototype.getQueryParam = function Url$getQueryParam(itemKey)
	{
		return this.queryParam[itemKey];
	};

	Url.prototype.getHashParam = function Url$getHashParam(itemKey)
	{
		return this.hashParam[itemKey];
	};

	Url.prototype.setQueryParam = function Url$setQueryParam(itemKey, itemValue)
	{
		this.queryParam[itemKey] = itemValue;
	};

	Url.prototype.setHashParam = function Url$setHashParam(itemKey, itemValue)
	{
		return this.hashParam[itemKey] = itemValue;
	};

	Url.prototype.removeQueryParam = function Url$removeQueryParam(itemKey)
	{
		delete this.queryParam[itemKey];
	};

	Url.prototype.removeHashParam = function Url$removeHashParam(itemKey)
	{
		delete this.hashParam[itemKey];
	};

	Url.prototype.parse = function Url$parse(value)
	{
		if (value == null)
			return;

		this.init();

		var matches = URL_EXPRESSION.exec(value);
		var names = MATCH_NAMES;

		for (var i = names.length - 1; i > 0; i--)
			this.components[names[i]] = matches[i] || $string.EMPTY;

		var hash = this.components.hash;
		var query = this.components.query;

		if (hash == EMPTY_HASH)
			hash = $string.EMPTY;

		this.hashParam = url.parseQuery(hash);
		this.queryParam = url.parseQuery(query);
	};

	Url.prototype.toString = function Url$toString()
	{
		var prefix = this.components.protocol
			? $string.concat(this.components.protocol, PROTOCOL_SEPARATOR)
			: "";

		return $string.concat(
			prefix,
			this.components.authority,
			this.components.path,
			url.serializeParams(this.queryParam, "?", true),
			url.serializeParams(this.hashParam, "#", false)
		);

		return result;
	};

	var url = function makeUrl(value)
	{
		return new Url(value);
	};

	/**
	 * Combines the parent <c>folderUrl</c> with the child <c>fileUrl</c> into a single URL.
	 * @param {String} folderUrl The parent URL to combine, e.g. <c>my/directory/to/files</c>.
	 * @param {String} fileUrl The child URL to combine, e.g. <c>../file2.txt</c>
	 * @return {String} The combined URL.
	 * @example
	 * // the following returns "my/directory/to/file2.txt":
	 * var combined = combine("my/directory/to/files", "../file2.txt");
	 */
	url.combine = function Url$combine(folderUrl, fileUrl)
	{
		var filePath = $string.concat(folderUrl, "/", fileUrl);
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
	url.getQueryParam = function Url$getQueryParam(name, defaultValue)
	{
		return new Url().getQueryParam(name) || defaultValue;
	};

	url.getHashParam = function Url$getHashParam(name, defaultValue)
	{
		return new Url().getHashParam(name) || defaultValue;
	};

	url.getHash = function Url$getHash()
	{
		return String(location.hash).substring(1);
	};

	url.getQuery = function Url$getQuery()
	{
		return String(location.search).substring(1);
	};

	/**
	 * Sets the value of the specified hash parameter.
	 * If a single argument is passed as an object, it will be treated as a name/value collection. If two values are
	 * passed, they will be treated a single key/value pair.
	 * @example setHashParam("color", "red");
	 * @example setHashParam({ color: "red", size: "xlarge" });
	 */
	url.setHashParam = function Url$setHashParam()
	{
		var current = new Url(location.href);
		if (arguments.length == 2)
		{
			current.setHashParam(arguments[0], arguments[1]);
		}
		else if (arguments.length == 1)
		{
			if ($type.isArray(arguments[0]))
				for (var name in arguments[0])
					current.setHashParam(name, arguments[0][name]);
			else
				current.setHashParam(arguments[0], $string.EMPTY);
		}
		else
		{
			return;
		}

		this.setHash(current.getHash());
	};

	url.setHash = function Url$setHash(hash)
	{
		// ensure that setting the location hash to empty doesn't cause the page to scroll to the top
		if (hash == $string.EMPTY)
			hash = EMPTY_HASH;

		location.hash = hash;
	};

	url.removeHashParam = function Url$removeHashParam(name)
	{
		var current = new Url(location);
		current.removeHashParam(name);

		this.setHash(url.getHash());
	};

	url.getFileExtension = function Url$getFileExtension(file)
	{
		var dotIndex = file ? file.lastIndexOf(".") : -1;
		return (dotIndex > -1 ? file.slice(dotIndex + 1) : "");
	};

	url.getFileName = function Url$getFileName(file)
	{
		return file && file.match(/[^\\\/]*$/)[0];
	};

	url.parseQuery = function Url$parseQuery(value)
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
			var itemValue = pair[1] || $string.EMPTY;

			if (query[itemKey] != null)
			{
				if (!$type.isArray(query[itemKey]))
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

	url.serializeParams = function Url$serializeParams(params, prefix, encode)
	{
		var result = [];
		for (var name in params)
		{
			var itemName = encode ? encodeURIComponent(name) : name;
			var itemValue = encode ? encodeURIComponent(params[name]  || $string.EMPTY) : params[name];
			if (itemValue)
				result.push(itemName + "=" + itemValue);
			else
				result.push(itemName);
		}
		var value = result.join("&");
		if (prefix && value.length)
		{
			return $string.concat(prefix, value);
		}
		return value;
	};

	return url;
};

