var Transitions = new function Transitions()
{
	var Transition = Prototype.extend(function Transition(object)
	{
		if (object && $type.isFunction(object.init))
			this.init = object.init;

		if (object && $type.isFunction(object.render))
			this.render = object.render;
	});

	Transition.prototype.init = function Transition$render(image, transition)
	{
		this.options = $.extend({}, transition.options);
	};

	Transition.prototype.render = function Transition$render(image, transition, progress)
	{
	};

	var ClippedTransition = Prototype.extend(function Transition(clipFunction)
	{
		this.clipFunction = clipFunction;
	});

	ClippedTransition.prototype.render = function ClippedTransition$render(image, transition, progress)
	{
		var canvas = image.canvas;
		var context = image.canvas.getContext("2d");
		image.drawImage(transition.from);

		context.save();
		context.beginPath();
		this.clipFunction(context, canvas.width, canvas.height, progress);
		context.clip();
		image.drawImage(transition.to);
		context.restore();
	};

	this.clock = new ClippedTransition(function render(ctx, w, h, p)
	{
		ctx.moveTo(w / 2, h / 2);
		return ctx.arc(w / 2, h / 2, Math.max(w, h), 0, Math.PI * 2 * p, false);
	});

	this.circle = new ClippedTransition(function render(ctx, w, h, p)
	{
		return ctx.arc(w / 2, h / 2, 0.6 * p * Math.max(w, h), 0, Math.PI * 2, false);
	});

	this.diamond = new ClippedTransition(function render(ctx, w, h, p)
	{
		var dh, dw, h2, w2;
		w2 = w / 2;
		h2 = h / 2;
		dh = p * h;
		dw = p * w;
		ctx.moveTo(w2, h2 - dh);
		ctx.lineTo(w2 + dw, h2);
		ctx.lineTo(w2, h2 + dh);
		return ctx.lineTo(w2 - dw, h2);
	});

	this.verticalOpen = new ClippedTransition(function render(ctx, w, h, p)
	{
		var h1, h2, hi, nbSpike, pw, spikeh, spikel, spiker, spikew, xl, xr, _results;
		nbSpike = 8;
		spikeh = h / (2 * nbSpike);
		spikew = spikeh;
		pw = p * w / 2;
		xl = w / 2 - pw;
		xr = w / 2 + pw;
		spikel = xl - spikew;
		spiker = xr + spikew;
		ctx.moveTo(xl, 0);
		for (hi = 0; 0 <= nbSpike ? hi <= nbSpike : hi >= nbSpike; 0 <= nbSpike ? hi++ : hi--)
		{
			h1 = (2 * hi) * spikeh;
			h2 = h1 + spikeh;
			ctx.lineTo(spikel, h1);
			ctx.lineTo(xl, h2);
		}
		ctx.lineTo(spiker, h);
		_results = [];
		for (hi = nbSpike; nbSpike <= 0 ? hi <= 0 : hi >= 0; nbSpike <= 0 ? hi++ : hi--)
		{
			h1 = (2 * hi) * spikeh;
			h2 = h1 - spikeh;
			ctx.lineTo(xr, h1);
			_results.push(ctx.lineTo(spiker, h2));
		}
		return _results;
	});

	this.horizontalOpen = new ClippedTransition(function render(ctx, w, h, p)
	{
		return context.rect(0, (1 - progress) * height / 2, width, height * progress);
	});

	this.horizontalSunblind = new ClippedTransition(function render(ctx, w, h, p)
	{
		var blind, blindHeight, blinds, _results;
		p = 1 - (1 - p) * (1 - p);
		blinds = 6;
		blindHeight = h / blinds;
		_results = [];
		for (blind = 0; 0 <= blinds ? blind <= blinds : blind >= blinds; 0 <= blinds ? blind++ : blind--)
		{
			_results.push(ctx.rect(0, blindHeight * blind, w, blindHeight * p));
		}
		return _results;
	});

	this.verticalSunblind = new ClippedTransition(function render(ctx, w, h, p)
	{
		var blind, blindWidth, blinds, prog, _results;
		p = 1 - (1 - p) * (1 - p);
		blinds = 10;
		blindWidth = w / blinds;
		_results = [];
		for (blind = 0; 0 <= blinds ? blind <= blinds : blind >= blinds; 0 <= blinds ? blind++ : blind--)
		{
			prog = Math.max(0, Math.min(2 * p - (blind + 1) / blinds, 1));
			_results.push(ctx.rect(blindWidth * blind, 0, blindWidth * prog, h));
		}
		return _results;
	});

	this.circles = new ClippedTransition(function render(ctx, w, h, p)
	{
		var circleH, circleW, circlesX, circlesY, cx, cy, maxRad, maxWH, r, x, y, _results;
		circlesY = 6;
		circlesX = Math.floor(circlesY * w / h);
		circleW = w / circlesX;
		circleH = h / circlesY;
		maxWH = Math.max(w, h);
		maxRad = 0.7 * Math.max(circleW, circleH);
		_results = [];
		for (x = 0; 0 <= circlesX ? x <= circlesX : x >= circlesX; 0 <= circlesX ? x++ : x--)
		{
			_results.push((function ()
			{
				var _results2;
				_results2 = [];
				for (y = 0; 0 <= circlesY ? y <= circlesY : y >= circlesY; 0 <= circlesY ? y++ : y--)
				{
					cx = (x + 0.5) * circleW;
					cy = (y + 0.5) * circleH;
					r = Math.max(0, Math.min(2 * p - cx / w, 1)) * maxRad;
					ctx.moveTo(cx, cy);
					_results2.push(ctx.arc(cx, cy, r, 0, Math.PI * 2, false));
				}
				return _results2;
			})());
		}
		return _results;
	});

	this.squares = new ClippedTransition(function render(ctx, w, h, p)
	{
		var blindHeight, blindWidth, blindsX, blindsY, prog, rh, rw, sx, sy, x, y, _results;
		p = 1 - (1 - p) * (1 - p);
		blindsY = 5;
		blindsX = Math.floor(blindsY * w / h);
		blindWidth = w / blindsX;
		blindHeight = h / blindsY;
		_results = [];
		for (x = 0; 0 <= blindsX ? x <= blindsX : x >= blindsX; 0 <= blindsX ? x++ : x--)
		{
			_results.push((function ()
			{
				var _results2;
				_results2 = [];
				for (y = 0; 0 <= blindsY ? y <= blindsY : y >= blindsY; 0 <= blindsY ? y++ : y--)
				{
					sx = blindWidth * x;
					sy = blindHeight * y;
					prog = Math.max(0, Math.min(3 * p - sx / w - sy / h, 1));
					rw = blindWidth * prog;
					rh = blindHeight * prog;
					_results2.push(ctx.rect(sx - rw / 2, sy - rh / 2, rw, rh));
				}
				return _results2;
			})());
		}
		return _results;
	});

	this.fadeLeft = new Transition(
	{
		init: function init(self, transition)
		{
			this.options = $.extend({ direction: "right" }, transition.options);

			transition.randomTrait = [];
			transition.fromData = image.getData(transition.from);
			transition.toData = image.getData(transition.to);
			transition.output = self.context.createImageData(self.canvas.width, self.canvas.height);

			var h = self.canvas.height;
			for (var i = 0; 0 <= h ? i <= h : i >= h; 0 <= h ? i++ : i--)
				transition.randomTrait[i] = Math.random();
		},

		render: function render(self, transition, progress, data)
		{
			var blur = 150;
			var height = self.canvas.height;
			var width = self.canvas.width;
			var fd = transition.fromData.data;
			var td = transition.toData.data;
			var out = transition.output.data;

			var wpdb = width * progress / blur;
			for (var x = 0; x < width; ++x)
			{
				var xdb = x / blur;
				for (var y = 0; y < self.canvas.height; ++y)
				{
					var b = (y * width + x) * 4
					var p1 = Math.min(Math.max((xdb - wpdb * (1 + transition.randomTrait[y] / 10)), 0), 1)
					var p2 = 1 - p1
					for (var c = 0; c < 3; ++c)
					{
						var i = b + c;
						out[i] = p1 * (fd[i]) + p2 * (td[i])
					}
					out[b + 3] = 255;
				}
			}

			return self.context.putImageData(transition.output, 0, 0);
		}
	});

};

