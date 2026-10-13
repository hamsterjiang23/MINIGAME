using System;
using System.Linq;
using RTLTMPro;
using UnityEngine;
using Yarn.Unity;
using _Code.Characters.DialogSystem;
using _Code.Utils;

namespace _Code.DialogSystem;

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
			return _option;
		}
		set
		{
			_option = value;
			hasSubmittedOptionSelection = false;
			OnOptionSelected = (Action<DialogueOption>)Delegate.Remove(OnOptionSelected, new Action<DialogueOption>(ConsumeEnergy));
			OnOptionSelected = (Action<DialogueOption>)Delegate.Remove(OnOptionSelected, new Action<DialogueOption>(OnDayDialogSelected));
			OnOptionSelected = (Action<DialogueOption>)Delegate.Remove(OnOptionSelected, new Action<DialogueOption>(WereBadBoy));
			string text = value.Line.Text.Text;
			_button.SelectStyle(EDialogButtonStyle.Default);
			string[] metadata = value.Line.Metadata;
			if (metadata != null && metadata.Contains("energy"))
			{
				_button.SelectStyle(EDialogButtonStyle.Energy);
				OnOptionSelected = (Action<DialogueOption>)Delegate.Combine(OnOptionSelected, new Action<DialogueOption>(ConsumeEnergy));
				_text.text = text;
			}
			else
			{
				string[] metadata2 = value.Line.Metadata;
				if (metadata2 != null && metadata2.Contains("get_item"))
				{
					_button.SelectStyle(EDialogButtonStyle.ConsumablesGet);
					_text.text = text;
				}
				else
				{
					string[] metadata3 = value.Line.Metadata;
					if (metadata3 != null && metadata3.Contains("give_item"))
					{
						_button.SelectStyle(EDialogButtonStyle.ConsumablesGive);
						_text.text = text;
					}
					else
					{
						string[] metadata4 = value.Line.Metadata;
						if (metadata4 != null && metadata4.Contains("gun"))
						{
							_button.SelectStyle(EDialogButtonStyle.Gun);
							_text.text = text;
						}
						else
						{
							string[] metadata5 = value.Line.Metadata;
							if (metadata5 != null && metadata5.Contains("day_dialog"))
							{
								_text.text = text;
								OnOptionSelected = (Action<DialogueOption>)Delegate.Combine(OnOptionSelected, new Action<DialogueOption>(OnDayDialogSelected));
							}
							else
							{
								string[] metadata6 = value.Line.Metadata;
								if (metadata6 != null && metadata6.Contains("skip"))
								{
									_button.SelectStyle(EDialogButtonStyle.Skip);
									_text.text = text;
								}
								else
								{
									_text.text = text;
								}
							}
						}
					}
				}
			}
			string[] metadata7 = value.Line.Metadata;
			if (metadata7 != null && metadata7.Contains("rude"))
			{
				OnOptionSelected = (Action<DialogueOption>)Delegate.Combine(OnOptionSelected, new Action<DialogueOption>(WereBadBoy));
			}
			interactable = value.IsAvailable;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if ((object)_dialogRunner == null)
		{
			_dialogRunner = UnityEngine.Object.FindFirstObjectByType<FakeDialogRunner>(FindObjectsInactive.Exclude);
		}
		_text.GetComponent<FontChanger>().Init();
	}

	private void OnDayDialogSelected(DialogueOption obj)
	{
		_dialogRunner.OnDayDialogSelected();
	}

	private void ConsumeEnergy(DialogueOption obj)
	{
		_dialogRunner.OnEnergyConsumed();
	}

	private void WereBadBoy(DialogueOption obj)
	{
		_dialogRunner.OnWereBadBoy();
	}
}
