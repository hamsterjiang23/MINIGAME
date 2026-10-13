using ECM2;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Code.Player;

public class PlayerController : MonoBehaviour
{
	[Header("Camera Settings")]
	[Tooltip("How far in degrees can you move the camera up.")]
	[SerializeField]
	private float maxPitch = 80f;

	[Tooltip("How far in degrees can you move the camera down.")]
	[SerializeField]
	private float minPitch = -80f;

	[Space(15f)]
	[Tooltip("Mouse look sensitivity")]
	[SerializeField]
	private Vector2 mouseSensitivity = new Vector2(1.5f, 1.25f);

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow.")]
	[SerializeField]
	private GameObject cameraTarget;

	[Space(15f)]
	[Tooltip("Cinemachine Virtual Camera positioned at desired crouched height.")]
	[SerializeField]
	private CinemachineVirtualCamera crouchedCamera;

	[FormerlySerializedAs("unCrouchedCamera")]
	[Tooltip("Cinemachine Virtual Camera positioned at desired un-crouched height.")]
	[SerializeField]
	private CinemachineVirtualCamera normalCamera;

	[Tooltip("Cinemachine Virtual Camera positioned at desired un-crouched height with fixed aim on look at")]
	[SerializeField]
	private CinemachineVirtualCamera lookedAtCamera;

	[Tooltip("Camera noise amplitude gain multiplier.")]
	[SerializeField]
	private float cameraNoiseAmplitudeMultiplier = 1f;

	private CinemachineBasicMultiChannelPerlin _normalNoiseProfile;

	private CinemachineBasicMultiChannelPerlin _crouchedNoiseProfile;

	private Character _character;

	private float _cameraTargetPitch;

	private InputHandling _inputHandler;

	private PlayerInputActions _inputActions;

	private bool _isLookingAt;

	[field: FormerlySerializedAs("<_realCamera>k__BackingField")]
	[field: SerializeField]
	public Camera RealCamera { get; private set; }

	public CinemachineVirtualCamera CurrentCamera
	{
		get
		{
			if (!_character.IsCrouched())
			{
				if (!_isLookingAt)
				{
					return normalCamera;
				}
				return lookedAtCamera;
			}
			return crouchedCamera;
		}
	}

	public void Init(InputHandling inputHandler)
	{
		_inputHandler = inputHandler;
	}

	public void SetIsLookingAtState(bool isLookingAt)
	{
		_isLookingAt = isLookingAt;
		if (_isLookingAt)
		{
			normalCamera.Priority = 10;
			crouchedCamera.Priority = 10;
			lookedAtCamera.Priority = 11;
		}
		else
		{
			lookedAtCamera.Priority = 10;
			crouchedCamera.Priority = (_character.IsCrouched() ? 11 : 10);
			normalCamera.Priority = (_character.IsCrouched() ? 10 : 11);
		}
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
		if (_character == null)
		{
			Debug.LogError("Character component is missing on PlayerController.");
		}
		if (normalCamera != null)
		{
			_normalNoiseProfile = normalCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		}
		if (crouchedCamera != null)
		{
			_crouchedNoiseProfile = crouchedCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		}
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		_character.SetRotationMode(Character.RotationMode.None);
		if (_inputHandler == null)
		{
			Debug.LogError("PlayerInputHandler instance is not found.");
		}
	}

	private void OnEnable()
	{
		_character.Crouched += OnCrouched;
		_character.UnCrouched += OnUnCrouched;
	}

	private void OnDisable()
	{
		_character.Crouched -= OnCrouched;
		_character.UnCrouched -= OnUnCrouched;
	}

	private void Update()
	{
		HandleMovement();
		HandleCrouching();
		HandleRun();
	}

	private void LateUpdate()
	{
		HandleRotation();
		UpdateNoiseAmplitude();
	}

	private void HandleMovement()
	{
		Vector3 direction = new Vector3(_inputHandler.MoveInput.x, 0f, _inputHandler.MoveInput.y);
		Vector3 movementDirection = transform.TransformDirection(direction);
		if (movementDirection.magnitude > 1f)
		{
			movementDirection.Normalize();
		}
		_character.SetMovementDirection(movementDirection);
	}

	private void HandleCrouching()
	{
		if (!_character.canEverCrouch)
		{
			return;
		}
		if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
		{
			if (_inputHandler.CrouchTriggered)
			{
				_character.Crouch();
			}
			else
			{
				_character.UnCrouch();
			}
		}
		else if (_inputHandler.CrouchPressed)
		{
			_character.ChangeCrouchState();
		}
	}

	private void HandleRun()
	{
		if (_inputHandler.RunPerformedThisFrame)
		{
			_character.StartRunning();
		}
		if (_inputHandler.RunReleasedThisFrame)
		{
			_character.StopRunning();
		}
	}

	private void OnCrouched()
	{
		crouchedCamera.Priority = 11;
		normalCamera.Priority = 10;
	}

	private void OnUnCrouched()
	{
		crouchedCamera.Priority = 10;
		normalCamera.Priority = 11;
	}

	private void HandleRotation()
	{
		float value = _inputHandler.LookInput.x * mouseSensitivity.x;
		AddControlYawInput(value);
		float value2 = _inputHandler.LookInput.y * mouseSensitivity.y;
		AddControlPitchInput(value2, minPitch, maxPitch);
	}

	private void AddControlYawInput(float value)
	{
		_character.AddYawInput(value);
	}

	private void AddControlPitchInput(float value, float minValue = -80f, float maxValue = 80f)
	{
		if (value != 0f)
		{
			_cameraTargetPitch = MathLib.ClampAngle(_cameraTargetPitch + value, minValue, maxValue);
			cameraTarget.transform.localRotation = Quaternion.Euler(0f - _cameraTargetPitch, 0f, 0f);
		}
	}

	public float EaseInCubic(float start, float end, float value)
	{
		end -= start;
		return end * value * value * value + start;
	}

	public void ResetNoiseAmplitude()
	{
		if (!(_normalNoiseProfile == null) || !(_crouchedNoiseProfile == null))
		{
			_crouchedNoiseProfile.AmplitudeGain = 0f;
			_normalNoiseProfile.AmplitudeGain = 0f;
		}
	}

	private void UpdateNoiseAmplitude()
	{
		if (!(_normalNoiseProfile == null) || !(_crouchedNoiseProfile == null))
		{
			float magnitude = _character.velocity.magnitude;
			if (_character.IsCrouched())
			{
				float value = magnitude / _character.maxWalkSpeedCrouched;
				_crouchedNoiseProfile.AmplitudeGain = EaseInCubic(0f, _character.maxWalkSpeedCrouched, value) * cameraNoiseAmplitudeMultiplier;
			}
			else
			{
				float value2 = magnitude / _character.maxWalkSpeed;
				_normalNoiseProfile.AmplitudeGain = EaseInCubic(0f, _character.maxWalkSpeedCrouched, value2) * cameraNoiseAmplitudeMultiplier;
			}
			if (!_character.IsOnGround())
			{
				_crouchedNoiseProfile.AmplitudeGain = 0f;
				_normalNoiseProfile.AmplitudeGain = 0f;
			}
		}
	}
}
