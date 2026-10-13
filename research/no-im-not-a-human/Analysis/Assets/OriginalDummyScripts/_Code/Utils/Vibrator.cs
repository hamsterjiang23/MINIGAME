using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;

namespace _Code.Utils
{
	public static class Vibrator
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CVibrateForTime_003Ed__1 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public float lowStrength;

			public float highStrength;

			public float duration;

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

		private static bool _isEnabled;

		[AsyncStateMachine(typeof(_003CVibrateForTime_003Ed__1))]
		public static UniTask VibrateForTime(float duration, float lowStrength, float highStrength)
		{
			return default;
		}

		public static void StartVibrate(float lowStrength, float highStrength)
		{
		}

		public static void StopVibrate()
		{
		}

		public static void SetEnabledState(bool value)
		{
		}
	}
}
