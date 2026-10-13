using UnityEngine;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure.GameEvents
{
	[TabsNames(new string[] { "Event settings" })]
	[CreateAssetMenu(menuName = "Game Events Data")]
	public sealed class GameEventsManagerSOData : DataClass
	{
		[TabIndex(0)]
		[Tooltip("Minimal time to first guest come after night start")]
		[SerializeField]
		private float _minDelayBeforeDingDongForFirstGuest;

		[TabIndex(0)]
		[Tooltip("Maximum time to first guest come after night start")]
		[SerializeField]
		private float _maxDelayBeforeDingDongForFirstGuest;

		[TabIndex(0)]
		[Tooltip("Minimal time to any guest except first come after previous guest")]
		[SerializeField]
		private float _minDelayBeforeDingDongForGuest;

		[TabIndex(0)]
		[Tooltip("Minimal time to any guest except first come after previous guest")]
		[SerializeField]
		private float _maxDelayBeforeDingDongForGuest;

		public float MinDelayBeforeDingDongForFirstGuest => _minDelayBeforeDingDongForFirstGuest;

		public float MaxDelayBeforeDingDongForFirstGuest => _maxDelayBeforeDingDongForGuest;

		public float MinDelayBeforeDingDongForGuest => _minDelayBeforeDingDongForGuest;

		public float MaxDelayBeforeDingDongForGuest => _maxDelayBeforeDingDongForGuest;
	}
}
