using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Scripts.Services.Sound.Instance;

namespace _Scripts.Services.Sound.Service
{
	public class SoundService : ISoundService
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CFadeIn_003Ed__5 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SoundService _003C_003E4__this;

			public ESoundSource source;

			public ESound clip;

			public ASoundSetting[] settings;

			public float duration;

			public Ease ease;

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
		private struct _003CFadeOut_003Ed__6 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SoundService _003C_003E4__this;

			public ESoundSource source;

			public float duration;

			public Ease ease;

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
		private struct _003CFadeVolume_003Ed__4 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SoundService _003C_003E4__this;

			public ESoundSource source;

			public float targetVolume;

			public float duration;

			public Ease ease;

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
		private struct _003CPlayAsync_003Ed__8 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SoundService _003C_003E4__this;

			public ESoundSource source;

			public ASoundSetting[] settings;

			public ESound clip;

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
		private struct _003CPlayOneShotAsync_003Ed__7 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SoundService _003C_003E4__this;

			public ESoundSource source;

			public ASoundSetting[] settings;

			public ESound clip;

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

		private readonly SoundServiceInstance _soundServiceInstance;

		public SoundService(ISoundServiceInstanceProvider soundServiceInstanceProvider)
		{
		}

		public void Init()
		{
		}

		public void SetEnabled(ESoundSource source, bool isEnabled)
		{
		}

		[AsyncStateMachine(typeof(_003CFadeVolume_003Ed__4))]
		public UniTask FadeVolume(ESoundSource source, float targetVolume, float duration, Ease ease = Ease.Unset)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CFadeIn_003Ed__5))]
		public UniTask FadeIn(ESoundSource source, ESound clip, float duration, Ease ease = Ease.Unset, params ASoundSetting[] settings)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CFadeOut_003Ed__6))]
		public UniTask FadeOut(ESoundSource source, float duration, Ease ease = Ease.Unset)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CPlayOneShotAsync_003Ed__7))]
		public UniTask PlayOneShotAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CPlayAsync_003Ed__8))]
		public UniTask PlayAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings)
		{
			return default;
		}

		public void PlayOneShot(ESoundSource source, ESound clip, params ASoundSetting[] settings)
		{
		}

		public void Play(ESoundSource source, ESound clip, params ASoundSetting[] settings)
		{
		}

		public void Stop(ESoundSource source)
		{
		}

		public void Pause(ESoundSource source)
		{
		}

		public void Start(ESoundSource source)
		{
		}

		private void ApplySettingToSource(ESoundSource source, ASoundSetting[] settings)
		{
		}

		public GameObject GetSourceObject(ESoundSource source)
		{
			return null;
		}

		public float GetVolume(ESoundSource source)
		{
			return 0f;
		}
	}
}
