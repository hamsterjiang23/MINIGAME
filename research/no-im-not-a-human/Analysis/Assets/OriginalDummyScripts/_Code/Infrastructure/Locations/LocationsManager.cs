using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Zenject;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Player;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Menues.HUD;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Locations
{
	public sealed class LocationsManager : ASavableClass<LocationSaveData>, ILocationsManager, IInitializable, IDisposable
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass24_0
		{
			public ELocation location;

			internal bool _003CGoToLocation_003Eb__0(Location x)
			{
				return false;
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CGoToLocation_003Ed__24 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ELocation location;

			public LocationsManager _003C_003E4__this;

			private _003C_003Ec__DisplayClass24_0 _003C_003E8__1;

			private Location _003CnewLocation_003E5__2;

			private ETimeOfDay _003CtimeOfDay_003E5__3;

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
		private struct _003CSetMusic_003Ed__25 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public LocationsManager _003C_003E4__this;

			public Location newLocation;

			public ETimeOfDay timeOfDay;

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
		private struct _003CSetMusicOnLoad_003Ed__27 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public LocationsManager _003C_003E4__this;

			public Location newLocation;

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

		private LocationSaveData _saveData;

		private readonly IHUDPresenter _hudPresenter;

		private readonly IPlayerService _playerService;

		private readonly Location[] _locationsList;

		private readonly IEffectsController _effectsController;

		private Location _currentLocation;

		private readonly IDialogManager _dialogManager;

		private readonly CharacterSOData[] _charactersSOData;

		private readonly INotAHumanSoundService _soundService;

		private readonly IDayNightController _dayNightController;

		private readonly IDataModelService _dataModelService;

		private const float FADE_DURATION = 0.5f;

		[CompilerGenerated]
		private Action<ELocation> m_LocationChanged;

		public ELocation CurrentLocation => ELocation.HouseInterior;

		public bool IsGoingThroughLocations { get; private set; }

		public event Action<ELocation> LocationChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public LocationsManager(ILocationsViewProvider viewProvider, IHUDPresenter hudPresenter, IPlayerService playerService, IDialogManager dialogManager, IEffectsController effectsController, ICharactersSODataProvider charactersSODataProvider, INotAHumanSoundService soundService, IDayNightController dayNightController, IDataModelService dataModelService)
		{
		}

		public void Initialize()
		{
		}

		private void OnWentToLocation(ELocation location)
		{
		}

		[AsyncStateMachine(typeof(_003CGoToLocation_003Ed__24))]
		public UniTask GoToLocation(ELocation location)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CSetMusic_003Ed__25))]
		private UniTask SetMusic(Location newLocation, ETimeOfDay timeOfDay)
		{
			return default;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}

		[AsyncStateMachine(typeof(_003CSetMusicOnLoad_003Ed__27))]
		private UniTask SetMusicOnLoad(Location newLocation, ETimeOfDay currentTimeOfDay)
		{
			return default;
		}
	}
}
