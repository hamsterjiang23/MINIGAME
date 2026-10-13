using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

		[CompilerGenerated]
		private Action m_Changed;

		public ScreenSettingsData ScreenSettingsData => null;

		public SoundSettingsData SoundSettingsData => null;

		public TextSettingsData TextSettingsData => null;

		public ControlSettingsData ControlSettingsData => null;

		public event Action Changed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public void Init(IDataModelService dataModelService, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController)
		{
		}

		private void OnLanguageChanged()
		{
		}

		public void SaveSettings()
		{
		}

		private void LoadSettings()
		{
		}

		public void Initialize()
		{
		}

		private void Start()
		{
		}

		private void OnItemSelected(BaseEventData eventData)
		{
		}

		public void Show()
		{
		}
	}
}
