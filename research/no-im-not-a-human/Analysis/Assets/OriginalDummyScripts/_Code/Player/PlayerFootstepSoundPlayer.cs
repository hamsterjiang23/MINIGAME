using ECM2;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Player
{
	public class PlayerFootstepSoundPlayer : MonoBehaviour
	{
		[Header("Raycast Settings")]
		[SerializeField]
		private Transform footRayOrigin;

		[SerializeField]
		private float rayDistance;

		[SerializeField]
		private LayerMask surfaceMask;

		[Header("Step Settings")]
		[SerializeField]
		private float stepDistance;

		[SerializeField]
		private float minVelocityToStep;

		[SerializeField]
		private float volumeMultiplier;

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

		private string _currentSurfaceTag;

		private Vector3 _lastPosition;

		private float _distanceAccumulator;

		private void Awake()
		{
		}

		public void InitModules(INotAHumanSoundService soundService)
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void UpdateSurfaceTag()
		{
		}

		private void HandleFootsteps()
		{
		}

		private void PlayFootstep()
		{
		}

		private ESound[] GetStepArrayBySurface()
		{
			return null;
		}

		public void PlayCrouchSound()
		{
		}

		public void PlayUncrouchSound()
		{
		}
	}
}
