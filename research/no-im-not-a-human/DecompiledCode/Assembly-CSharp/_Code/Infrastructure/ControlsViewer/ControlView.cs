using System.Linq;
using Cysharp.Threading.Tasks;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Infrastructure.ControlsViewer;

public sealed class ControlView : MonoBehaviour
{
	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Image _keyImage;

	[SerializeField]
	private TMP_Text _keyText;

	[SerializeField]
	private RTLTextMeshPro _descriptionText;

	[SerializeField]
	private ControlsSpritesListSOData _controlsSpritesListSOData;

	[SerializeField]
	private ControlSpriteGaypadListSOData _controlsSpritesGaypadListSOData;

	private bool _isAvailable = true;

	private bool _isEnabled;

	private bool _isChangingNow;

	public EControl Control { get; private set; }

	public void InitKey(string key, EControl control)
	{
		Control = control;
		EControlKeyType controlType = key switch
		{
			"Space" => EControlKeyType.Space, 
			"Shift" => EControlKeyType.Shift, 
			"Left Button" => EControlKeyType.MouseClickedLeft, 
			_ => EControlKeyType.UsualKey, 
		};
		if (key == "Q" && LocalizationSettings.SelectedLocale.Identifier.Code == "fr")
		{
			key = "A";
		}
		_keyText.text = key;
		_keyText.gameObject.SetActive(key != "Left Button");
		_keyImage.sprite = _controlsSpritesListSOData.ControlsData.FirstOrDefault((ControlSpriteSOData x) => x.KeyType == controlType)?.Sprite;
		_keyImage.SetNativeSize();
	}

	public void InitGamepadKey(string key, EControl control, EGaypadType gaypadType)
	{
		Control = control;
		EControlKeyType controlType = key switch
		{
			"Button East" => EControlKeyType.GamepadEast, 
			"Button South" => EControlKeyType.GamepadSouth, 
			"Button West" => EControlKeyType.GamepadWest, 
			"Button North" => EControlKeyType.GamepadNorth, 
			"Right Stick" => EControlKeyType.GamepadRightStick, 
			"Left Stick" => EControlKeyType.GamepadLeftStick, 
			"Right Shoulder" => EControlKeyType.GamepadRightBumper, 
			"Left Shoulder" => EControlKeyType.GamepadLeftBumper, 
			"Left Trigger" => EControlKeyType.GamepadLeftTrigger, 
			"Right Trigger" => EControlKeyType.GamepadRightTrigger, 
			_ => EControlKeyType.UsualKey, 
		};
		_keyText.gameObject.SetActive(value: false);
		_keyImage.sprite = _controlsSpritesGaypadListSOData.ControlsData.FirstOrDefault((ControlSpriteGaypadSOData x) => x.KeyType == controlType)?.Sprites[gaypadType];
		_keyImage.SetNativeSize();
	}

	public void SetDescription(string description)
	{
		_descriptionText.text = description;
		_isAvailable = true;
		_canvasGroup.alpha = 1f;
	}

	public void Enable()
	{
		gameObject.SetActive(value: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
		_isEnabled = true;
		UpdateVisual();
	}

	public async UniTask Disable()
	{
		_isEnabled = false;
		UpdateVisual();
		gameObject.SetActive(value: false);
	}

	public void SetAvailability(bool isAvailable)
	{
		if (_isAvailable != isAvailable)
		{
			_isAvailable = isAvailable;
			UpdateVisual();
		}
	}

	private void UpdateVisual()
	{
		float alpha;
		if (_isEnabled)
		{
			alpha = (_isAvailable ? 1f : 0.2f);
		}
		else
		{
			alpha = 0f;
		}
		_canvasGroup.alpha = alpha;
	}
}
