using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Endings.View;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure._NINAH__Endings.View
{
	public sealed class EndingView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CResetSkipHint_003Ed__34 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public EndingView _003C_003E4__this;

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
		private struct _003CShowSlides_003Ed__32 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public EndingView _003C_003E4__this;

			public EndingViewDataSlide[] dataSlides;

			private int _003CcurrentSlide_003E5__2;

			private string[] _003CtextLines_003E5__3;

			private int _003CcurrentTextLine_003E5__4;

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
		private struct _003CShowSubtitles_003Ed__31 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public EndingSubtitlesData dataSubtitles;

			public EndingView _003C_003E4__this;

			private EndingSubtitleData[] _003C_003E7__wrap1;

			private int _003C_003E7__wrap2;

			private EndingSubtitleData _003Csubtitle_003E5__4;

			private float _003ClineDuration_003E5__5;

			private UniTask.Awaiter _003C_003Eu__1;

			private string[] _003C_003E7__wrap5;

			private int _003C_003E7__wrap6;

			private string _003Cnode_003E5__8;

			private float _003CappearTime_003E5__9;

			private float _003CshowTime_003E5__10;

			private int _003Ci_003E5__11;

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
		private struct _003CStartCheckingVideoEnd_003Ed__30 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public float length;

			public EndingView _003C_003E4__this;

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

		[Obsolete]
		[SerializeField]
		private AnimatedImage _image;

		[SerializeField]
		private VideoPlayer _videoPlayer;

		[SerializeField]
		private RTLTextMeshPro _subtitlesText;

		[SerializeField]
		private CanvasGroup _skipFillHint;

		[SerializeField]
		private Image _skippFillBar;

		[SerializeField]
		private ControlView _controlView;

		private INotAHumanSoundService _soundService;

		private InputHandling _inputHandler;

		private CustomYarnReader _yarnReader;

		private bool _clicked;

		private CancellationTokenSource _cancellationToken;

		private CancellationTokenSource _cancellationTokenSubtitles;

		private float _skipProgress;

		private int _resetsHintCount;

		private bool _isShowing;

		private bool _isSkipping;

		private IDataModelService _dataModelService;

		private EndingViewSOData _currentEnding;

		private bool _isUnlock;

		private WatcherManager _watcherManager;

		[CompilerGenerated]
		private Action m_Ended;

		[CompilerGenerated]
		private Action<bool> m_EndingUnlocked;

		private const float SkipDuration = 1.5f;

		public event Action Ended
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

		public event Action<bool> EndingUnlocked
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

		public void Init(INotAHumanSoundService soundService, CustomYarnReader yarnReader, InputHandling inputHandling, IDataModelService dataModelService, WatcherManager watcherManager)
		{
		}

		private void OnInputChanged(EInputDevice device)
		{
		}

		public void ShowEnding(EndingViewSOData data, bool isUnlock = false)
		{
		}

		[AsyncStateMachine(typeof(_003CStartCheckingVideoEnd_003Ed__30))]
		private UniTaskVoid StartCheckingVideoEnd(float length)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CShowSubtitles_003Ed__31))]
		private UniTask ShowSubtitles(EndingSubtitlesData dataSubtitles)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CShowSlides_003Ed__32))]
		[Obsolete]
		private UniTaskVoid ShowSlides(EndingViewDataSlide[] dataSlides)
		{
			return default;
		}

		private void Update()
		{
		}

		[AsyncStateMachine(typeof(_003CResetSkipHint_003Ed__34))]
		private UniTaskVoid ResetSkipHint()
		{
			return default;
		}
	}
}
