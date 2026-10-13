using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Windows;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.ActionObjects.LookAtObject
{
	public sealed class WindowBlindsTrigger : AActionableObjectView
	{
		[SerializeField]
		private Transform[] _lines;

		[SerializeField]
		private Vector3 _rotateVector;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private Material _windowMaterial;

		[SerializeField]
		private Material _blindsMaterial;

		[SerializeField]
		private float _blindsAlphaOnLookingIn;

		[SerializeField]
		private WindowView _linkedWindow;

		private Color _baseBlindsColor;

		private Vector3 _baseAngle;

		private WatcherManager _watcherManager;

		protected override bool CanLeave => false;

		protected override Func<UniTask> ExtraActionIn => null;

		protected override Func<UniTask> ExtraActionOut => null;

		protected override Func<UniTask> ExtraActionInE => null;

		protected override Func<UniTask> ExtraActionOutE => null;

		private void Awake()
		{
		}

		public override void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
		}

		protected override void ExtraActionBeforeIn()
		{
		}
	}
}
