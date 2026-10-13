using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.Video;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Infrastructure.Rooms
{
	public sealed class BedroomView : ARoomView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnLoad_003Ed__27 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public BedroomView _003C_003E4__this;

			public RoomsSaveData saveData;

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
		private CharacterRoomObjectView _tv;

		[SerializeField]
		private UIButton _tvUiButton;

		[SerializeField]
		private CharacterRoomObjectView _sexy;

		[SerializeField]
		private ObjectRoomObjectView _bed;

		[SerializeField]
		private VideoPlayer _videoPlayer;

		[SerializeField]
		private GameObject _screen;

		[SerializeField]
		private AnimatedImage _babyCurtain;

		[SerializeField]
		private UIButton _babyCurtainButton;

		[SerializeField]
		private UIButton _mushroomlistButton;

		[SerializeField]
		private UIButton _clock;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDayNightController _dayNightController;

		private IDialogManager _dialogManager;

		private RoomsSaveData _saveData;

		[CompilerGenerated]
		private Action m_BabyActivated;

		public event Action BabyActivated
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

		public void Init(Action<CharacterRoomObjectView, bool> tvClickedAction, IGameplayEndingManager gameplayEndingManager, IDialogManager dialogManager, RoomsSaveData roomsSaveData)
		{
		}

		private void OnListClosed()
		{
		}

		private void OnWentToBed()
		{
		}

		private void OnWokeUp()
		{
		}

		private void OnTimeOfDayChanged(ETimeOfDay timeOfDay)
		{
		}

		public void ActivateBaby()
		{
		}

		public void GrowUpBelly()
		{
		}

		private void UpdateSexyState()
		{
		}

		public void SetTVActive(bool isActive)
		{
		}

		public void DisableSexyButton()
		{
		}

		[AsyncStateMachine(typeof(_003COnLoad_003Ed__27))]
		public UniTaskVoid OnLoad(RoomsSaveData saveData)
		{
			return default;
		}
	}
}
