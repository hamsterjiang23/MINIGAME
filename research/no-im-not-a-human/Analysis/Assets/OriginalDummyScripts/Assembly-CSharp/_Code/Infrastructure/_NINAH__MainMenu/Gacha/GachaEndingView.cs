using System;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.Endings.View;
using _Code.Utils.Extensions;

namespace _Code.Infrastructure._NINAH__MainMenu.Gacha;

public sealed class GachaEndingView : MonoBehaviour
{
	[SerializeField]
	private Image _backgroundImage;

	[SerializeField]
	private Sprite[] _backgroundSprites;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private RTLTextMeshPro _description;

	[SerializeField]
	private Image[] _spoilersImages;

	[SerializeField]
	private Image _watchImage;

	[SerializeField]
	private Sprite _watchImageUnlocked;

	[SerializeField]
	private Sprite _watchImageLocked;

	[SerializeField]
	private Material _imageFogMaterial;

	[SerializeField]
	private Material _imageFogUnlockedMaterial;

	private bool _isUnlocked;

	private EndingViewSOData _data;

	private Material _fogMaterialInstance;

	public event Action<EndingViewSOData> WatchMovieClicked;

	private void OnEnable()
	{
		_button.onClick.AddListener(WatchMovie);
	}

	private void OnDisable()
	{
		_button.onClick.RemoveAllListeners();
	}

	public void Init(EndingViewSOData data, bool isUnlocked, int index)
	{
		_isUnlocked = isUnlocked;
		_data = data;
		_button.interactable = isUnlocked;
		_backgroundImage.sprite = _backgroundSprites[index % 3];
		_watchImage.sprite = (isUnlocked ? _watchImageUnlocked : _watchImageLocked);
		for (int i = 0; i < _spoilersImages.Length; i++)
		{
			_spoilersImages[i].sprite = data.GachaData.SpoilerImages[i];
			if (!isUnlocked)
			{
				Material material = UnityEngine.Object.Instantiate(_imageFogMaterial);
				_imageFogMaterial.SetFloat("_Seed", UnityEngine.Random.Range(0, 100));
				_spoilersImages[i].material = material;
			}
			else
			{
				Material material2 = UnityEngine.Object.Instantiate(_imageFogUnlockedMaterial);
				_imageFogMaterial.SetFloat("_Seed", UnityEngine.Random.Range(0, 100));
				_spoilersImages[i].material = material2;
			}
		}
		_description.text = (isUnlocked ? data.GachaData.Description : ("<color=#ffffff88>" + "gacha_locked_ending".GetLocalization("UI") + "</color>"));
	}

	private void WatchMovie()
	{
		if (_isUnlocked)
		{
			WatchMovieClicked?.Invoke(_data);
		}
	}
}
