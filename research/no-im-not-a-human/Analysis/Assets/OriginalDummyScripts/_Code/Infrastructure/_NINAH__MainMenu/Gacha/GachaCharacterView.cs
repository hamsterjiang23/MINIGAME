using System;
using System.Runtime.CompilerServices;
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

		[CompilerGenerated]
		private Action<CharacterSOData> m_CharacterSelected;

		public event Action<CharacterSOData> CharacterSelected
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

		public void Init(CharacterSOData character, bool isUnlocked, InputHandling inputHandling)
		{
		}

		private void OnSelected(BaseEventData eventData)
		{
		}
	}
}
