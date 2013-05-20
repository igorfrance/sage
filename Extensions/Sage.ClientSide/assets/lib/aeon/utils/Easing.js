var $easing = new function Easing()
{
	/**
	 * Calculate the current value without any easing
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeNone = function easeNone(t, b, c, d)
	{
		return c * t/ d + b;
	}

	/**
	 * Calculate the current value without any easing
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeIn = function easeIn(t, b, c, d)
	{
		return c * t / d + b;
	}

	/**
	 * Calculate the current value without any easing
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOut = function easeOut(t, b, c, d)
	{
		return c * t / d + b;
	}

	/**
	 * Calculate the current value without any easing
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOut = function easeInOut(t, b, c, d)
	{
		return c * t / d + b;
	}

	/**
	 * Calculate the current value with quad (^2) easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInQuad = function easeInQuad(t, b, c, d)
	{
		return c * (t /= d) * t + b;
	}

	/**
	 * Calculate the current value with quad (^2) easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutQuad = function easeOutQuad(t, b, c, d)
	{
		return -c * ( t/= d) * (t - 2) + b;
	}

	/**
	 * Calculate the current value with quad (^2) easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutQuad = function easeInOutQuad(t, b, c, d)
	{
		if ((t /= d / 2) < 1)
			return c / 2 * t * t + b;

		return -c / 2 * ((--t) * (t-2) - 1) + b;
	}

	/**
	 * Calculate the current value with cubic (^3) easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInCubic = function easeInCubic(t, b, c, d)
	{
		return c * (t /= d) * t * t + b;
	}

	/**
	 * Calculate the current value with cubic (^3) easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutCubic = function easeOutCubic(t, b, c, d)
	{
		return c * ((t = t/d - 1) * t * t + 1) + b;
	}

	/**
	 * Calculate the current value with cubic (^3) easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutCubic = function easeInOutCubic(t, b, c, d)
	{
		if ((t /= d/2) < 1)
			return c/2 * t * t * t + b;

		return c/2 * ((t -= 2) * t * t + 2) + b;
	}

	/**
	 * Calculate the current value with quadruple (^4) easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInQuart = function easeInQuart(t, b, c, d)
	{
		return c * (t /= d) * t * t * t + b;
	}

	/**
	 * Calculate the current value with quadruple (^4) easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutQuart = function easeOutQuart(t, b, c, d)
	{
		return -c * ((t = t/d - 1) * t * t * t - 1) + b;
	}

	/**
	 * Calculate the current value with quadruple (^4) easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutQuart = function easeInOutQuart(t, b, c, d)
	{
		if ((t /= d/2) < 1)
			return c/2 * t * t * t * t + b;

		return -c/2 * ((t -= 2) * t * t * t - 2) + b;
	}

	/**
	 * Calculate the current value with quntiple (^5) easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInQuint = function easeInQuint(t, b, c, d)
	{
		return c * (t /= d) * t * t * t * t + b;
	}

	/**
	 * Calculate the current value with quntiple (^5) easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutQuint = function easeOutQuint(t, b, c, d)
	{
		return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
	}

	/**
	 * Calculate the current value with quntiple (^5) easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutQuint = function easeInOutQuint(t, b, c, d)
	{
		if ((t /= d / 2) < 1)
			return c / 2 * t * t * t * t * t + b;

		return c/2 * ((t -= 2) * t * t * t * t + 2) + b;
	}

	/**
	 * Calculate the current value with exponential easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInExpo = function easeInExpo(t, b, c, d)
	{
		return (t == 0) ? b :  c * Math.pow(2, 10 * (t/d - 1)) + b;
	}

	/**
	 * Calculate the current value with exponential easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutExpo = function easeOutExpo(t, b, c, d)
	{
		return (t == d) ? b+c :  c * (-Math.pow(2, -10 * t/d) + 1) + b;
	}

	/**
	 * Calculate the current value with exponential easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutExpo = function easeInOutExpo(t, b, c, d)
	{
		if (t == 0) return b;
		if (t == d) return b + c;
		if ((t /= d/2) < 1)
			return c/2 * Math.pow(2, 10 * (t - 1)) + b;

		return c/2 * (-Math.pow(2, -10 * --t) + 2) + b;
	}

	/**
	 * Calculate the current value with sinusoid easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInSine = function easeInSine(t, b, c, d)
	{
		return -c * Math.cos(t/d * (Math.PI/2)) + c + b;
	}

	/**
	 * Calculate the current value with sinusoid easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutSine = function easeOutSine(t, b, c, d)
	{
		return c * Math.sin(t/d * (Math.PI/2)) + b;
	}

	/**
	 * Calculate the current value with sinusoid easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutSine = function easeInOutSine(t, b, c, d)
	{
		return -c/2 * (Math.cos(Math.PI*t/d) - 1) + b;
	}

	/**
	 * Calculate the current value with circular easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInCirc = function easeInCirc(t, b, c, d)
	{
		return -c * (Math.sqrt(1 - (t /= d) * t) - 1) + b;
	}

	/**
	 * Calculate the current value with circular easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutCirc = function easeOutCirc(t, b, c, d)
	{
		return c * Math.sqrt(1 - (t = t/d - 1) * t) + b;
	}

	/**
	 * Calculate the current value with circular easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutCirc = function easeInOutCirc(t, b, c, d)
	{
		if ((t /= d/2) < 1)
			return -c/2 * (Math.sqrt(1 - t * t) - 1) + b;

		return c/2 * (Math.sqrt(1 - (t -= 2) * t) + 1) + b;
	}

	// BACK
	this.easeInBack = function easeInBack(t, b, c, d, s)
	{
		if (s == undefined)
			s = 1.70158;

		return c * (t /= d) * t * ((s + 1) * t - s) + b;
	}
	this.easeOutBack = function easeOutBack(t, b, c, d, s)
	{
		if (s == undefined)
			s = 1.70158;

		return c * ((t = t/d - 1) * t * ((s + 1) * t + s) + 1) + b;
	}
	this.easeInOutBack = function easeInOutBack(t, b, c, d, s)
	{
		if (s == undefined)
			s = 1.70158;
		if ((t /= d/2) < 1)
			return c/2 * (t * t * (((s *= (1.525)) + 1) * t - s)) + b;

		return c/2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2) + b;
	}

	// ELASTIC
	this.easeInElastic = function easeInElastic(t, b, c, d, a, p)
	{
		if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
		if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		return -(a*Math.pow(2,10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )) + b;
	}
	this.easeOutElastic = function easeOutElastic(t, b, c, d, a, p)
	{
		if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
		if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		return (a*Math.pow(2,-10*t) * Math.sin( (t*d-s)*(2*Math.PI)/p ) + c + b);
	}
	this.easeInOutElastic = function easeInOutElastic(t, b, c, d, a, p)
	{
		if (t==0) return b;  if ((t/=d/2)==2) return b+c;  if (!p) p=d*(.3*1.5);
		if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		if (t < 1) return -.5*(a*Math.pow(2,10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )) + b;
		return a*Math.pow(2,-10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )*.5 + c + b;
	}

	// BOUNCE
	this.easeInBounce = function easeInBounc(t, b, c, d)
	{
		if ((t /= d) < (1/2.75))
			return c * (7.5625 * t * t) + b;
		else if (t < (2/2.75))
			return c * (7.5625 * (t -= (1.5/2.75)) * t + .75) + b;
		else if (t < (2.5/2.75))
			return c * (7.5625 * (t -= (2.25/2.75)) * t + .9375) + b;
		else
			return c * (7.5625 * (t-= (2.625/2.75)) * t + .984375) + b;
	}

	this.easeOutBounce = function easeOutBounce(t, b, c, d)
	{
		return c - this.easeOut(d-t, 0, c, d) + b;
	}

	this.easeInOutBounce = function easeInOutBounce(t, b, c, d)
	{
		if (t < d/2)
			return this.easeIn(t * 2, 0, c, d) * .5 + b;
		else
			return this.easeOut(t * 2 - d, 0, c, d) * .5 + c * .5 + b;
	}

	/**
	 * Calculate the current value using a sine wave
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 * @param {Number} a The amplitude (in pixels) of the sinal curve
	 * @param {Number} p The phase (in pixels) of the sinal curve
	 */
	this.sineLinear = function sineLinear(t, b, c, d, a, p)
	{
		a = a;
		d = d / 1000;
		t = 1000 - t;

		var value = a * Math.sin(d * t);

		$log("Y: {0} * Math.sin(({1} * {2})) = {3}", a, d, t, value);
		return value;
	}

	var extension = {};
	for (var name in this)
	{
		if ($type.isFunction(this[name]))
		{
			extension[name] = function proxy()
			{
				var targetFx = arguments.callee.fx;
				return targetFx.apply(this, $array.fromArguments(arguments, 1));
			};

			extension[name].fx = this[name];
		}
	}

	extension.def = "easeOutQuad";
	extension.swing = function (x, t, b, c, d)
	{
		return jQuery.easing[jQuery.easing.def](x, t, b, c, d);
	}

	jQuery.extend(jQuery.easing, extension);
};
