using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuHouseController : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAppear_003Ed__13 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public MainMenuHouseController _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private DOTweenAsyncExtensions.TweenAwaiter _003C_003Eu__2;

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
		private Image _house;

		private float _minDelay;

		private float _maxDelay;

		private float _minDuration;

		private float _maxDuration;

		private float _minAlpha;

		private float _maxAlpha;

		private float _durationIn;

		private float _durationOut;

		private float _delay;

		private float _alpha;

		private float _lastAppear;

		private void Update()
		{
		}

		[AsyncStateMachine(typeof(_003CAppear_003Ed__13))]
		private UniTask Appear()
		{
			return default;
		}
	}
}
