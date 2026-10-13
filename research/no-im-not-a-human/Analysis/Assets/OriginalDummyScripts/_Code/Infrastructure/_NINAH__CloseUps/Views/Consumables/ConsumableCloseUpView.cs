using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Updatable;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Consumables
{
	public sealed class ConsumableCloseUpView : ACloseUpView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CHide_003Ed__27 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ConsumableCloseUpView _003C_003E4__this;

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
		private struct _003CShow_003Ed__26 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ConsumableCloseUpView _003C_003E4__this;

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
		private RTLTextMeshPro _name;

		[SerializeField]
		private RTLTextMeshPro _gameplayDescription;

		[SerializeField]
		private RTLTextMeshPro _narrativeDescription;

		[SerializeField]
		private Button _yesButton;

		[SerializeField]
		private Button _noButton;

		[SerializeField]
		private ConsumableLocalizationsListSOData _localizationsList;

		private EConsumable _selectedConsumable;

		private IConsumablesController _consumablesController;

		private Action _onUseAction;

		private Action _onCloseAction;

		private IDataModelService _dataModelService;

		private WatcherManager _watcherManager;

		protected override bool IsUseFade => false;

		public override IUpdateable[] Updateables { get; }

		public bool IsAnimating { get; private set; }

		private void OnYesClicked()
		{
		}

		private void OnNoClicked()
		{
		}

		public void InitModules(IConsumablesController consumablesController, IHUDPresenter hudPresenter, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController, IDataModelService dataModelService, WatcherManager watcherManager)
		{
		}

		public void SetOnUseAction(Action onUseAction)
		{
		}

		public void SetupConsumable(EConsumable consumable)
		{
		}

		[AsyncStateMachine(typeof(_003CShow_003Ed__26))]
		public override UniTask Show()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHide_003Ed__27))]
		public override UniTask Hide()
		{
			return default;
		}

		public void SetOnCloseAction(Action onClose)
		{
		}

		private void Update()
		{
		}
	}
}
