using RTLTMPro;
using UnityEngine;
using Yarn.Unity;
using _Code.Characters.DialogSystem;

namespace _Code.DialogSystem
{
	public sealed class FakeOptionView : OptionView
	{
		[SerializeField]
		private HoverableButton _button;

		[SerializeField]
		private RTLTextMeshPro _text;

		private FakeDialogRunner _dialogRunner;

		private DialogueOption _option;

		public override DialogueOption Option
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		protected override void OnEnable()
		{
		}

		private void OnDayDialogSelected(DialogueOption obj)
		{
		}

		private void ConsumeEnergy(DialogueOption obj)
		{
		}

		private void WereBadBoy(DialogueOption obj)
		{
		}
	}
}
