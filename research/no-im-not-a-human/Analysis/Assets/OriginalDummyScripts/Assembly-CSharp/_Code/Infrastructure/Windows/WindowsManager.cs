using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.DialogSystem;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Utils.Extensions;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Windows;

public sealed class WindowsManager : IWindowsManager
{
	private readonly IWindowView[] _windows;

	private readonly IDayNightController _dayNightController;

	private readonly IDialogManager _dialogManager;

	private readonly IWindowsSODataProvider _windowsDataProvider;

	private readonly RoomDisplayer _roomDisplayer;

	private readonly IPlayerService _playerService;

	private readonly INotAHumanSoundService _soundService;

	private readonly IDataModelService _dataModelService;

	public bool IsInWindow { get; private set; }

	public WindowsManager(IWindowsViewProvider windowsViewProvider, IDayNightController dayNightController, IDialogManager dialogManager, IRoomDisplayerViewProvider roomDisplayerViewProvider, IWindowsSODataProvider windowsDataProvider, IPlayerService playerService, INotAHumanSoundService soundService, IDataModelService dataModelService)
	{
		IWindowView[] windowsViews = windowsViewProvider.WindowsViews;
		_windows = windowsViews;
		_dayNightController = dayNightController;
		_dialogManager = dialogManager;
		_windowsDataProvider = windowsDataProvider;
		_playerService = playerService;
		_roomDisplayer = roomDisplayerViewProvider.RoomDisplayer;
		_soundService = soundService;
		_dataModelService = dataModelService;
		Init();
	}

	private void Init()
	{
		IWindowView[] windows = _windows;
		foreach (IWindowView obj in windows)
		{
			obj.Init(_dayNightController, _dialogManager, _windowsDataProvider, _soundService, _dataModelService);
			obj.Opened -= OnOpened;
			obj.Opened += OnOpened;
			obj.Closed -= OnClosed;
			obj.Closed += OnClosed;
			obj.StartedOpening -= OnStartOpen;
			obj.StartedOpening += OnStartOpen;
		}
		_dialogManager.StartedWindowNoise -= StartNoise;
		_dialogManager.StartedWindowNoise += StartNoise;
		_dayNightController.DayChanged -= OnDayChanged;
		_dayNightController.DayChanged += OnDayChanged;
	}

	private void OnStartOpen(WindowView obj)
	{
		Debug.Log("OnStartOpen");
		IsInWindow = true;
	}

	private void OnDayChanged(int day)
	{
		IWindowView[] windows = _windows;
		for (int i = 0; i < windows.Length; i++)
		{
			windows[i].DisableSound();
		}
	}

	private void StartNoise(ESound sound)
	{
		IWindowView[] windows = _windows;
		for (int i = 0; i < windows.Length; i++)
		{
			windows[i].EnableSound(sound);
		}
	}

	private void OnOpened(WindowView window)
	{
		Debug.Log("OnOpened");
		_roomDisplayer.SetData(window.LinkedCamera, window.CameraDistance, window.DialogCameraDistance);
		OnOpenedAsync(window).Forget();
		IsInWindow = true;
	}

	private async UniTask OnOpenedAsync(WindowView window)
	{
		UniTask taskToAwaitResult;
		if (Mathf.Abs(window.LookDirection.NormalizeAngle(40f).y - _playerService.LookDirection.NormalizeAngle(40f).y) < 40f)
		{
			taskToAwaitResult = _roomDisplayer.OpenFadeLocal();
			window.ShowView();
		}
		else
		{
			taskToAwaitResult = _roomDisplayer.OpenFadeGlobal();
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
			window.ShowView();
		}
		await taskToAwaitResult;
	}

	private void OnClosed(WindowView window)
	{
		_roomDisplayer.CloseFade().Forget();
		Debug.Log("OnClosed");
		IsInWindow = false;
	}
}
