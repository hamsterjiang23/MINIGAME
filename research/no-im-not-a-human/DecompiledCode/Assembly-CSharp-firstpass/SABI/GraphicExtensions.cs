using UnityEngine;
using UnityEngine.UI;

namespace SABI;

public static class GraphicExtensions
{
	public static Graphic SetColorR(this Graphic graphic, float colorR)
	{
		graphic.color = graphic.color.WithR(colorR);
		return graphic;
	}

	public static Graphic SetColorG(this Graphic graphic, float colorG)
	{
		graphic.color = graphic.color.WithG(colorG);
		return graphic;
	}

	public static Graphic SetColorB(this Graphic graphic, float colorB)
	{
		graphic.color = graphic.color.WithB(colorB);
		return graphic;
	}

	public static Graphic SetColorA(this Graphic graphic, float colorA)
	{
		graphic.color = graphic.color.WithA(colorA);
		return graphic;
	}

	public static Graphic SetColorRGB(this Graphic graphic, float colorR, float colorG, float colorB)
	{
		graphic.color = graphic.color.WithRGB(colorR, colorG, colorB);
		return graphic;
	}

	public static Graphic SetColorRGB(this Graphic graphic, float colorR, float colorG, float colorB, float colorA)
	{
		graphic.color = graphic.color.WithRGB(colorR, colorG, colorB).WithA(colorA);
		return graphic;
	}

	public static Graphic SetColorRGB(this Graphic graphic, Color color)
	{
		graphic.color = color.WithA(graphic.color.a);
		return graphic;
	}
}
