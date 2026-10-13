using System;
using System.Linq;
using UnityEngine;
using _Code.Characters;

namespace _Code.Rooms
{
	public sealed class CharacterRoomObjectView : ARoomObjectView<ERoomPeopleState>
	{
		[SerializeField]
		private ERoomPeopleState _startState;

		[SerializeField]
		private Material _corpseShader;

		private bool _isGigaDisabled;

		[field: SerializeField]
		public CharacterSOData Data { get; private set; }

		protected override RoomObjectState<ERoomPeopleState>[] States => Data.Poses;

		protected override ERoomPeopleState StartState => _startState;

		public bool IsButtonActive => _button.IsActive;

		public event Action<CharacterRoomObjectView, bool> DialogStarted;

		protected override void Awake()
		{
			if (SelectedState != ERoomPeopleState.Bags && SelectedState != ERoomPeopleState.Corpse)
			{
				base.Awake();
			}
			SetAction(TryStartDialog);
		}

		private void TryStartDialog()
		{
			if (!DialogManager.IsOpened)
			{
				RoomObjectState<ERoomPeopleState> roomObjectState = Data.Poses.FirstOrDefault((RoomObjectState<ERoomPeopleState> x) => x.Name == SelectedState);
				if (roomObjectState == null || !roomObjectState.CannotInteract)
				{
					DialogStarted?.Invoke(this, arg2: false);
				}
			}
		}

		public void MakeCorpse()
		{
			if (Data.IsImposter)
			{
				SelectState(ERoomPeopleState.Corpse);
				_image.material = UnityEngine.Object.Instantiate(_corpseShader);
				_image.material.SetTexture("_MaskTex", Data.CorpseMask);
			}
			else
			{
				SelectState(ERoomPeopleState.Bags);
				transform.SetSiblingIndex(transform.parent.childCount - 2);
			}
			SetClickability(isClickable: false);
		}

		public override void Activate()
		{
			if (!_isGigaDisabled)
			{
				base.Activate();
			}
		}

		public void GigaDeactivate()
		{
			_isGigaDisabled = true;
			Deactivate();
		}

		public void EnableButton()
		{
			_button.IsActive = true;
		}

		public void DisableButton()
		{
			_button.IsActive = false;
		}
	}
}
