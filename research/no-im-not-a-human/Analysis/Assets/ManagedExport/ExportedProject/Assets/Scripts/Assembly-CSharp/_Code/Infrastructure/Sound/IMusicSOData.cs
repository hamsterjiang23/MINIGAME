using AYellowpaper.SerializedCollections;

namespace _Code.Infrastructure.Sound
{
	public interface IMusicSOData
	{
		SerializedDictionary<int, MusicDayData> MusicByDay { get; }
	}
}
