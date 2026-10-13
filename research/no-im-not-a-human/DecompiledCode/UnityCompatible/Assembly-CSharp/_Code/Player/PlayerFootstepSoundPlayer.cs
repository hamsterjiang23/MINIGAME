using ECM2;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Player
{
	public class PlayerFootstepSoundPlayer : MonoBehaviour
	{
		[Header("Raycast Settings")]
		[SerializeField]
		private Transform footRayOrigin;

		[SerializeField]
		private float rayDistance = 0.2f;

		[SerializeField]
		private LayerMask surfaceMask;

		[Header("Step Settings")]
		[SerializeField]
		private float stepDistance = 1f;

		[SerializeField]
		private float minVelocityToStep = 0.1f;

		[SerializeField]
		private float volumeMultiplier = 1f;

		[Header("Footstep Sounds")]
		[SerializeField]
		[SearchableEnum]
		private ESound[] woodSteps;

		[SerializeField]
		[SearchableEnum]
		private ESound[] dirtSteps;

		[SerializeField]
		[SearchableEnum]
		private ESound[] waterSteps;

		[SerializeField]
		[SearchableEnum]
		private ESound[] grassSteps;

		[Header("Crouch Sounds")]
		[SerializeField]
		[SearchableEnum]
		private ESound crouchSound;

		[SerializeField]
		[SearchableEnum]
		private ESound uncrouchSound;

		private Character _character;

		private INotAHumanSoundService _soundService;

		private string _currentSurfaceTag = "Default";

		private Vector3 _lastPosition;

		private float _distanceAccumulator;

		private void Awake()
		{
			_character = GetComponent<Character>();
		}

		public void InitModules(INotAHumanSoundService soundService)
		{
			_soundService = soundService;
		}

		private void Start()
		{
			_lastPosition = transform.position;
		}

		private void Update()
		{
			if (!(_character == null) && _character.IsOnGround())
			{
				if (_character.velocity.magnitude < minVelocityToStep)
				{
					_distanceAccumulator = 0f;
					_lastPosition = transform.position;
				}
				else
				{
					UpdateSurfaceTag();
					HandleFootsteps();
				}
			}
		}

		private void UpdateSurfaceTag()
		{
			if (Physics.Raycast(footRayOrigin.position, Vector3.down, out var hitInfo, rayDistance, surfaceMask))
			{
				_currentSurfaceTag = hitInfo.collider.tag;
			}
			else
			{
				_currentSurfaceTag = "Default";
			}
		}

		private void HandleFootsteps()
		{
			Vector3 position = transform.position;
			float num = Vector3.Distance(position, _lastPosition);
			_distanceAccumulator += num;
			_lastPosition = position;
			if (_distanceAccumulator >= stepDistance)
			{
				PlayFootstep();
				_distanceAccumulator = 0f;
			}
		}

		private void PlayFootstep()
		{
			if (_soundService != null)
			{
				ESound[] stepArrayBySurface = GetStepArrayBySurface();
				if (stepArrayBySurface != null && stepArrayBySurface.Length != 0)
				{
					_soundService.PlayOneShot(ESoundSource.s1_GameScene_3DSFXFootstepSoundSource, stepArrayBySurface.GetRandomElementWithoutRepeating(), new SoundSettingRandomPitch(0.05f));
					_soundService.FadeVolume(ESoundSource.s1_GameScene_3DSFXFootstepSoundSource, volumeMultiplier, 0f);
				}
			}
		}

		private ESound[] GetStepArrayBySurface()
		{
			return _currentSurfaceTag switch
			{
				"Wood" => woodSteps, 
				"Dirt" => dirtSteps, 
				"Water" => waterSteps, 
				"Grass" => grassSteps, 
				_ => woodSteps, 
			};
		}

		public void PlayCrouchSound()
		{
			_soundService?.PlayOneShot(ESoundSource.s1_GameScene_3DSFXFootstepSoundSource, crouchSound);
		}

		public void PlayUncrouchSound()
		{
			_soundService?.PlayOneShot(ESoundSource.s1_GameScene_3DSFXFootstepSoundSource, uncrouchSound);
		}
	}
}
