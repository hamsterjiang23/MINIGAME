using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dream.Ball
{
	public sealed class BallTimeline : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CChangeWomanToSuper_003Ed__22 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public BallTimeline _003C_003E4__this;

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
		private struct _003CLightsOut_003Ed__23 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public BallTimeline _003C_003E4__this;

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
		private TMP_Text _timer;

		[SerializeField]
		private Light[] _lights;

		[SerializeField]
		private Image _fade;

		[SerializeField]
		private CoupleAnimation[] _couples;

		[SerializeField]
		private Transform _farWoman;

		[SerializeField]
		private Image _farWomanImage;

		[SerializeField]
		private Image _myWoman;

		[SerializeField]
		private CameraRotate _cameraRotate;

		[SerializeField]
		private Sprite _superSprite;

		private bool _isFadeOff;

		private bool _areLightsOn;

		private bool _areCouplesCreated;

		private bool _areCouplesWentToPositions;

		private bool _isFarWomanCreated;

		private bool _isFarWomanWentToMe;

		private bool _isFlashedToGetWoman;

		private bool _isStartedDance;

		private bool _areLightsOut;

		private bool _isWomanChangedToSuper;

		private float _startTime;

		private void Start()
		{
		}

		private void Update()
		{
		}

		[AsyncStateMachine(typeof(_003CChangeWomanToSuper_003Ed__22))]
		private UniTaskVoid ChangeWomanToSuper()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CLightsOut_003Ed__23))]
		private UniTaskVoid LightsOut()
		{
			return default;
		}

		private void StartDance()
		{
		}

		private void FlashToGetWoman()
		{
		}

		private void FarWomanToMe()
		{
		}

		private void CreateFarWoman()
		{
		}

		private void CouplesToPositions()
		{
		}

		private void CreateCouples()
		{
		}

		private void LightsOn()
		{
		}

		private void FadeOff()
		{
		}
	}
}
