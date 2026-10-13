using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using _Code.Player;
using _Code.Utils.UI;

namespace _Code.Infrastructure.Settings.Screen
{
	public sealed class ScreenSettingsInstance : ASettingsInstance
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CTrySetSteamdeckResolution_003Ed__13 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ScreenSettingsInstance _003C_003E4__this;

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
		private struct _003CUpdateVisualsForLoadedDataAsync_003Ed__12 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public ScreenSettingsInstance _003C_003E4__this;

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
		private ScrollableDropdown _resolutionDropdown;

		[SerializeField]
		private ScrollableDropdown _fullScreenDropdown;

		[SerializeField]
		private Toggle _vSyncToggle;

		private readonly ScreenSettings _screenSettings;

		private bool _isSceneStarted;

		private InputHandling _inputHandling;

		public override ISetting Setting => null;

		public void InitModules(InputHandling inputHandling)
		{
		}

		protected override void Init()
		{
		}

		public void OnLanguageChanged()
		{
		}

		protected override void UpdateVisualsForLoadedData()
		{
		}

		[AsyncStateMachine(typeof(_003CUpdateVisualsForLoadedDataAsync_003Ed__12))]
		private UniTaskVoid UpdateVisualsForLoadedDataAsync()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CTrySetSteamdeckResolution_003Ed__13))]
		private UniTask TrySetSteamdeckResolution()
		{
			return default;
		}

		private int FindOptimalResolution()
		{
			return 0;
		}

		private void Start()
		{
		}

		private void OnVSyncChanged(bool isOn)
		{
		}

		private void OnResolutionChanged(int selectedIndex)
		{
		}

		private void OnFullScreenChanged(int selectedIndex)
		{
		}
	}
}
