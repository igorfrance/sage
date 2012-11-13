Type.registerNamespace("aeon.animation");

/**
 * Calculate the current value without any easing
 * @param {Number} t Elapsed time (in milliseconds)
 * @param {Number} b Begin value
 * @param {Number} c The current difference (between begin value and end value)
 * @param {Number} d The duration of transition (in milliseconds)
 */
Math.easeNone = function Math$easeNone(t, b, c, d)
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
Math.easeIn = function Math$easeIn(t, b, c, d)
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
Math.easeOut = function Math$easeOut(t, b, c, d)
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
Math.easeInOut = function Math$easeInOut(t, b, c, d)
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
Math.easeInQuad = function Math$easeInQuad(t, b, c, d)
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
Math.easeOutQuad = function Math$easeOutQuad(t, b, c, d)
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
Math.easeInOutQuad = function Math$easeInOutQuad(t, b, c, d)
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
Math.easeInCubic = function Math$easeInCubic(t, b, c, d)
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
Math.easeOutCubic = function Math$easeOutCubic(t, b, c, d)
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
Math.easeInOutCubic = function Math$easeInOutCubic(t, b, c, d)
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
Math.easeInQuart = function Math$easeInQuart(t, b, c, d)
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
Math.easeOutQuart = function Math$easeOutQuart(t, b, c, d)
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
Math.easeInOutQuart = function Math$easeInOutQuart(t, b, c, d)
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
Math.easeInQuint = function Math$easeInQuint(t, b, c, d)
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
Math.easeOutQuint = function Math$easeOutQuint(t, b, c, d)
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
Math.easeInOutQuint = function Math$easeInOutQuint(t, b, c, d)
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
Math.easeInExpo = function Math$easeInExpo(t, b, c, d)
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
Math.easeOutExpo = function Math$easeOutExpo(t, b, c, d)
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
Math.easeInOutExpo = function Math$easeInOutExpo(t, b, c, d)
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
Math.easeInSine = function Math$easeInSine(t, b, c, d)
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
Math.easeOutSine = function Math$easeOutSine(t, b, c, d)
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
Math.easeInOutSine = function Math$easeInOutSine(t, b, c, d)
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
Math.easeInCirc = function Math$easeInCirc(t, b, c, d)
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
Math.easeOutCirc = function Math$easeOutCirc(t, b, c, d)
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
Math.easeInOutCirc = function Math$easeInOutCirc(t, b, c, d)
{
	if ((t /= d/2) < 1)
		return -c/2 * (Math.sqrt(1 - t * t) - 1) + b;

	return c/2 * (Math.sqrt(1 - (t -= 2) * t) + 1) + b;
}

// BACK
Math.easeInBack = function Math$easeInBack(t, b, c, d, s)
{
	if (s == undefined)
		s = 1.70158;

	return c * (t /= d) * t * ((s + 1) * t - s) + b;
}
Math.easeOutBack = function Math$easeOutBack(t, b, c, d, s)
{
	if (s == undefined)
		s = 1.70158;

	return c * ((t = t/d - 1) * t * ((s + 1) * t + s) + 1) + b;
}
Math.easeInOutBack = function Math$easeInOutBack(t, b, c, d, s)
{
	if (s == undefined)
		s = 1.70158;
	if ((t /= d/2) < 1)
		return c/2 * (t * t * (((s *= (1.525)) + 1) * t - s)) + b;

	return c/2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2) + b;
}

// ELASTIC
Math.easeInElastic = function Math$easeInElastic(t, b, c, d, a, p)
{
	if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
	if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
	else var s = p/(2*Math.PI) * Math.asin (c/a);
	return -(a*Math.pow(2,10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )) + b;
}
Math.easeOutElastic = function Math$easeOutElastic(t, b, c, d, a, p)
{
	if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
	if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
	else var s = p/(2*Math.PI) * Math.asin (c/a);
	return (a*Math.pow(2,-10*t) * Math.sin( (t*d-s)*(2*Math.PI)/p ) + c + b);
}
Math.easeInOutElastic = function Math$easeInOutElastic(t, b, c, d, a, p)
{
	if (t==0) return b;  if ((t/=d/2)==2) return b+c;  if (!p) p=d*(.3*1.5);
	if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
	else var s = p/(2*Math.PI) * Math.asin (c/a);
	if (t < 1) return -.5*(a*Math.pow(2,10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )) + b;
	return a*Math.pow(2,-10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )*.5 + c + b;
}

// BOUNCE
Math.easeInBounce = function Math$easeInBounc(t, b, c, d)
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
Math.easeOutBounce = function Math$easeOutBounce(t, b, c, d)
{
	return c - Math.easeOut(d-t, 0, c, d) + b;
}
Math.easeInOutBounce = function Math$easeInOutBounce(t, b, c, d)
{
	if (t < d/2)
		return Math.easeIn(t * 2, 0, c, d) * .5 + b;
	else
		return Math.easeOut(t * 2 - d, 0, c, d) * .5 + c * .5 + b;
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
Math.sineLinear = function Math$sineLinear(t, b, c, d, a, p)
{
	a = a;
	d = d / 1000;
	t = 1000 - t;

	var value = a * Math.sin(d * t);

	$log.message("Y: {0} * Math.sin(({1} * {2})) = {3}", a, d, t, value);
	return value;
}

