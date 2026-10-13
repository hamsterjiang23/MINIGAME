using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Utils.UI.ImageAnimating
{
	public sealed class AnimatedImage : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CStartPlayingAnimation_003Ed__12 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public AnimatedImage _003C_003E4__this;

			private float _003CdeltaTime_003E5__2;

			private int _003CplayCount_003E5__3;

			private int _003CtargetPlayCount_003E5__4;

			private int _003Ci_003E5__5;

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
		private Image _image;

		[SerializeField]
		private bool _isPlayFromAwake;

		[SerializeField]
		private AnimationData _animationData;

		[SerializeField]
		private bool _ignoreTimescale;

		private bool _isBreakingAnimation;

		private CancellationTokenSource _cancellationTokenSource;

		public Image Image => null;

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		public void SetData(AnimationData animationData)
		{
		}

		public void StartAnimationOrSetFirstFrame()
		{
		}

		[AsyncStateMachine(typeof(_003CStartPlayingAnimation_003Ed__12))]
		private UniTask StartPlayingAnimation()
		{
			return default;
		}

		public void StopAnimation()
		{
		}
	}
}
