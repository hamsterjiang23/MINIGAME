using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD;
using _Code.Utils.Attributes.MinMaxRange;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class RadioInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CRunRandomSoundCheck_003Ed__21 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public RadioInteractable _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[SerializeField]
		private GameObject _radio3dObject;

		[SerializeField]
		[MinMaxRange(0f, 60f)]
		private Vector2 _randomSoundTimeRange;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _randomSounds;

		private ICloseUpsController _closeUpsController;

		private IHUDPresenter _hudPresenter;

		private IDayNightController _dayNightController;

		private INotAHumanSoundService _soundService;

		private float _randomSoundTime;

		private CancellationTokenSource _cancellationTokenSource;

		[CompilerGenerated]
		private Action m_RadioTaken;

		[CompilerGenerated]
		private Action m_RadioRemoved;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public event Action RadioTaken
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

		public event Action RadioRemoved
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

		public void Init(ICloseUpsController closeUpsController, IHUDPresenter hudPresenter, IPauseController pauseController, IDayNightController dayNightController, INotAHumanSoundService soundService)
		{
		}

		private void OnDayTimeChanged(ETimeOfDay timeOfDay)
		{
		}

		[AsyncStateMachine(typeof(_003CRunRandomSoundCheck_003Ed__21))]
		private UniTask RunRandomSoundCheck()
		{
			return default;
		}

		public override void Interact()
		{
		}

		private void OnRadioOpened()
		{
		}

		private void OnRadioClosed()
		{
		}

		private void AttractAttention()
		{
		}
	}
}
