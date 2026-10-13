namespace _Code.Infrastructure._NINAH__Cat;

public interface ICatViewProvider
{
	CatInstance Cat { get; }

	CatPosition[] DayPositions { get; }

	CatPosition[] NightPositions { get; }
}
