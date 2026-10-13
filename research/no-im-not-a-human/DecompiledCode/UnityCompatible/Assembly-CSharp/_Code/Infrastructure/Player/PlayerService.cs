using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Updatable;
using _Code.Player;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Player
{
	public sealed class PlayerService : ASavableClass<PlayerServiceSaveData>, IPlayerService, IUpdateable, IDisposable
	{
		private PlayerServiceSaveData _saveData = new PlayerServiceSaveData();

		private StartPoint _afterSaveStartPoint;

		private readonly PlayerInstance _playerInstance;

		private readonly ICursorController _cursorController;

		private readonly IDayNightController _dayNightController;

		private readonly IDialogManager _dialogManager;

		private float _lookAtTime;

		private float _lookAtDuration;

		private Vector3 _lookPoint;

		private Quaternion _lookAtStartRotation;

		private bool _isLookingAt;

		private int _immovableCounter;

		private float? _baseFov;

		private readonly CommonEnumEventus _commonEnumEventus;

		private readonly IDataModelService _dataModelService;

		public PlayerSigns Signs { get; private set; } = new PlayerSigns();

		public bool CanLookAround { get; private set; }

		public bool CanMove { get; private set; }

		public bool IsInRoom { get; private set; }

		public Vector3 Position => _playerInstance.Character.position;

		public bool IsCrouched
		{
			set
			{
				if (value)
				{
					_playerInstance.Character.Crouch();
				}
				else
				{
					_playerInstance.Character.UnCrouch();
				}
			}
		}

		public Vector3 LookDirection => _playerInstance.Character.transform.eulerAngles;

		public IUpdateable Updateable => this;

		public bool IsMouseEnabled { get; set; }

		public PlayerService(IPlayerViewProvider playerViewProvider, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, IDataModelService dataModelService, INotAHumanSoundService soundService, IDayNightController dayNightController, IDialogManager dialogManager, CommonEnumEventus commonEnumEventus)
		{
			_playerInstance = playerViewProvider.PlayerInstance;
			_afterSaveStartPoint = playerViewProvider.AfterSaveStartPoint;
			_cursorController = cursorController;
			_dayNightController = dayNightController;
			_dialogManager = dialogManager;
			_dataModelService = dataModelService;
			_playerInstance.Init(inputHandlerProvider.InputHandler);
			Init().Forget();
			_playerInstance.PlayerFootstepSoundPlayer?.InitModules(soundService);
			_playerInstance.PlayerBodyeaterstepPlayer?.InitModules(commonEnumEventus);
			Signs.InitModules(_saveData, dayNightController);
			_commonEnumEventus = commonEnumEventus;
			_dialogManager.PlayerSignShowed -= OnSignShowed;
			_dialogManager.PlayerSignShowed += OnSignShowed;
			_dialogManager.ArmpitsWashed -= Signs.Wash;
			_dialogManager.ArmpitsWashed += Signs.Wash;
			_dayNightController.DayChanged -= OnDayChanged;
			_dayNightController.DayChanged += OnDayChanged;
			LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
			_dataModelService.GameSaveDataHandler.SavedAll -= OnSave;
			_dataModelService.GameSaveDataHandler.SavedAll += OnSave;
		}

		private void OnDayChanged(int day)
		{
			if (day >= _saveData.DayOfTeethReset)
			{
				Signs.ResetTeeth();
			}
			if (day >= _saveData.DayOfHandsReset)
			{
				Signs.ResetHands();
			}
			if (day >= _saveData.DayOfEyesReset)
			{
				Signs.ResetEye();
			}
			if (day >= _saveData.DayOfArmpitReset)
			{
				Signs.ResetArmpits();
			}
		}

		private bool OnSignShowed(ECharacterSign sign)
		{
			_dataModelService.MetaPrefsDataHandler.WatchSelfSign(sign);
			return sign switch
			{
				ECharacterSign.Eye => Signs.IsEyeFake, 
				ECharacterSign.Hands => Signs.IsHandsFake, 
				ECharacterSign.Teeth => Signs.IsTeethFake, 
				ECharacterSign.Armpit => Signs.IsArmpitFake, 
				ECharacterSign.AuraPhoto => Signs.IsPhotoFake, 
				_ => false, 
			};
		}

		public void TeleportTo(StartPoint newLocationStartPoint)
		{
			TeleportToAsync(newLocationStartPoint).Forget();
		}

		public async UniTaskVoid TeleportToAsync(StartPoint newLocationStartPoint)
		{
			_playerInstance.PlayerController.enabled = false;
			_playerInstance.Character.enabled = false;
			await UniTask.DelayFrame(1);
			_playerInstance.Character.TeleportPosition(newLocationStartPoint.transform.position);
			_playerInstance.PlayerController.transform.eulerAngles = newLocationStartPoint.transform.rotation.eulerAngles;
			await UniTask.DelayFrame(1);
			_playerInstance.PlayerController.enabled = true;
			_playerInstance.Character.enabled = true;
		}

		public void SetRunAvailability(bool isAvailable)
		{
			_playerInstance.Character.IsRunningAvailable = isAvailable;
		}

		public void ResetFov(float duration)
		{
			IsInRoom = false;
			DOTween.To(() => _playerInstance.PlayerController.CurrentCamera.m_Lens.FieldOfView, (float x) =>
			{
				_playerInstance.PlayerController.CurrentCamera.m_Lens.FieldOfView = x;
			}, _baseFov.Value, duration).SetEase(Ease.OutCubic);
			_playerInstance.PlayerController.SetIsLookingAtState(isLookingAt: false);
			_playerInstance.PlayerController.CurrentCamera.LookAt = null;
		}

		private async UniTask Init()
		{
		}

		public void MakeImmovable()
		{
			_immovableCounter++;
			CanMove = false;
			CanLookAround = false;
			_playerInstance.Character.enabled = false;
			_playerInstance.PlayerController.enabled = false;
			_playerInstance.PlayerController.ResetNoiseAmplitude();
		}

		public void MakeMovable()
		{
			if (--_immovableCounter <= 0)
			{
				CanMove = true;
				CanLookAround = true;
				_playerInstance.Character.enabled = true;
				_playerInstance.PlayerController.enabled = true;
			}
		}

		public async UniTask LookAtWithZoom(Transform lookAtPos, float fov, float duration)
		{
			_baseFov.GetValueOrDefault();
			if (!_baseFov.HasValue)
			{
				float fieldOfView = _playerInstance.Character.camera.fieldOfView;
				_baseFov = fieldOfView;
			}
			IsInRoom = true;
			_playerInstance.PlayerController.SetIsLookingAtState(isLookingAt: true);
			_playerInstance.PlayerController.CurrentCamera.LookAt = lookAtPos;
			float num = (_lookAtDuration = (_lookAtTime = duration / 2f));
			_lookPoint = lookAtPos.position;
			_lookAtStartRotation = _playerInstance.Character.camera.transform.rotation;
			_isLookingAt = true;
			DOTween.To(() => _playerInstance.PlayerController.CurrentCamera.m_Lens.FieldOfView, (float x) =>
			{
				_playerInstance.PlayerController.CurrentCamera.m_Lens.FieldOfView = x;
			}, fov, duration).SetEase(Ease.OutCubic);
			await UniTask.Delay(TimeSpan.FromSeconds(num));
			_isLookingAt = false;
		}

		public async UniTask MoveXZ(Vector3 position, float duration)
		{
			await (Tween)_playerInstance.Character.transform.DOMove(new Vector3(position.x, _playerInstance.Character.transform.position.y, position.z), duration);
		}

		public void OnUpdateAction()
		{
			if (_isLookingAt)
			{
				Transform transform = _playerInstance.Character.camera.transform;
				Quaternion b = Quaternion.LookRotation(_lookPoint - transform.position, Vector3.up);
				transform.rotation = Quaternion.Lerp(_lookAtStartRotation, b, 1f - _lookAtTime / _lookAtDuration);
				_lookAtTime -= Time.deltaTime;
			}
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
			_saveData = saver.LoadKey(this);
			if (_saveData.Position != Vector3.zero)
			{
				_playerInstance.PlayerController.transform.position = _saveData.Position;
				_playerInstance.Character.SetYaw(_saveData.Yaw);
			}
			else
			{
				TeleportTo(_afterSaveStartPoint);
			}
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}

		private void OnSave(bool isReserve)
		{
			if (isReserve)
			{
				Vector3 vector = _playerInstance?.Character?.transform.position ?? Vector3.zero;
				_saveData.PositionX = vector.x;
				_saveData.PositionY = vector.y;
				_saveData.PositionZ = vector.z;
				if (_playerInstance != null && (bool)_playerInstance.Character)
				{
					_saveData.Yaw = _playerInstance.Character.GetYaw();
				}
				else
				{
					_saveData.Yaw = 0f;
				}
			}
			else
			{
				_saveData.PositionX = 0f;
				_saveData.PositionY = 0f;
				_saveData.PositionZ = 0f;
			}
		}
	}
}
