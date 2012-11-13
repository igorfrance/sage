Number.random = function Number$random(randTop)
{
	var top = Math.abs(randTop);
	var random = Math.round((top) * Math.random());
	return random;
};

Number.randomInRange = function Number$randomInRange(min, max, noRounding)
{
	var random = min + ((max - min) * Math.random());
	return noRounding ? random : Math.round(random);
};
