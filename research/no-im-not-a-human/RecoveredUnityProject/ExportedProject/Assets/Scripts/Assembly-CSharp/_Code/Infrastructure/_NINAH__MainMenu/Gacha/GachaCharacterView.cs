using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Characters;
using _Code.Player;
using _Code.Utils.UI;

namespace _Code.Infrastructure._NINAH__MainMenu.Gacha
{
	public sealed class GachaCharacterView : MonoBehaviour
	{
		[SerializeField]
		private Image _bg;

		[SerializeField]
		private Sprite _unlockedBg;

		[SerializeField]
		private Sprite _lockedBg;

		[SerializeField]
		private Button _button;

		[SerializeField]
		private Image _characterSprite;

		[SerializeField]
		private Material _lockedCharacterMaterial;

		[SerializeField]
		private Material _unlockedCharacterMaterial;

		[SerializeField]
		private UISelectable _selectable;

		private bool _isUnlocked;

		private CharacterSOData _characterData;

		private InputHandling _inputHandling;

		public event Action<CharacterSOData> CharacterSelected;

		public void Init(CharacterSOData character, bool isUnlocked, InputHandling inputHandling)
		{
			_isUnlocked = isUnlocked;
			_button.interactable = isUnlocked;
			_bg.sprite = (isUnlocked ? _unlockedBg : _lockedBg);
			_characterSprite.sprite = character.GetEmotion(EDialogEmotionState.Base).Frames[0];
			_characterSprite.transform.localPosition = new Vector3(character.GachaPosBias.x - 100f, character.GachaPosBias.y - 200f, 0f);
			_characterSprite.transform.localScale = new Vector3(character.GachaScaleBias.x + 1f, character.GachaScaleBias.y + 1f, 1f);
			_characterData = character;
			if (isUnlocked)
			{
				_characterSprite.material = UnityEngine.Object.Instantiate(_unlockedCharacterMaterial);
			}
			else
			{
				_characterSprite.material = UnityEngine.Object.Instantiate(_lockedCharacterMaterial);
			}
			_inputHandling = inputHandling;
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(() =>
			{
				CharacterSelected?.Invoke(character);
			});
			_selectable.Selected -= OnSelected;
			_selectable.Selected += OnSelected;
		}

		private void OnSelected(BaseEventData eventData)
		{
			if (_inputHandling.CurrentDevice == EInputDevice.Gaypad)
			{
				CharacterSelected?.Invoke(_characterData);
			}
		}
	}
}
