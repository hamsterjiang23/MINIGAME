using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Code.Player;
using _Code.Utils.Extensions;
using _Code.Utils.UI;

namespace _Code.Infrastructure.Settings.Screen;

public sealed class ScreenSettingsInstance : ASettingsInstance
{
	[SerializeField]
	private ScrollableDropdown _resolutionDropdown;

	[SerializeField]
	private ScrollableDropdown _fullScreenDropdown;

	[SerializeField]
	private Toggle _vSyncToggle;

	private readonly ScreenSettings _screenSettings = new ScreenSettings();

	private bool _isSceneStarted;

	private InputHandling _inputHandling;

	public override ISetting Setting => _screenSettings;

	public void InitModules(InputHandling inputHandling)
	{
		_inputHandling = inputHandling;
	}

	protected override void Init()
	{
		_resolutionDropdown.InitModules(_inputHandling);
		_fullScreenDropdown.InitModules(_inputHandling);
		_resolutionDropdown.Dropdown.ClearOptions();
		for (int num = UnityEngine.Screen.resolutions.Length - 1; num >= 0; num--)
		{
			Resolution resolution = UnityEngine.Screen.resolutions[num];
			_resolutionDropdown.Dropdown.options.Add(new TMP_Dropdown.OptionData(resolution.width + " x " + resolution.height + " (" + Math.Round(resolution.refreshRateRatio.value, 2) + " Hz)"));
		}
		_fullScreenDropdown.Dropdown.ClearOptions();
		string[] names = Enum.GetNames(typeof(FullScreenMode));
		foreach (string text in names)
		{
			_fullScreenDropdown.Dropdown.options.Add(new TMP_Dropdown.OptionData(("fullscreen_settings_" + text).GetLocalization("UI")));
		}
		_resolutionDropdown.Dropdown.onValueChanged.RemoveAllListeners();
		_fullScreenDropdown.Dropdown.onValueChanged.RemoveAllListeners();
		_vSyncToggle.onValueChanged.RemoveAllListeners();
		_resolutionDropdown.Dropdown.onValueChanged.AddListener(OnResolutionChanged);
		_fullScreenDropdown.Dropdown.onValueChanged.AddListener(OnFullScreenChanged);
		_vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
	}

	public void OnLanguageChanged()
	{
		for (int i = 0; i < _fullScreenDropdown.Dropdown.options.Count; i++)
		{
			TMP_Dropdown.OptionData optionData = _fullScreenDropdown.Dropdown.options[i];
			FullScreenMode fullScreenMode = (FullScreenMode)i;
			optionData.text = ("fullscreen_settings_" + fullScreenMode).GetLocalization("UI");
		}
		_fullScreenDropdown.Dropdown.RefreshShownValue();
	}

	protected override void UpdateVisualsForLoadedData()
	{
		UpdateVisualsForLoadedDataAsync().Forget();
	}

	private async UniTaskVoid UpdateVisualsForLoadedDataAsync()
	{
		await UniTask.WaitUntil(() => _isSceneStarted);
		await UniTask.DelayFrame(1);
		ScreenSettingsData screenSettingsData = (ScreenSettingsData)Setting.SettingsData;
		if (!screenSettingsData.IsNotFirstLaunch)
		{
			screenSettingsData.IsNotFirstLaunch = true;
			_resolutionDropdown.Dropdown.value = FindOptimalResolution();
			_fullScreenDropdown.Dropdown.value = 1;
			_vSyncToggle.isOn = true;
		}
		else
		{
			_resolutionDropdown.Dropdown.value = screenSettingsData.SelectedResolutionIndex;
		}
		_fullScreenDropdown.Dropdown.value = (int)screenSettingsData.FullScreenMode;
		_vSyncToggle.isOn = screenSettingsData.IsVSync;
		TrySetSteamdeckResolution().Forget();
	}

	private async UniTask TrySetSteamdeckResolution()
	{
		await UniTask.WaitUntil(() => SteamManager.Initialized);
		if (SteamUtils.IsSteamRunningOnSteamDeck() && !((ScreenSettingsData)Setting.SettingsData).HasUpdatedToExclusiveFullscreenForSteamdeckSinceVersion120)
		{
			((ScreenSettingsData)Setting.SettingsData).HasUpdatedToExclusiveFullscreenForSteamdeckSinceVersion120 = true;
			_fullScreenDropdown.Dropdown.value = 0;
		}
	}

	private int FindOptimalResolution()
	{
		List<Resolution> list = UnityEngine.Screen.resolutions.Reverse().ToList();
		float num = (float)UnityEngine.Screen.currentResolution.width / (float)UnityEngine.Screen.currentResolution.height;
		int targetWidth = (int)(1080f * num);
		Resolution item = list.FirstOrDefault((Resolution x) => x.width == targetWidth && x.height == 1080);
		return list.IndexOf(item);
	}

	private void Start()
	{
		_isSceneStarted = true;
	}

	private void OnVSyncChanged(bool isOn)
	{
		_screenSettings.SetVSync(isOn);
	}

	private void OnResolutionChanged(int selectedIndex)
	{
		_screenSettings.SetResolution(selectedIndex);
	}

	private void OnFullScreenChanged(int selectedIndex)
	{
		_screenSettings.SetFullScreenMode((FullScreenMode)selectedIndex);
	}
}
