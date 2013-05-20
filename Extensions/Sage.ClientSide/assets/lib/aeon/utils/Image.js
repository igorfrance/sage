/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides various image manipulation and processing utilities.
 * @type Object
 */
var $image = new function image()
{
	var $image = this;

	var XImage = HtmlControl.extend(function XImage(element)
	{
		this.construct(element, "start", "progress", "done");

		var canvasSupported = isCanvasSupported();
		if (canvasSupported)
		{
			var tagName = this.$element.prop("tagName");
			if (tagName == "CANVAS")
			{
				this.canvas = this.$element[0];
			}
			else
			{
				var canvas = this.$element.parent().find("canvas")[0];
				if (canvas == null)
				{
					canvas = document.createElement("canvas");
					canvas.width = this.$element.width();
					canvas.height = this.$element.height();

					this.$element.parent().append(canvas);
				}

				if (tagName == "IMG")
				{
					var img = new Image();
					var me = this;
					img.onload = function onload()
					{
						if (canvas.width == 0 || canvas.height == 0)
						{
							canvas.width = img.width;
							canvas.height = img.height;
						}

						var context = canvas.getContext("2d");
						context.drawImage(img, 0, 0, canvas.width, canvas.height);
					}

					img.src = this.$element.prop("src");
				}

				this.canvas = canvas;
				this.context = canvas.getContext("2d");
				this.$element.hide();
			}
		}
		else
		{
			this.$image2 = $("<img/>");
			for (var i = 0; i < this.$element[0].attributes.length; i++)
			{
				var attr = this.$element[0].attributes[i];
				this.$image2.attr(attr.name, attr.value);
			}

			this.$image2.hide();
			this.$image2.addClass("image2");
			this.$element.parent().append(this.$image2);
		}
	});

	XImage.prototype.drawImage = function XImage$drawImage(image)
	{
		this.clear();
		return this.context.drawImage(image, 0, 0,
			this.canvas.width,
			this.canvas.width * image.height / image.width);
	};

	XImage.prototype.cloneCurrent = function XImage$cloneCurrent()
	{
		var result = document.createElement("canvas");
		result.width = this.canvas.width;
		result.height = this.canvas.height;
		result.getContext("2d").drawImage(this.canvas, 0, 0, result.width, result.height);
		return result;
	};

	XImage.prototype.clear = function XImage$clear()
	{
		this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
	};

	XImage.prototype.transition = function XImage$transition(options)
	{
		var transition = this.__transition = $.extend(image.transitionDefaults, options);

		if (transition.start)
			this.on("start", transition.start);
		if (transition.progress)
			this.on("progress", transition.progress);
		if (transition.done)
			this.on("done", transition.done);

		transition.start = $date.time();
		transition.end = transition.start + transition.duration;

		if (this.canvas != null)
			transitionUsingCanvas.call(this, options);
		else
			transitionUsingOpacity.call(this, options);
	};

	function transitionUsingCanvas(options)
	{
		var me = this;
		var transition = this.__transition;

		transition.effect = image.Transitions[transition.effect] || image.Transitions.horizontalOpen;
		transition.easingFx = $easing[transition.easing] || $easing.easeOutQuad;
		transition.from = this.cloneCurrent();

		function beginTransition()
		{
			if (transition.effect.init)
				transition.effect.init(me, transition);

			me.fireEvent("start");
			me.animating = true;
			schedule(proxy(renderTransition, me));
		}

		if (transition.to instanceof Image)
			beginTransition();
		else
		{
			var src = transition.to;
			transition.to = new Image();
			transition.to.onload = beginTransition;
			transition.to.src = src;
		}
	}

	function transitionUsingOpacity(options)
	{
		var me = this;

		function beginTransition()
		{
			me.$image2.attr("src", options.to.src);
			me.$image2.css({ opacity: 0, display: "block" });
			me.$image2.animate({ opacity: 1 });
			me.$element.animate({ opacity: 0 },
			{
				duration: options.duration,
				easing: options.easing,
				progress: function progress(promise, progress, remaining) /**/
				{
					me.fireEvent("progress", { progress: progress });
				},
				complete: function complete() /**/
				{
					var img2 = me.$image2;
					var elem = me.$element;
					me.$element.hide().addClass("image2");
					me.$image2.removeClass("image2");
					me.$element = img2;
					me.$image2 = elem;
					me.fireEvent("done");
				}
			});

			me.fireEvent("start");
		}

		if (options.to instanceof Image)
		{
			beginTransition();
		}
		else
		{
			options.to = new Image();
			options.to.onload = beginTransition;
			options.to.src = src;
		}
	}

	function renderTransition()
	{
		var time = $date.time();
		var transition = this.__transition;
		var progress = transition.easingFx(time - transition.start, 0, 1, transition.duration);

		if (time >= transition.end)
		{
			this.clear();
			this.fireEvent("done");
			this.animating = false;
			return this.drawImage(transition.to);
		}
		else
		{
			transition.effect.render(this, transition, progress);
			this.fireEvent("progress", { progress: progress });
			schedule(proxy(renderTransition, this));
		}
	};

	function image(elem)
	{
		return new XImage(elem);
	}

	image.getData = function XImage$getData(source)
	{
		var temp = document.createElement("canvas");
		temp.width = source.width;
		temp.height = source.height;

		var context = temp.getContext("2d");
		context.drawImage(source, 0, 0, temp.width, temp.height);

		var result = context.getImageData(0, 0, temp.width, temp.height);
		temp = null;
		context = null;
		return result;
	};

	image.transitionDefaults =
	{
		duration: 1000,
		transition: "horizontalOpen",
		easing: "easeOutExpo",
	};

	/*#include: Image.Transitions.js */
	image.Transitions	= Transitions;

	var schedule =
		window.requestAnimationFrame ||
		window.webkitRequestAnimationFrame ||
		window.mozRequestAnimationFrame ||
		window.oRequestAnimationFrame ||
		window.msRequestAnimationFrame ||
			function schedule(callback, element) { window.setTimeout(callback, 1000 / 60); };

	var proxy = function (fn, me)
	{
		return function proxy()
		{
			return fn.apply(me, arguments);
		};
	};

	function isCanvasSupported()
	{
		var canvas = document.createElement("canvas");
		return !(canvas == null || canvas.getContext == null || canvas.getContext("2d") == null);
	}


	return image;
};
