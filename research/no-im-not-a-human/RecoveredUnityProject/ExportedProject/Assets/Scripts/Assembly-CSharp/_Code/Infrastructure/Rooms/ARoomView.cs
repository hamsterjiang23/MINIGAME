using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Menues.HUD;
using _Code.Rooms;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Rooms
{
	public abstract class ARoomView : MonoBehaviour, IRoomView
	{
		[SerializeField]
		protected UIButton[] _uiButtons;

		[Header("Door")]
		[SerializeField]
		private Transform _door;

		[SerializeField]
		private Transform _brokenDoor;

		[SerializeField]
		private DoorTrigger _doorTrigger;

		private Action ActionOnOpen;

		protected CharacterRoomObjectView _dialogCompanion;

		protected IDialogManager DialogManager;

		protected IDayNightController DayNightController;

		protected ICloseUpsController CloseUpsController;

		protected INotAHumanSoundService SoundService;

		protected IHUDPresenter HudPresenter;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private bool _isLeaving;

		[field: SerializeField]
		public float CameraDistance { get; private set; }

		[field: SerializeField]
		public float DialogCameraDistance { get; private set; }

		[field: SerializeField]
		public CharacterRoomObjectView[] CharacterViews { get; private set; }

		[field: SerializeField]
		public ObjectRoomObjectView[] ObjectsViews { get; private set; }

		[field: SerializeField]
		public NarrativeRoomObject[] NarrativeObjects { get; private set; }

		[field: SerializeField]
		public AudioSource WhispersAudioSource { get; private set; }

		[field: SerializeField]
		public Camera LinkedCamera { get; private set; }

		public bool HasDialogNow { get; protected set; }

		public bool IsDead { get; private set; }

		public Vector3 LookDirection => transform.eulerAngles;

		public virtual void Init(IDialogManager dialogManager, IDayNightController dayNightController, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IHUDPresenter hudPresenter, IDataModelService dataModelService)
		{
			DialogManager = dialogManager;
			DayNightController = dayNightController;
			CloseUpsController = closeUpsController;
			SoundService = soundService;
			HudPresenter = hudPresenter;
			CharacterRoomObjectView[] characterViews = CharacterViews;
			for (int i = 0; i < characterViews.Length; i++)
			{
				characterViews[i].Init(DayNightController, DialogManager, CloseUpsController);
			}
			ObjectRoomObjectView[] objectsViews = ObjectsViews;
			for (int i = 0; i < objectsViews.Length; i++)
			{
				objectsViews[i].Init(DayNightController, DialogManager, CloseUpsController);
			}
			NarrativeRoomObject[] narrativeObjects = NarrativeObjects;
			foreach (NarrativeRoomObject obj in narrativeObjects)
			{
				obj.Init(DialogManager, dataModelService);
				obj.Started -= OnNarrativeObjectStarted;
				obj.Started += OnNarrativeObjectStarted;
				obj.Ended -= OnNarrativeObjectEnded;
				obj.Ended += OnNarrativeObjectEnded;
			}
		}

		private void OnNarrativeObjectStarted()
		{
			DisableObjects();
		}

		private void OnNarrativeObjectEnded()
		{
			OnNarrativeObjectEndedAsync().Forget();
		}

		private async UniTaskVoid OnNarrativeObjectEndedAsync()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
			EnableObjects();
		}

		public void SetDialogCompanion(CharacterRoomObjectView character)
		{
			_dialogCompanion = character;
		}

		public void SetOnOpenAction(Action action)
		{
			ActionOnOpen = action;
		}

		public async UniTaskVoid StopWhispering(float time)
		{
			WhispersAudioSource?.DOFade(0f, time);
			await UniTask.Delay(TimeSpan.FromSeconds(time));
			WhispersAudioSource?.Stop();
		}

		public void StartWhispering(AudioClip sound, float time)
		{
			WhispersAudioSource.clip = sound;
			WhispersAudioSource.Play();
			WhispersAudioSource?.DOFade(1f, time);
		}

		protected virtual void OnEnable()
		{
			DialogManager.DialogStarted += OnDialogShowed;
			DialogManager.SubtitleStarted += OnSubtitleShowed;
			DialogManager.DialogEnded += OnDialogHidden;
			DialogManager.Dead += OnDead;
		}

		protected virtual void OnDisable()
		{
			DialogManager.DialogStarted -= OnDialogShowed;
			DialogManager.SubtitleStarted -= OnSubtitleShowed;
			DialogManager.DialogEnded -= OnDialogHidden;
			DialogManager.Dead -= OnDead;
		}

		private void OnDead(string cause, Camera cam)
		{
			IsDead = true;
		}

		private void OnSubtitleShowed()
		{
			HasDialogNow = true;
			DisableObjects();
		}

		protected virtual void OnDialogShowed()
		{
			HasDialogNow = true;
			_dialogCompanion?.Deactivate();
			DisableObjects();
		}

		protected virtual void OnDialogHidden(bool isEndedDialog, bool isEndedSubtitle)
		{
			if (isEndedDialog)
			{
				_dialogCompanion?.Activate();
			}
			OnDialogHiddenAsync().Forget();
			EnableObjectsAsync().Forget();
		}

		private async UniTaskVoid OnDialogHiddenAsync()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
			HasDialogNow = false;
		}

		public virtual void Enter()
		{
			gameObject.SetActive(value: true);
			ActionOnOpen?.Invoke();
			ActionOnOpen = null;
		}

		public virtual void Leave()
		{
			gameObject.SetActive(value: false);
		}

		public async UniTaskVoid Blink(Action action)
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();
			if (gameObject.activeSelf)
			{
				await HudPresenter.FadeIn(0.2f, _cancellationTokenSource.Token);
				if (_isLeaving)
				{
					await UniTask.WaitUntil(() => !_isLeaving, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
					await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
					await UniTask.WaitUntil(() => !gameObject.activeSelf, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
					await UniTask.WaitUntil(() => gameObject.activeSelf, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
					action?.Invoke();
				}
				else
				{
					action?.Invoke();
					await HudPresenter.FadeOut(0.2f, _cancellationTokenSource.Token);
				}
			}
			else
			{
				await UniTask.WaitUntil(() => (bool)gameObject && gameObject.activeSelf, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
				action?.Invoke();
			}
			HasDialogNow = true;
		}

		public void StartLeavingRoom()
		{
			_isLeaving = true;
		}

		public void StopLeavingRoom()
		{
			_isLeaving = false;
		}

		private void OnDestroy()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
		}

		public void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose)
		{
			CharacterRoomObjectView characterRoomObjectView = CharacterViews.FirstOrDefault((CharacterRoomObjectView x) => x.Data.CharacterType == character);
			if (!(characterRoomObjectView == null) && characterRoomObjectView.Data.Poses.FirstOrDefault((RoomObjectState<ERoomPeopleState> x) => x.Name == pose) != null)
			{
				characterRoomObjectView.SelectState(pose);
			}
		}

		public void EnableObjects()
		{
			UIButton[] uiButtons = _uiButtons;
			for (int i = 0; i < uiButtons.Length; i++)
			{
				uiButtons[i].IsActive = true;
			}
		}

		public void DisableObjects()
		{
			UIButton[] uiButtons = _uiButtons;
			for (int i = 0; i < uiButtons.Length; i++)
			{
				uiButtons[i].IsActive = false;
			}
		}

		public void BreakDoor()
		{
			_doorTrigger.Disable();
			_door.gameObject.SetActive(value: false);
			_brokenDoor.gameObject.SetActive(value: true);
		}

		public void RepairDoor()
		{
			_doorTrigger.Enable();
			_door.gameObject.SetActive(value: true);
			_brokenDoor.gameObject.SetActive(value: false);
		}

		private async UniTaskVoid EnableObjectsAsync()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
			EnableObjects();
		}

		public Dictionary<string, int> GetUIBUttonsStates()
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			UIButton[] uiButtons = _uiButtons;
			foreach (UIButton uIButton in uiButtons)
			{
				dictionary.Add(uIButton.gameObject.name, uIButton.IsActiveCount);
			}
			return dictionary;
		}

		public void SetUIButtonState(Dictionary<string, int> buttonStates)
		{
			UIButton[] uiButtons = _uiButtons;
			foreach (UIButton uIButton in uiButtons)
			{
				int activeCount = buttonStates[uIButton.gameObject.name];
				uIButton.Init(activeCount);
			}
		}
	}
}
