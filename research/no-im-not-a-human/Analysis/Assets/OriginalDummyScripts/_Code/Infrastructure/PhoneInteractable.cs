using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Menues.HUD;

namespace _Code.Infrastructure
{
	public sealed class PhoneInteractable : AInteractableObject
	{
		[SerializeField]
		private GameObject _receiver;

		private ICloseUpsController _closeUpsController;

		private IHUDPresenter _hudPresenter;

		private IDayNightController _dayNightController;

		[CompilerGenerated]
		private Action m_Entered;

		[CompilerGenerated]
		private Action m_Exited;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public event Action Entered
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

		public event Action Exited
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

		public void Init(ICloseUpsController closeUpsController, IHUDPresenter hudPresenter, IPauseController pauseController, IDayNightController dayNightController)
		{
		}

		private void ClosePhone()
		{
		}

		public override void Interact()
		{
		}
	}
}
