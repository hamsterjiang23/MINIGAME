using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Sound;

namespace _Scripts.Services.Sound.Service
{
	public sealed class NotAHumanSoundService : SoundService, INotAHumanSoundService, ISoundService
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CPlayFromEventusAsync_003Ed__6 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public bool isCycle;

			public NotAHumanSoundService _003C_003E4__this;

			public ESoundSource source;

			public ESound sound;

			public float fadeTime;

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
		private struct _003CStopFromEventusAsync_003Ed__7 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public NotAHumanSoundService _003C_003E4__this;

			public ESoundSource source;

			public float fadeTime;

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

		private readonly IMusicSOData _musicData;

		private readonly CommonEnumEventus _commonEnumEventus;

		public NotAHumanSoundService(ISoundServiceInstanceProvider soundServiceInstanceProvider, IMusicSOData musicSoData, CommonEnumEventus commonEnumEventus)
			: base(null)
		{
		}

		private void PlayFromEventus(ESoundSource source, ESound sound, float fadeTime, bool isCycle)
		{
		}

		private void StopFromEventus(ESoundSource source, float fadeTime)
		{
		}

		private void PlayFromEventusOneShot(ESoundSource source, ESound sound)
		{
		}

		[AsyncStateMachine(typeof(_003CPlayFromEventusAsync_003Ed__6))]
		private UniTask PlayFromEventusAsync(ESoundSource source, ESound sound, float fadeTime, bool isCycle)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CStopFromEventusAsync_003Ed__7))]
		private UniTask StopFromEventusAsync(ESoundSource source, float fadeTime)
		{
			return default;
		}

		public void DisableDayTheme(float time)
		{
		}

		public void DisableNightTheme(float time)
		{
		}

		public void EnableNightTheme(int day, float time)
		{
		}

		public void EnableDayTheme(int day, float time)
		{
		}
	}
}
