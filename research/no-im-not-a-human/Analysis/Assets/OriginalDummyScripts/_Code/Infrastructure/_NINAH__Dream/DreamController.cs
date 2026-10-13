using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.DialogSystem;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure._NINAH__Dream
{
	public sealed class DreamController : ASavableClass<DreamSaveData>, IDreamController
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInvokeExtraDreamActions_003Ed__18 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public EDream dream;

			public DreamController _003C_003E4__this;

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
		private struct _003CShowDream_003Ed__17 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public DreamController _003C_003E4__this;

			private DreamData _003Cdream_003E5__2;

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

		private DreamSaveData _saveData;

		private OtherGameSOData _data;

		private readonly IHUDPresenter _hudPresenter;

		private readonly IDayNightControllerViewProvider _viewProvider;

		private readonly IEffectsController _effectsController;

		private EDream _dreamToShow;

		private readonly INotAHumanSoundService _soundService;

		private readonly WatcherManager _watcherManager;

		private readonly IDataModelService _dataModelService;

		private readonly IDialogManager _dialogManager;

		public bool NeedToShowDream { get; private set; }

		public bool HasSeen(EDream dream)
		{
			return false;
		}

		public DreamController(IOtherGameSODataProvider otherGameSoDataProvider, IHUDPresenter hudPresenter, IDayNightControllerViewProvider viewProvider, IEffectsController effectsController, INotAHumanSoundService soundService, WatcherManager watcherManager, IDataModelService dataModelService, IDialogManager dialogManager)
		{
		}

		public void WantToShowDream(EDream dream)
		{
		}

		[AsyncStateMachine(typeof(_003CShowDream_003Ed__17))]
		public UniTask ShowDream()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CInvokeExtraDreamActions_003Ed__18))]
		private UniTaskVoid InvokeExtraDreamActions(EDream dream)
		{
			return default;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
