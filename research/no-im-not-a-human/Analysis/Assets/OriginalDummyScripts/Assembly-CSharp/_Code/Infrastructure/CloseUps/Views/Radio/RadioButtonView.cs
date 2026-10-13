using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Code.Infrastructure.CloseUps.Views.Radio;

public sealed class RadioButtonView : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private Sprite _usualSprite;

	[SerializeField]
	private Sprite _pressedSprite;

	[SerializeField]
	private ERadioState _radioState;

	[SerializeField]
	private Material _outlineMaterialSource;

	[SerializeField]
	private Material _hintMaterialSource;

	private bool _isPressed;

	private Material _outlineMaterial;

	private Material _hintMaterial;

	private bool _isOutlineEnabled = true;

	public event Action<ERadioState> Pressed;

	private void Awake()
	{
		_outlineMaterial = UnityEngine.Object.Instantiate(_outlineMaterialSource);
		_hintMaterial = UnityEngine.Object.Instantiate(_hintMaterialSource);
		_image.material = _hintMaterial;
		_outlineMaterial.SetFloat("_OutlineThickness", 0.002f);
	}

	public void SetState(bool isPressed)
	{
		_isPressed = isPressed;
		UpdateSprite();
	}

	private void UpdateSprite()
	{
		_image.sprite = (_isPressed ? _pressedSprite : _usualSprite);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Pressed?.Invoke(_radioState);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_isOutlineEnabled)
		{
			_image.material = _outlineMaterial;
			_outlineMaterial.SetFloat("_IsActive", 1f);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_image.material = _hintMaterial;
		_outlineMaterial.SetFloat("_IsActive", 0f);
	}

	public void SetOutlineEnabled(bool isEnabled)
	{
		_isOutlineEnabled = isEnabled;
		_outlineMaterial.SetFloat("_IsActive", 0f);
	}
}
