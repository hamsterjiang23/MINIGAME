using ECM2;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Code.Player
{
	public class PlayerController : MonoBehaviour
	{
		[Header("Camera Settings")]
		[Tooltip("How far in degrees can you move the camera up.")]
		[SerializeField]
		private float maxPitch;

		[Tooltip("How far in degrees can you move the camera down.")]
		[SerializeField]
		private float minPitch;

		[Space(15f)]
		[Tooltip("Mouse look sensitivity")]
		[SerializeField]
		private Vector2 mouseSensitivity;

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
		private float cameraNoiseAmplitudeMultiplier;

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

		public CinemachineVirtualCamera CurrentCamera => null;

		public void Init(InputHandling inputHandler)
		{
		}

		public void SetIsLookingAtState(bool isLookingAt)
		{
		}

		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private void Update()
		{
		}

		private void LateUpdate()
		{
		}

		private void HandleMovement()
		{
		}

		private void HandleCrouching()
		{
		}

		private void HandleRun()
		{
		}

		private void OnCrouched()
		{
		}

		private void OnUnCrouched()
		{
		}

		private void HandleRotation()
		{
		}

		private void AddControlYawInput(float value)
		{
		}

		private void AddControlPitchInput(float value, float minValue = -80f, float maxValue = 80f)
		{
		}

		public float EaseInCubic(float start, float end, float value)
		{
			return 0f;
		}

		public void ResetNoiseAmplitude()
		{
		}

		private void UpdateNoiseAmplitude()
		{
		}
	}
}
