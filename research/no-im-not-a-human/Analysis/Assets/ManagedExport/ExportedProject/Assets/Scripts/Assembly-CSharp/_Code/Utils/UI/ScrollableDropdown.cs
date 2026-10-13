using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Utils.UI
{
	public sealed class ScrollableDropdown : MonoBehaviour, ISubmitHandler, IEventSystemHandler, IPointerClickHandler
	{
		[SerializeField]
		private bool _isLocalizationDD;

		[SerializeField]
		private FontChangerSOData _fontChangerSOData;

		private ScrollRect _scrollRect;

		private UISelectable[] _selectables;

		private InputHandling _inputHandler;

		[field: SerializeField]
		public TMP_Dropdown Dropdown { get; private set; }

		public void InitModules(InputHandling inputHandling)
		{
			_inputHandler = inputHandling;
			_inputHandler.InputDeviceChanged -= OnDeviceChanged;
			_inputHandler.InputDeviceChanged += OnDeviceChanged;
		}

		private void OnEnable()
		{
			Dropdown.onValueChanged.RemoveListener(OnValueChanged);
			Dropdown.onValueChanged.AddListener(OnValueChanged);
			LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
			LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
		}

		private void OnDeviceChanged(EInputDevice device)
		{
			Dropdown.Hide();
		}

		private void Refresh()
		{
			_selectables = GetComponentsInChildren<UISelectable>();
			UISelectable[] selectables = _selectables;
			foreach (UISelectable obj in selectables)
			{
				obj.Selected -= OnItemSelected;
				obj.Selected += OnItemSelected;
			}
			_scrollRect = GameObject.Find("Dropdown List").GetComponentInChildren<ScrollRect>();
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

		public void OnSubmit(BaseEventData eventData)
		{
			if (_isLocalizationDD)
			{
				RefreshFonts();
			}
			RefreshAsync().Forget();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_isLocalizationDD)
			{
				RefreshFonts();
			}
		}

		private void RefreshFonts()
		{
			Transform transform = base.transform.Find("Dropdown List").Find("Viewport/Content");
			List<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
			for (int i = 1; i < transform.childCount; i++)
			{
				TMP_Text componentInChildren = transform.GetChild(i).GetComponentInChildren<TMP_Text>();
				string code = locales[i - 1].Identifier.Code;
				componentInChildren.font = _fontChangerSOData.Fonts[code];
			}
		}

		private async UniTaskVoid RefreshAsync()
		{
			await UniTask.DelayFrame(1);
			Refresh();
		}

		private void OnLocaleChanged(Locale obj)
		{
			OnValueChanged(Dropdown.value);
		}

		private void OnValueChanged(int value)
		{
			if (Dropdown.captionText is RTLTextMeshPro rTLTextMeshPro)
			{
				rTLTextMeshPro.text = Dropdown.options[value].text;
			}
		}
	}
}
