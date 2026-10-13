using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using _Code.Characters.DialogSystem;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Rooms;
using _Scripts.Services.Sound;
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

		private IRoom Room
		{
			get
			{
				if (_room == null)
				{
					_room = RoomsManager.GetRoom(_linkedRoom);
				}
				return _room;
			}
		}

		protected override bool CanLeave
		{
			get
			{
				if (!Room.HasDialogNow)
				{
					return !Room.SuspendedExit;
				}
				return false;
			}
		}

		protected override Func<UniTask> ExtraActionIn => async () =>
		{
			_watcherManager.EnterState(EWatcherState.Peephole);
			_volume.profile.TryGet<LensDistortion>(out _lensDistortion);
			Room.Enter();
			DOTween.To(() => _lensDistortion.intensity.value, (float x) =>
			{
				_lensDistortion.intensity.value = x;
			}, _distortionScale, _transitionTime).SetEase(Ease.OutCubic);
			await UniTask.Delay(TimeSpan.FromSeconds(_transitionTime / 2f));
			PlayerService.IsMouseEnabled = true;
			_peepholeCam.SetActive(value: true);
			SoundService.FadeIn(ESoundSource.s1_GameScene_2DSFXSourceExtra, ESound.Night, 1.5f, Ease.InCubic, new SoundSettingCycling()).Forget();
		};

		protected override Func<UniTask> ExtraActionOut => async () =>
		{
			Room.StartLeavingRoom();
			_watcherManager.LeaveState(EWatcherState.Peephole);
			DOTween.To(() => _lensDistortion.intensity.value, (float x) =>
			{
				_lensDistortion.intensity.value = x;
			}, 0f, _transitionTime).SetEase(Ease.InCubic);
			PlayerService.IsMouseEnabled = false;
			await UniTask.Delay(TimeSpan.FromSeconds(_transitionTime / 2f));
			_peepholeCam.SetActive(value: false);
			Room.Leave();
			SoundService.FadeOut(ESoundSource.s1_GameScene_2DSFXSourceExtra, 1.5f).Forget();
			if (GameEventsManager.AreAllDingDongsCompletedThisNight(DayNightController.Day))
			{
				_dialogManager.ShowSubtitlePopup(EInfoMessageType.NightEnd);
			}
			Room.StopLeavingRoom();
		};

		protected override Func<UniTask> ExtraActionInE { get; }

		protected override Func<UniTask> ExtraActionOutE { get; }

		public override void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
			_watcherManager = watcherManager;
			base.Init(hudPresenter, dayNightController, roomsManager, gameEventsManager, closeUpsController, playerService, cursorController, pauseController, soundService, inputHandlerProvider, watcherManager);
			RoomsManager.LinkActionableToRoom(_linkedRoom, this);
		}
	}
}
