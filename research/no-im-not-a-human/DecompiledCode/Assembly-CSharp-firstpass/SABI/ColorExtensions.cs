using UnityEngine;

namespace SABI;

public static class ColorExtensions
{
	public static Color With(this Color color, int channel, float value)
	{
		color[channel] = value;
		return color;
	}

	public static Color WithR(this Color color, float r)
	{
		return color.With(0, r);
	}

	public static Color WithG(this Color color, float g)
	{
		return color.With(1, g);
	}

	public static Color WithB(this Color color, float b)
	{
		return color.With(2, b);
	}

	public static Color WithA(this Color color, float a)
	{
		return color.With(3, a);
	}

	public static Color With(this Color color, int channel1, float value1, int channel2, float value2)
	{
		color[channel1] = value1;
		color[channel2] = value2;
		return color;
	}

	public static Color WithRG(this Color color, float r, float g)
	{
		return color.With(0, r, 1, g);
	}

	public static Color WithRB(this Color color, float r, float b)
	{
		return color.With(0, r, 2, b);
	}

	public static Color WithRA(this Color color, float r, float a)
	{
		return color.With(0, r, 3, a);
	}

	public static Color WithGB(this Color color, float g, float b)
	{
		return color.With(1, g, 2, b);
	}

	public static Color WithGA(this Color color, float g, float a)
	{
		return color.With(1, g, 3, a);
	}

	public static Color WithBA(this Color color, float b, float a)
	{
		return color.With(2, b, 3, a);
	}

	public static Color With(this Color color, int channel1, float value1, int channel2, float value2, int channel3, float value3)
	{
		color[channel1] = value1;
		color[channel2] = value2;
		color[channel3] = value3;
		return color;
	}

	public static Color WithRGB(this Color color, float r, float g, float b)
	{
		return color.With(0, r, 1, g, 2, b);
	}

	public static Color WithRGA(this Color color, float r, float g, float a)
	{
		return color.With(0, r, 1, g, 3, a);
	}

	public static Color WithRBA(this Color color, float r, float b, float a)
	{
		return color.With(0, r, 2, b, 3, a);
	}

	public static Color WithGBA(this Color color, float g, float b, float a)
	{
		return color.With(1, g, 2, b, 3, a);
	}
}
