using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class SaveInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInteractAsync_003Ed__25 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public SaveInteractable _003C_003E4__this;

			private UniTask _003CdrinkAnim_003E5__2;

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
		private GameObject _cinemachineCam;

		[SerializeField]
		private GameObject[] _saveObject3D;

		[SerializeField]
		private Transform _movingThing;

		[SerializeField]
		private LocalizedString _localizedString;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _kombuchaSounds;

		private INotAHumanSoundService _soundService;

		private IHUDPresenter _hudPresenter;

		private IDataModelService _dataModelService;

		private IConsumablesController _consumablesController;

		private IPlayerService _playerService;

		private ICloseUpsController _closeUpsController;

		private int _actualCount;

		private Vector3 _movingThingStartPosition;

		private bool _isInteracting;

		private IDayNightController _dayNightController;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IDataModelService dataModelService, IConsumablesController consumablesController, IPlayerService playerService, ICloseUpsController closeUpsController, IDayNightController dayNightController, INotAHumanSoundService soundService)
		{
		}

		private void OnDestroy()
		{
		}

		private void OnUpdatedConsumablesCount(EConsumable consumable, int count)
		{
		}

		private void UpdateConsumableCount()
		{
		}

		public override void Interact()
		{
		}

		private void OnClose()
		{
		}

		[AsyncStateMachine(typeof(_003CInteractAsync_003Ed__25))]
		private UniTaskVoid InteractAsync()
		{
			return default;
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		public override void OnLoad()
		{
		}
	}
}
