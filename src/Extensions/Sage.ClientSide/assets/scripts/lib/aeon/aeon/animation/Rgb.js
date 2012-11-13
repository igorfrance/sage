Type.registerNamespace("aeon.animation");

aeon.animation.Rgb = function Rgb(r, g, b)
{
	this.r = 0;
	this.g = 0;
	this.b = 0;

	if (r >= 0 && r <= 255) this.r = parseInt(r);
	if (g >= 0 && g <= 255) this.g = parseInt(g);
	if (b >= 0 && b <= 255) this.b = parseInt(b);
}
aeon.animation.Rgb.prototype.toString = function Rgb$toString()
{
	if (arguments[0] == "hex")
	{
		var r = this.decimalToHex(this.r);
		var g = this.decimalToHex(this.g);
		var b = this.decimalToHex(this.b);

		r = r.length == 1 ? "0" + r : r;
		g = g.length == 1 ? "0" + g : g;
		b = b.length == 1 ? "0" + b : b;

		return ("#" + r + g + b);
	}
	else
		return "rgb({0}, {1}, {2})".format(this.r, this.g, this.b);
}

aeon.animation.Rgb.HD ="0123456789ABCDEF";

aeon.animation.Rgb.decimalToHex = function Rgb$decimalToHex(value)
{
	var hex = aeon.animation.Rgb.HD.substr(value & 15, 1);
	while (value > 15)
	{
		value >>= 4;
		hex = aeon.animation.Rgb.HD.substr(value & 15, 1) + hex;
	}
	return hex;
}

aeon.animation.Rgb.parse = function Rgb$parse(value)
{
	var result = new aeon.animation.Rgb(0, 0, 0);
	if (value == null)
		return result;

	if (Type.instanceOf(value, aeon.animation.Rgb))
		return new aeon.animation.Rgb(value.r, value.g, value.b);

	var color = String(value);
	if (color.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/))
	{
		result = new aeon.animation.Rgb(parseInt(RegExp.$1), parseInt(RegExp.$2), parseInt(RegExp.$3));
	}
	else if (color.match(/^#([\da-fA-F])([\da-fA-F])([\da-fA-F])$/))
	{
		var r = parseInt(RegExp.$1 + RegExp.$1, 16);
		var g = parseInt(RegExp.$2 + RegExp.$2, 16);
		var b = parseInt(RegExp.$3 + RegExp.$3, 16);
		result = new aeon.animation.Rgb(r, g, b);
	}
	else if (color.match(/^#([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})$/))
	{
		var r = parseInt(RegExp.$1, 16);
		var g = parseInt(RegExp.$2, 16);
		var b = parseInt(RegExp.$3, 16);
		result = new aeon.animation.Rgb(r, g, b);
	}
	else if (aeon.animation.X11Colors[color] != null)
	{
		if (aeon.animation.X11Colors[color].match(/^#([\da-fA-F]){2}([\da-fA-F]){2}([\da-fA-F]){2}$/))
		{
			var r = parseInt(RegExp.$1, 16);
			var g = parseInt(RegExp.$2, 16);
			var b = parseInt(RegExp.$3, 16);
			result = new aeon.animation.Rgb(r, g, b);
		}
	}
	return result;
}

