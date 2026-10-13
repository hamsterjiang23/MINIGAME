using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Settings.Control;
using _Code.Infrastructure.Settings.Language;
using _Code.Infrastructure.Settings.Screen;
using _Code.Infrastructure.Settings.Sound;
using _Code.Player;
using _Code.Utils.UI;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Settings
{
	public sealed class SettingsInstance : MonoBehaviour
	{
		[SerializeField]
		private EventSystem _eventSystem;

		[SerializeField]
		private ScrollRect _scrollRect;

		[SerializeField]
		private GameObject _firstSelectedObject;

		[SerializeField]
		private ScreenSettingsInstance _screenSettings;

		[SerializeField]
		private SoundSettingsInstance _soundSettings;

		[SerializeField]
		private ATextSettingsInstance _textSettings;

		[SerializeField]
		private ControlSettingsInstance _controlSettings;

		private List<ASettingsInstance> _settingsInstances;

		private IDataModelService _dataModelService;

		private InputHandling _inputHandler;

		private ICursorController _cursorController;

		private UISelectable[] _selectables;

		public ScreenSettingsData ScreenSettingsData => (ScreenSettingsData)_screenSettings.Setting.SettingsData;

		public SoundSettingsData SoundSettingsData => (SoundSettingsData)_soundSettings.Setting.SettingsData;

		public TextSettingsData TextSettingsData => (TextSettingsData)_textSettings.Setting.SettingsData;

		public ControlSettingsData ControlSettingsData => (ControlSettingsData)_controlSettings.Setting.SettingsData;

		public event Action Changed;

		public void Init(IDataModelService dataModelService, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController)
		{
			_cursorController = cursorController;
			_inputHandler = inputHandlerProvider.InputHandler;
			_dataModelService = dataModelService;
			_settingsInstances = new List<ASettingsInstance> { _textSettings, _screenSettings, _soundSettings, _controlSettings };
			_textSettings.InitModules(_inputHandler);
			_screenSettings.InitModules(_inputHandler);
			_controlSettings.InitModules(_inputHandler);
			LoadSettings();
			foreach (ASettingsInstance settingsInstance in _settingsInstances)
			{
				settingsInstance.Initialize();
			}
			_textSettings.LanguageChanged -= OnLanguageChanged;
			_textSettings.LanguageChanged += OnLanguageChanged;
		}

		private void OnLanguageChanged()
		{
			_screenSettings.OnLanguageChanged();
		}

		public void SaveSettings()
		{
			_dataModelService.SettingsPrefsDataHandler.ScreenSettings = (ScreenSettingsData)_screenSettings.Setting.SettingsData;
			_dataModelService.SettingsPrefsDataHandler.TextSettings = (TextSettingsData)_textSettings.Setting.SettingsData;
			_dataModelService.SettingsPrefsDataHandler.SoundSettings = (SoundSettingsData)_soundSettings.Setting.SettingsData;
			_dataModelService.SettingsPrefsDataHandler.ControlSettings = (ControlSettingsData)_controlSettings.Setting.SettingsData;
			Changed?.Invoke();
		}

		private void LoadSettings()
		{
			_screenSettings.Setting.SettingsData = _dataModelService.SettingsPrefsDataHandler.ScreenSettings ?? new ScreenSettingsData();
			_textSettings.Setting.SettingsData = _dataModelService.SettingsPrefsDataHandler.TextSettings ?? new TextSettingsData();
			_soundSettings.Setting.SettingsData = _dataModelService.SettingsPrefsDataHandler.SoundSettings ?? new SoundSettingsData();
			_controlSettings.Setting.SettingsData = _dataModelService.SettingsPrefsDataHandler.ControlSettings ?? new ControlSettingsData();
			Changed?.Invoke();
		}

		public void Initialize()
		{
			_soundSettings.OnStarted();
			_controlSettings.OnStarted();
		}

		private void Start()
		{
			_selectables = _scrollRect.GetComponentsInChildren<UISelectable>();
			UISelectable[] selectables = _selectables;
			foreach (UISelectable obj in selectables)
			{
				obj.Selected -= OnItemSelected;
				obj.Selected += OnItemSelected;
			}
		}

		private void OnItemSelected(BaseEventData eventData)
		{
			if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				return;
			}
			RectTransform rectTransform = eventData.selectedObject.transform as RectTransform;
			if (!(_scrollRect == null) && !(_scrollRect.content == null) && !(_scrollRect.viewport == null))
			{
				RectTransform content = _scrollRect.content;
				RectTransform viewport = _scrollRect.viewport;
				Vector3 position = rectTransform.TransformPoint(rectTransform.rect.center);
				float y = _scrollRect.transform.InverseTransformPoint(position).y;
				float y2 = _scrollRect.transform.InverseTransformPoint(viewport.position).y;
				float num = y - y2;
				float num2 = content.rect.height - viewport.rect.height;
				if (!Mathf.Approximately(num2, 0f))
				{
					float num3 = num / num2;
					_scrollRect.verticalNormalizedPosition = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + num3);
				}
			}
		}

		public void Show()
		{
			gameObject.SetActive(value: true);
			EventSystem.current.SetSelectedGameObject((_inputHandler.CurrentDevice == EInputDevice.Gaypad) ? _firstSelectedObject : null);
		}
	}
}
