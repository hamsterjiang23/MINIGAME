using AYellowpaper.SerializedCollections;
using UnityEngine;
using _Code.Characters;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.Endings.Data;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Randomization;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Windows;
using _Code.Infrastructure._NINAH__RandomSoundEmitters;

namespace _Code.Infrastructure
{
	[CreateAssetMenu(menuName = "Resource Mother")]
	public sealed class ResourceMother : ScriptableObject, ICharactersSODataProvider, IDayNightControllerSODataProvider, IGameEventsManagerSODataProvider, IOtherGameSODataProvider, ICursorDataProvider, IRandomGenerationSettingsSODataProvider, IWindowsSODataProvider, IEndingDataProvider, IEndingSODataProvider, IRandomSoundsEmitterSODataViewProvider, IMusicSOData
	{
		[SerializeField]
		private DayNightControllerSOData _dayNightControllerData;

		[SerializeField]
		private GameEventsManagerSOData _gameEventsManagerData;

		[SerializeField]
		private OtherGameSOData _otherGameData;

		[SerializeField]
		private CharacterSOData[] _charactersData;

		[SerializeField]
		private CursorSOData _cursorData;

		[SerializeField]
		private RandomGenerationSettingsSOData _randomGenerationSettingsData;

		[SerializeField]
		private WindowsSOData _windowsData;

		[SerializeField]
		private EndingViewSOData[] _endingsViewData;

		[SerializeField]
		private EndingsSOData _endingsData;

		[SerializeField]
		private CutsceneData[] _cutscenesData;

		[SerializeField]
		private RandomSoundsEmitterSOData _randomSoundsEmitterData;

		[SerializeField]
		private MusicSOData _musicSOData;

		public DayNightControllerSOData DayNightControllerData => null;

		public GameEventsManagerSOData GameEventsManagerData => null;

		public CharacterSOData[] CharactersData => null;

		public OtherGameSOData OtherGameData => null;

		public CursorSOData CursorData => null;

		public RandomGenerationSettingsSOData RandomGenerationSettingsSOData => null;

		public WindowsSOData GetWindowData => null;

		public EndingViewSOData[] EndingsData => null;

		public EndingsSOData EndingSOData => null;

		public RandomSoundsEmitterSOData RandomEmitterData => null;

		public SerializedDictionary<int, MusicDayData> MusicByDay => null;
	}
}
