using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using TMPro;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Infrastructure._NINAH__CloseUps.Views.Phone.Pins;
using _Code.Player;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.CloseUps.Views.Phone
{
	public sealed class PhoneCloseUpView : ACloseUpView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CCall_003Ed__39 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public PhoneCloseUpView _003C_003E4__this;

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
		private struct _003CCallTo_003Ed__40 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public PhoneCloseUpView _003C_003E4__this;

			public EPhoneSubscriber phoneSubscriber;

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
		private struct _003CEmptyCall_003Ed__41 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public PhoneCloseUpView _003C_003E4__this;

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
		private struct _003CHide_003Ed__33 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public PhoneCloseUpView _003C_003E4__this;

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
		private struct _003CShow_003Ed__32 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public PhoneCloseUpView _003C_003E4__this;

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
		private struct _003CTalkTo_003Ed__43 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public EPhoneSubscriber phoneSubscriber;

			public PhoneCloseUpView _003C_003E4__this;

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
		private struct _003CTryCall_003Ed__38 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public PhoneCloseUpView _003C_003E4__this;

			private EPhoneSubscriber _003Csubscriber_003E5__2;

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
		private PhoneButtonView[] _phoneButtonViews;

		[SerializeField]
		private TMP_Text _screenText;

		[SerializeField]
		private CharacterSOData _phoneCharacter;

		[SerializeField]
		private PhonePinController _phonePinController;

		[SerializeField]
		private GameObject _camera;

		private string _screenContent;

		private EPhoneState _phoneState;

		private readonly PhoneNumbersController _phoneNumbersController;

		private IDialogManager _dialogManager;

		private IDayNightController _dayNightController;

		private ICursorController _cursorController;

		private CloseUpSaveData _saveData;

		private OtherGameSOData _otherGameSOData;

		private const float MaxRealCallDelay = 5f;

		private const float MinRealCallDelay = 2.5f;

		private bool _canLeave;

		private ICharactersManager _charactersManager;

		private string _lastActualNumber;

		private InputHandling _inputHandling;

		private bool _isActive;

		private PhoneButtonView _selectedButton;

		private IDataModelService _dataModelService;

		private WatcherManager _watcherManager;

		public override IUpdateable[] Updateables => null;

		public bool IsAnimating { get; private set; }

		public override void Init()
		{
		}

		public void InitModules(IDialogManager dialogManager, IDayNightController dayNightController, CloseUpSaveData saveData, OtherGameSOData otherGameSOData, ICursorController cursorController, ICharactersManager charactersManager, InputHandling inputHandling, IDataModelService dataModelService, WatcherManager watcherManager)
		{
		}

		private void OnDayChanged(int day)
		{
		}

		[AsyncStateMachine(typeof(_003CShow_003Ed__32))]
		public override UniTask Show()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHide_003Ed__33))]
		public override UniTask Hide()
		{
			return default;
		}

		private void Update()
		{
		}

		private void OnSelected(PhoneButtonView button)
		{
		}

		private void OnPhoneButtonPressedUp(EPhoneKey key)
		{
		}

		private void OnPhoneButtonPressed(EPhoneKey key)
		{
		}

		[AsyncStateMachine(typeof(_003CTryCall_003Ed__38))]
		private UniTaskVoid TryCall()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CCall_003Ed__39))]
		private UniTask Call()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CCallTo_003Ed__40))]
		private UniTask CallTo(EPhoneSubscriber phoneSubscriber)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CEmptyCall_003Ed__41))]
		private UniTask EmptyCall()
		{
			return default;
		}

		private void ResetPhone()
		{
		}

		[AsyncStateMachine(typeof(_003CTalkTo_003Ed__43))]
		private UniTask TalkTo(EPhoneSubscriber phoneSubscriber)
		{
			return default;
		}

		private void OnDialogEnded(bool dialogState, bool subtitleState)
		{
		}

		private void UpdateScreen()
		{
		}

		public void UnlockPhoneSubscriber(EPhoneSubscriber phoneSubscriber)
		{
		}

		public string GetPhoneNumber(EPhoneSubscriber subscriber)
		{
			return null;
		}

		public void RestorePhoneNumbers(List<EPhoneSubscriber> phoneSubscribers)
		{
		}

		public void ReinitSaveData(CloseUpSaveData saveData)
		{
		}
	}
}
