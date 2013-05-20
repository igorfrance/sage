/*!
 * aeon.js  v.2.0
 *
 * Copyright 2012 Igor France
 * Licensed under the MIT License
 */
var aeon = new function aeon()
{
	var aeon = {};
	var $ = window.jQuery;

	/*#include: core/Type.js  */
	/*#include: core/String.js */
	/*#include: core/Number.js */
	/*#include: core/Array.js */
	/*#include: core/Error.js */
	/*#include: core/Date.js  */
	/*#include: core/Function.js  */
	/*#include: core/Prototype.js  */
	/*#include: core/Dispatcher.js  */
	/*#include: core/Settings.js  */
	/*#include: core/QueryString.js  */

	/*#include: controls/ControlTypeInfo.js  */
	/*#include: controls/ControlRegistry.js  */
	/*#include: controls/HtmlControl.js  */
	/*include: controls/PageControl.js  */

	/*#include: utils/Css.js  */
	/*#include: utils/Dom.js  */
	/*#include: utils/Log.js  */
	/*#include: utils/Url.js  */
	/*#include: utils/Cookie.js  */
	/*#include: utils/Xml.js  */
	/*#include: utils/Initializer.js  */
	/*#include: utils/Constants.js  */
	/*#include: utils/Event.js  */
	/*#include: utils/Dragger.js  */
	/*#include: utils/Easing.js  */
	/*#include: utils/Image.js  */

	aeon.Prototype = Prototype;
	aeon.Dispatcher = Dispatcher;
	aeon.Settings = Settings;
	aeon.QueryString = QueryString;
	aeon.Initializer = Initializer;
	aeon.HtmlControl = HtmlControl;

	aeon.type = $type;
	aeon.string = $string;
	aeon.array = $array;
	aeon.date = $date;
	aeon.error = $error;
	aeon.log = $log;
	aeon.css = $css;
	aeon.dom = $dom;
	aeon.url = $url;
	aeon.cookie = $cookie;
	aeon.image = $image;
	aeon.xml = $xml;
	aeon.const = $const;
	aeon.event = $evt;
	aeon.drag = $drag;
	aeon.easing = $easing;

	aeon.controls = new ControlRegistry;

	aeon.init = new Initializer;
	aeon.init.register(aeon.controls, true);

	$(window)
		.on("ready", aeon.init.setup)
		.on("unload", aeon.init.dispose);

	return aeon;
};
