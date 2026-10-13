using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Rooms;
using _Scripts.Services.Sound.Service;

namespace _Code.ActionObjects.LookAtObject
{
	public class PeepholeTrigger : AActionableObjectView
	{
		[SerializeField]
		private GameObject _peepholeCam;

		[SerializeField]
		private Volume _volume;

		[SerializeField]
		private float _distortionScale;

		[FormerlySerializedAs("_linkedRoom")]
		[SerializeField]
		private ERoom _linkedRoom;

		private IRoom _room;

		private LensDistortion _lensDistortion;

		private WatcherManager _watcherManager;

		private IRoom Room => null;

		protected override bool CanLeave => false;

		protected override Func<UniTask> ExtraActionIn => null;

		protected override Func<UniTask> ExtraActionOut => null;

		protected override Func<UniTask> ExtraActionInE { get; }

		protected override Func<UniTask> ExtraActionOutE { get; }

		public override void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
		}
	}
}
