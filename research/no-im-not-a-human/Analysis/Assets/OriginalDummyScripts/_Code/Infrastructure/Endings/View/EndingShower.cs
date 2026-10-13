using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Zenject;
using _Code.Characters;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure._NINAH__Endings.View;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Endings.View
{
	public sealed class EndingShower : IEndingShower, IInitializable
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass12_0
		{
			public EEnding endingType;

			internal bool _003CShowEnding_003Eb__0(EndingViewSOData x)
			{
				return false;
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CShowEnding_003Ed__12 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public EEnding endingType;

			public EndingShower _003C_003E4__this;

			private _003C_003Ec__DisplayClass12_0 _003C_003E8__1;

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

		private readonly EndingView _view;

		private readonly EndingViewSOData[] _endings;

		private InputHandling _inputHandling;

		private IHUDPresenter _hudPresenter;

		private readonly IDataModelService _dataModelService;

		private readonly INotAHumanSoundService _soundService;

		private readonly ICharactersManager _charactersManager;

		private readonly IConsumablesController _consumablesController;

		private EEnding _currentEnding;

		public EndingShower(IEndingViewProvider endingViewProvider, IEndingDataProvider endingDataProvider, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, IInputHandlerProvider inputHandlerProvider, IHUDPresenter hudPresenter, ICutscenesManager cutscenesManager, IDataModelService dataModelService, ICharactersManager charactersManager, IConsumablesController consumablesController, WatcherManager watcherManager)
		{
		}

		public void Initialize()
		{
		}

		private void OnEndingUnlocked(bool isGameOver)
		{
		}

		[AsyncStateMachine(typeof(_003CShowEnding_003Ed__12))]
		public UniTaskVoid ShowEnding(EEnding endingType)
		{
			return default;
		}

		private void OnEndingEnded()
		{
		}

		private void OnCutsceneEnded(ECutscene cutscene)
		{
		}
	}
}
