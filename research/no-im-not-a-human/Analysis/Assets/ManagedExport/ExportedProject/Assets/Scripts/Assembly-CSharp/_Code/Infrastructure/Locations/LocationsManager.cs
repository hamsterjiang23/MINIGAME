using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Zenject;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Menues.HUD;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Locations
{
	public sealed class LocationsManager : ASavableClass<LocationSaveData>, ILocationsManager, IInitializable, IDisposable
	{
		private LocationSaveData _saveData = new LocationSaveData();

		private readonly IHUDPresenter _hudPresenter;

		private readonly IPlayerService _playerService;

		private readonly Location[] _locationsList;

		private readonly IEffectsController _effectsController;

		private Location _currentLocation;

		private readonly IDialogManager _dialogManager;

		private readonly CharacterSOData[] _charactersSOData;

		private readonly INotAHumanSoundService _soundService;

		private readonly IDayNightController _dayNightController;

		private readonly IDataModelService _dataModelService;

		private const float FADE_DURATION = 0.5f;

		public ELocation CurrentLocation => _saveData.CurrentLocation;

		public bool IsGoingThroughLocations { get; private set; }

		public event Action<ELocation> LocationChanged;

		public LocationsManager(ILocationsViewProvider viewProvider, IHUDPresenter hudPresenter, IPlayerService playerService, IDialogManager dialogManager, IEffectsController effectsController, ICharactersSODataProvider charactersSODataProvider, INotAHumanSoundService soundService, IDayNightController dayNightController, IDataModelService dataModelService)
		{
			_hudPresenter = hudPresenter;
			_playerService = playerService;
			_locationsList = viewProvider.Locations;
			_currentLocation = _locationsList[0];
			_effectsController = effectsController;
			_dialogManager = dialogManager;
			_charactersSOData = charactersSODataProvider.CharactersData;
			_soundService = soundService;
			_dayNightController = dayNightController;
			_dataModelService = dataModelService;
			for (int i = 1; i < _locationsList.Length; i++)
			{
				_locationsList[i].gameObject.SetActive(value: false);
			}
			dialogManager.WentToLocation -= OnWentToLocation;
			dialogManager.WentToLocation += OnWentToLocation;
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}

		public void Initialize()
		{
		}

		private void OnWentToLocation(ELocation location)
		{
			GoToLocation(location).Forget();
		}

		public async UniTask GoToLocation(ELocation location)
		{
			IsGoingThroughLocations = true;
			Location newLocation = _locationsList.FirstOrDefault((Location x) => x.LocationType == location);
			if (!(newLocation == null))
			{
				_playerService.MakeImmovable();
				ETimeOfDay timeOfDay = _dayNightController.CurrentTimeOfDay;
				if (timeOfDay == ETimeOfDay.Day)
				{
					_soundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceDay, 0.5f).Forget();
				}
				else
				{
					_soundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceNight, 0.5f).Forget();
				}
				_soundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceAmbient, 0.5f).Forget();
				await _hudPresenter.FadeIn(0.5f);
				_playerService.TeleportTo(newLocation.StartPoint);
				_currentLocation.gameObject.SetActive(value: false);
				newLocation.gameObject.SetActive(value: true);
				_currentLocation = newLocation;
				_playerService.MakeMovable();
				await SetMusic(newLocation, timeOfDay);
				LocationChanged?.Invoke(location);
				await _hudPresenter.FadeOut(0.5f);
				if (location == ELocation.Death)
				{
					_dialogManager.ShowSubtitle("Death_Appearance", null, EDialogOverlayType.None, autoskip: true);
				}
				_saveData.CurrentLocation = location;
				IsGoingThroughLocations = false;
			}
		}

		private async UniTask SetMusic(Location newLocation, ETimeOfDay timeOfDay)
		{
			if (_currentLocation.OverrideVolumeProfile)
			{
				_effectsController.ChangeVolumeProfile(_currentLocation.VolumeProfile);
			}
			if (_currentLocation.OverrideSkybox)
			{
				_effectsController.ChangeSkybox(_currentLocation.Skybox);
			}
			if (_currentLocation.OverrideFog)
			{
				_effectsController.EnableFog(_currentLocation.Fog.StartDistance, _currentLocation.Fog.EndDistance, _currentLocation.Fog.Color);
			}
			await UniTask.DelayFrame(1);
			if (newLocation.AmbientLoop != ESound.None)
			{
				_soundService.FadeIn(ESoundSource.s1_GameScene_2DMusicSourceAmbient, newLocation.AmbientLoop, 0.5f, Ease.InQuad, new SoundSettingCycling()).Forget();
			}
			switch (newLocation.MusicType)
			{
			case ELocationAudioType.CustomMusic:
				if (timeOfDay == ETimeOfDay.Day)
				{
					_soundService.FadeIn(ESoundSource.s1_GameScene_2DMusicSourceDay, newLocation.CustomMusic, 0.5f, Ease.InQuad, new SoundSettingCycling()).Forget();
				}
				else
				{
					_soundService.FadeIn(ESoundSource.s1_GameScene_2DMusicSourceNight, newLocation.CustomMusic, 0.5f, Ease.InQuad, new SoundSettingCycling()).Forget();
				}
				break;
			case ELocationAudioType.UsualDayNightMusic:
				if (timeOfDay == ETimeOfDay.Day)
				{
					_soundService.EnableDayTheme(_dayNightController.Day, 0.5f);
				}
				else
				{
					_soundService.EnableNightTheme(_dayNightController.Day, 0.5f);
				}
				break;
			case ELocationAudioType.NoMusic:
				break;
			}
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
			LocationSaveData locationSaveData = saver.LoadKey(this);
			_saveData = locationSaveData ?? new LocationSaveData();
			Location location = _locationsList.FirstOrDefault((Location x) => x.LocationType == _saveData.CurrentLocation);
			SetMusicOnLoad(location, _dayNightController.CurrentTimeOfDay).Forget();
			_currentLocation.gameObject.SetActive(value: false);
			location.gameObject.SetActive(value: true);
			_currentLocation = location;
			_playerService.MakeMovable();
			LocationChanged?.Invoke(_saveData.CurrentLocation);
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}

		private async UniTask SetMusicOnLoad(Location newLocation, ETimeOfDay currentTimeOfDay)
		{
			if (_dayNightController.CurrentTimeOfDay == ETimeOfDay.Day)
			{
				_soundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceDay, 0f).Forget();
			}
			else
			{
				_soundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceNight, 0f).Forget();
			}
			_soundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceAmbient, 0f).Forget();
			await UniTask.DelayFrame(1);
			SetMusic(newLocation, _dayNightController.CurrentTimeOfDay).Forget();
		}
	}
}
