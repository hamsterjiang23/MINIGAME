using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.UI;
using _Code.DialogSystem;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Player;
using _Code.Raycast;
using _Code.Utils.CustomYarnReading;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.CloseUps.Views.Radio
{
	public sealed class RadioCloseUpView : ACloseUpView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CHide_003Ed__47 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public RadioCloseUpView _003C_003E4__this;

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
		private struct _003CShow_003Ed__45 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public RadioCloseUpView _003C_003E4__this;

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
		private struct _003CUnlockRaycast_003Ed__46 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public float delay;

			public RadioCloseUpView _003C_003E4__this;

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
		private Image _bg;

		[SerializeField]
		private RTLTextMeshPro _messageBox;

		[SerializeField]
		private RadioKnobController _radioKnobController;

		[SerializeField]
		private RadioButtonsController _radioButtonsController;

		[SerializeField]
		private Image _fill;

		[SerializeField]
		private RaycastTargetHint _raycastTarget;

		[SerializeField]
		private RadioTutorial _tutorial;

		[SerializeField]
		private RectTransform _radioContent;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _voiceSounds;

		private Vector2 _radioStartPos;

		private Vector2 _radioHidePos;

		private RadioModel _radioModel;

		private IOtherGameSODataProvider _otherGameSoDataProvider;

		private CloseUpSaveData _saveData;

		private CustomYarnReader _customYarnReader;

		private IDayNightController _dayNightController;

		private IDialogManager _dialogManager;

		private bool _isTutorActive;

		private InputHandling _inputHandler;

		[CompilerGenerated]
		private Action<int> m_RadioWaveFound;

		private const float MONOSPACE_SCALE = 0.75f;

		private const float MONOSPACE_SCALE_HIEROGLYPHS = 1f;

		private string[] _hieroglyphsLocales;

		private float _monospaceScale;

		private IDataModelService _dataModelService;

		private bool _isAnimating;

		private WatcherManager _watcherManager;

		public override IUpdateable[] Updateables => null;

		public bool HasUsedToday => false;

		public event Action<int> RadioWaveFound
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

		public override void Init()
		{
		}

		private void OnKnobPointerPressedStateChanged(bool isClicked)
		{
		}

		private void OnHoldProgressChanged(float progress)
		{
		}

		public void InitModules(IOtherGameSODataProvider otherGameSoDataProvider, ICustomYarnReaderProvider customYarnReaderProvider, CloseUpSaveData saveData, IDayNightController dayNightController, IDialogManager dialogManager, IInputHandlerProvider inputHandlerProvider, IDataModelService dataModelService, WatcherManager watcherManager)
		{
		}

		private void OnKnobValueChanged(float position)
		{
		}

		private void OnRadioButtonPressed(ERadioState state)
		{
		}

		private void OnEnable()
		{
		}

		private void OnDestroy()
		{
		}

		private void InitRadioModel()
		{
		}

		private void OnTimeLeftChanged(float value)
		{
		}

		private void OnMessageFinished(bool isWaveFound)
		{
		}

		private void OnMessageLineFinished()
		{
		}

		[AsyncStateMachine(typeof(_003CShow_003Ed__45))]
		public override UniTask Show()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CUnlockRaycast_003Ed__46))]
		private UniTaskVoid UnlockRaycast(float delay)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHide_003Ed__47))]
		public override UniTask Hide()
		{
			return default;
		}

		private void SwitchTutorActiveState()
		{
		}

		private void LeaveTutor()
		{
		}

		public void ReinitSaveData(CloseUpSaveData saveData)
		{
		}

		private void Update()
		{
		}
	}
}
