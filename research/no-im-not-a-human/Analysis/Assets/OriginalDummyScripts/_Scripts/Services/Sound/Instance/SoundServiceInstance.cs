using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;

namespace _Scripts.Services.Sound.Instance
{
	public sealed class SoundServiceInstance : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CFadeVolume_003Ed__7 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SoundServiceInstance _003C_003E4__this;

			public ESoundSource source;

			public float targetVolume;

			public float duration;

			public Ease ease;

			private DOTweenAsyncExtensions.TweenAwaiter _003C_003Eu__1;

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
		private struct _003CPlayAsync_003Ed__2 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SoundServiceInstance _003C_003E4__this;

			public ESoundSource source;

			public ESound sound;

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
		private struct _003CPlayOneShotAsync_003Ed__3 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SoundServiceInstance _003C_003E4__this;

			public ESoundSource source;

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

		[SerializeReference]
		[SerializeField]
		private SoundServiceSourceArray _soundSources;

		[SerializeField]
		private SoundsList _soundsList;

		[AsyncStateMachine(typeof(_003CPlayAsync_003Ed__2))]
		public UniTask PlayAsync(ESoundSource source, ESound sound)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CPlayOneShotAsync_003Ed__3))]
		public UniTask PlayOneShotAsync(ESoundSource source, ESound clip)
		{
			return default;
		}

		public void Play(ESoundSource source, ESound sound)
		{
		}

		public void PlayOneShot(ESoundSource source, ESound clip)
		{
		}

		public void SetEnabled(ESoundSource source, bool isEnabled)
		{
		}

		[AsyncStateMachine(typeof(_003CFadeVolume_003Ed__7))]
		public UniTask FadeVolume(ESoundSource source, float targetVolume, float duration, Ease ease = Ease.Unset)
		{
			return default;
		}

		public void Stop(ESoundSource source)
		{
		}

		public void Pause(ESoundSource source)
		{
		}

		public void ApplySettingToSource(ESoundSource source, ASoundSetting[] settings)
		{
		}

		public void StartSource(ESoundSource source)
		{
		}

		public GameObject GetGameObject(ESoundSource source)
		{
			return null;
		}

		public AudioSource GetSource(ESoundSource source)
		{
			return null;
		}
	}
}
