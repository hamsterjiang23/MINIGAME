using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine.Audio;

namespace _Code.Infrastructure.Settings.Sound
{
	public sealed class SoundSettings : ISetting
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInitVolumeAsync_003Ed__9 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public SoundSettings _003C_003E4__this;

			public AudioMixerGroup group;

			public float volume;

			public float volumeSliderValue;

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

		private SoundSettingsData _settings;

		private AudioMixer _audioMixer;

		private bool _isSceneStarted;

		private const string VOLUME_SETTING = "Volume";

		public ASettingsData SettingsData
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public void Init(AudioMixer audioMixer)
		{
		}

		public void SetVolume(AudioMixerGroup group, float volume, float volumeSliderValue)
		{
		}

		[AsyncStateMachine(typeof(_003CInitVolumeAsync_003Ed__9))]
		private UniTaskVoid InitVolumeAsync(AudioMixerGroup group, float volume, float volumeSliderValue)
		{
			return default;
		}

		public void OnStarted()
		{
		}
	}
}
