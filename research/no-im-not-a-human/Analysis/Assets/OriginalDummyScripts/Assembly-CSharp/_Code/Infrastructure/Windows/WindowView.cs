using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.DialogSystem;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Rooms;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Windows;

public sealed class WindowView : MonoBehaviour, IWindowView
{
	[SerializeField]
	private int _windowIndex;

	[SerializeField]
	private AnimatedImage[] _images;

	[SerializeField]
	private ESoundSource _audioSource;

	[SerializeField]
	private MeshRenderer[] _beams;

	[SerializeField]
	private Material _windowMaterial;

	private IDayNightController _dayNightController;

	private IDialogManager _dialogManager;

	private INotAHumanSoundService _soundService;

	private IDataModelService _dataModelService;

	private WindowDayImageData[] _imagesByDays;

	private WindowsSOData _windowSOData;

	private AActionableObjectView _linkedActionableObject;

	private bool _hasWatchedMonologueToday;

	private int _imagesTonight;

	private float _lightIntensity;

	[field: SerializeField]
	public Camera LinkedCamera { get; private set; }

	[field: SerializeField]
	public float CameraDistance { get; private set; }

	[field: SerializeField]
	public float DialogCameraDistance { get; private set; }

	private bool HasDialogNow => _dialogManager.IsOpenedSubtitle;

	public bool CanLeave => !HasDialogNow;

	public Vector3 LookDirection => new Vector3(0f, 1f, 0f) * _linkedActionableObject.AwaitedPlayerLookAngle;

	public event Action<WindowView> Opened;

	public event Action<WindowView> Closed;

	public event Action<WindowView> StartedOpening;

	public void Init(IDayNightController dayNightController, IDialogManager dialogManager, IWindowsSODataProvider dataProvider, INotAHumanSoundService soundService, IDataModelService dataModelService)
	{
		_dayNightController = dayNightController;
		_dialogManager = dialogManager;
		_windowSOData = dataProvider.GetWindowData;
		_soundService = soundService;
		_dataModelService = dataModelService;
		int windowIndex = _windowIndex;
		_imagesByDays = windowIndex switch
		{
			1 => dataProvider.GetWindowData.CurtainsData, 
			2 => dataProvider.GetWindowData.BlindsPantryData, 
			3 => dataProvider.GetWindowData.BlindsBedroomData, 
		};
		InitImages(dayNightController.Day);
	}

	public void InitImages(int day)
	{
		AnimatedImage[] images = _images;
		for (int i = 0; i < images.Length; i++)
		{
			images[i].gameObject.SetActive(value: false);
		}
		_imagesTonight = _imagesByDays[day].WindowImageData.Length;
		for (int j = 0; j < _imagesTonight; j++)
		{
			_images[j].SetData(_imagesByDays[day].WindowImageData[j].Animation);
			_images[j].transform.localPosition = _imagesByDays[day].WindowImageData[j].PositionShift;
			_images[j].transform.localScale = _imagesByDays[day].WindowImageData[j].ScaleShift;
		}
	}

	public void StartOpen()
	{
		_lightIntensity = _beams[0].material.GetFloat("_LightIntensity");
		MeshRenderer[] beams = _beams;
		for (int i = 0; i < beams.Length; i++)
		{
			beams[i].material.DOFloat(0f, "_LightIntensity", 0.2f);
		}
		_windowMaterial.DOColor(new Color(0f, 0f, 0f), "_SpecColor", 0.5f);
		_soundService.FadeVolume(_audioSource, 0f, 1f, Ease.InSine);
		for (int j = 0; j < _imagesTonight; j++)
		{
			_images[j].gameObject.SetActive(value: true);
		}
		Opened?.Invoke(this);
	}

	public void FinishOpen()
	{
		if (!_hasWatchedMonologueToday)
		{
			_hasWatchedMonologueToday = true;
			ShowDialog();
		}
	}

	public void Close()
	{
		MeshRenderer[] beams = _beams;
		for (int i = 0; i < beams.Length; i++)
		{
			beams[i].material.DOFloat(_lightIntensity, "_LightIntensity", 1f);
		}
		_windowMaterial.DOColor(new Color(0.5f, 0.5f, 0.5f), "_SpecColor", 0.2f);
		_soundService.FadeVolume(_audioSource, 1f, 1f, Ease.OutSine);
		gameObject.SetActive(value: false);
		_dialogManager.HideSubtitle();
		Closed?.Invoke(this);
	}

	private void ShowDialog()
	{
		_dataModelService.MetaPrefsDataHandler.LookAtWindow(_windowIndex, _dayNightController.Day);
		string text = "window_" + _dayNightController.Day + "_" + _windowIndex;
		_dialogManager.ShowSubtitle(text, LinkedCamera);
		if (text == "window_12_2")
		{
			BodyEaterAchievement().Forget();
		}
	}

	private async UniTask BodyEaterAchievement()
	{
		await UniTask.Delay(TimeSpan.FromSeconds(1.0));
		SteamAchievements.Unlock("WTF");
	}

	public void RenewDialogs()
	{
		_hasWatchedMonologueToday = false;
	}

	public void StartOfStartOpening()
	{
		StartedOpening?.Invoke(this);
	}

	public void LinkActionableObject(AActionableObjectView actionableObject)
	{
		_linkedActionableObject = actionableObject;
	}

	public void ShowView()
	{
		gameObject.SetActive(value: true);
	}

	public void EnableSound(ESound sound)
	{
		_soundService.FadeIn(_audioSource, sound, 5f, Ease.InSine, new SoundSettingCycling());
	}

	public void DisableSound()
	{
		_soundService.FadeOut(_audioSource, 5f, Ease.OutSine);
	}
}
