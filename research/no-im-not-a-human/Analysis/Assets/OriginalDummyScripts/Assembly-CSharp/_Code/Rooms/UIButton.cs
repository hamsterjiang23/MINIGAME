using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Rooms;

public sealed class UIButton : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	private Action _action;

	private const float AlphaThreshold = 0.1f;

	private bool _isHovered;

	private Material _outlineMaterial;

	public static bool AreButtonsEnabled { get; set; }

	public int IsActiveCount { get; private set; }

	public bool IsActive
	{
		get
		{
			return IsActiveCount >= 1;
		}
		set
		{
			if (value)
			{
				IsActiveCount++;
			}
			else
			{
				IsActiveCount--;
			}
		}
	}

	private void Awake()
	{
		if (_image != null)
		{
			_outlineMaterial = UnityEngine.Object.Instantiate(_image.material);
			_image.material = _outlineMaterial;
		}
	}

	public void Init(int activeCount)
	{
		IsActiveCount = activeCount;
	}

	public void SetAction(Action action)
	{
		_action = action;
	}

	public void Click()
	{
		_action?.Invoke();
		OnUnhover();
	}

	public bool IsMousePosInSpriteArea(Vector3 position, Camera cam)
	{
		if (!IsActive)
		{
			return false;
		}
		Texture2D texture = _image.sprite.texture;
		Rect rect = _image.sprite.rect;
		if (texture == null)
		{
			return false;
		}
		RectTransform rectTransform = _image.rectTransform;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, cam, out var localPoint))
		{
			return false;
		}
		Rect rect2 = rectTransform.rect;
		float num = localPoint.x / rect2.width + 0.5f;
		float num2 = localPoint.y / rect2.height + 0.5f;
		if (num < 0f || num > 1f || num2 < 0f || num2 > 1f)
		{
			return false;
		}
		int x = Mathf.FloorToInt(rect.x + rect.width * num);
		int y = Mathf.FloorToInt(rect.y + rect.height * num2);
		return texture.GetPixel(x, y).a > 0.1f;
	}

	public void OnHover()
	{
		if (_outlineMaterial != null)
		{
			_outlineMaterial.SetFloat("_IsActive", 1f);
		}
	}

	public void OnUnhover()
	{
		if (_outlineMaterial != null)
		{
			_outlineMaterial.SetFloat("_IsActive", 0f);
		}
	}
}
