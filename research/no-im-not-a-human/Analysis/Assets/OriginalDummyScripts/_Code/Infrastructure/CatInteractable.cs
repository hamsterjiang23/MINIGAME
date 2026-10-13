using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure._NINAH__Cat;
using _Code.Menues.HUD;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class CatInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CPauseInteracting_003Ed__17 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public CatInteractable _003C_003E4__this;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CTakeAsync_003Ed__22 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public CatInteractable _003C_003E4__this;

			private float _003Cduration_003E5__2;

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
		private ParticleSystem _particleSystem;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _catPurrSounds;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _catGrabSounds;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _catDropSounds;

		private INotAHumanSoundService _soundService;

		private IHUDPresenter _hudPresenter;

		private IActionableObjectsManager _actionableObjectsManager;

		private ICatController _catController;

		[CompilerGenerated]
		private Action m_NaperdyshReleased;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public event Action NaperdyshReleased
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

		public override void Interact()
		{
		}

		public void Pet()
		{
		}

		[AsyncStateMachine(typeof(_003CPauseInteracting_003Ed__17))]
		private UniTaskVoid PauseInteracting()
		{
			return default;
		}

		public void Init(IHUDPresenter hudPresenter, INotAHumanSoundService soundService, IActionableObjectsManager actionableObjectsManager, ICatController catController)
		{
		}

		private void OnDayStarted()
		{
		}

		private void OnNightStarted()
		{
		}

		public void Take()
		{
		}

		[AsyncStateMachine(typeof(_003CTakeAsync_003Ed__22))]
		private UniTaskVoid TakeAsync()
		{
			return default;
		}

		public void ResetAnim()
		{
		}
	}
}
