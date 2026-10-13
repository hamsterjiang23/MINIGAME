using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine.Localization.Tables;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Player;
using _Code.Utils.CustomYarnReading;

namespace _Code.Infrastructure.CloseUps.Views.Radio
{
	public class RadioModel
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CStartUpdatingMessage_003Ed__59 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public RadioModel _003C_003E4__this;

			private float _003Cdelay_003E5__2;

			private float _003CstartDelay_003E5__3;

			private Cysharp.Threading.Tasks.YieldAwaitable.Awaiter _003C_003Eu__1;

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

		private readonly float _delta;

		private readonly float _thresholdStart;

		private readonly float _wrongStateMultiplier;

		private string _message;

		private float[] _thresholdMap;

		private float _currentPosition;

		private CloseUpSaveData _saveData;

		private readonly CustomYarnReader _customYarnReader;

		private readonly InputHandling _inputHandler;

		[CompilerGenerated]
		private Action<ERadioState> m_StateChanged;

		private StringTableEntry _noiseCharsLocalization;

		private string[] _currentText;

		private int _currentTextIndex;

		private float _delayBeforeShowingNextText;

		private bool _isForceStopped;

		private bool _isPaused;

		private float _onFoundProgresss;

		private bool _isWaveFound;

		private float _speedUpStrength;

		private const float BaseDelay = 2f;

		private const float BaseDelayForEachCharacter = 0.1f;

		private const float FarFromCorrectWaveModifier = 0.01f;

		private const string PUNCTUATION_CHARS = "!\"#$%&'()*+, -./:;<=>?@[\\]^_`{|}~—“”“”«»¡—‘’“”„…、。々《》「」！（），：？ｈ\n";

		[CompilerGenerated]
		private Action<bool> m_MessageFinished;

		[CompilerGenerated]
		private Action m_MessageLineFinished;

		[CompilerGenerated]
		private Action<float> m_TimeLeftChanged;

		public float TargetPosition { get; private set; }

		private float PositionCorrectnessError => 0f;

		public float CurrentPosition
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public ERadioState CurrentState { get; private set; }

		public ERadioState TargetState { get; private set; }

		public float NormalisedDistance { get; private set; }

		private float SpeedModifier => 0f;

		public event Action<ERadioState> StateChanged
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

		public event Action<bool> MessageFinished
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

		public event Action MessageLineFinished
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

		public event Action<float> TimeLeftChanged
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

		public RadioModel(float delta, float thresholdStart, float wrongStateMultiplier, CustomYarnReader customYarnReader, CloseUpSaveData saveData, InputHandling inputHandler)
		{
		}

		public void InitNode(string nodeName)
		{
		}

		[AsyncStateMachine(typeof(_003CStartUpdatingMessage_003Ed__59))]
		private UniTaskVoid StartUpdatingMessage()
		{
			return default;
		}

		public void ForceStop()
		{
		}

		private void Setup()
		{
		}

		public void SwitchState(ERadioState newState)
		{
		}

		public void UpdateMessage(string newMessage)
		{
		}

		private void CalculateThresholdMap()
		{
		}

		private void ShuffleArray(float[] array)
		{
		}

		public string GetDisplayedMessage()
		{
			return null;
		}

		public void SetPauseState(bool isPaused)
		{
		}

		public void ResetDay()
		{
		}

		public void SetSpeedUpStrength(float strength)
		{
		}
	}
}
