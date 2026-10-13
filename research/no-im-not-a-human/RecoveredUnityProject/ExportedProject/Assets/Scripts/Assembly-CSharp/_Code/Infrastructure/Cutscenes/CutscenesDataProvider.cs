using UnityEngine;
using UnityEngine.Timeline;

namespace _Code.Infrastructure.Cutscenes
{
	public sealed class CutscenesDataProvider : MonoBehaviour, ICutscenesDataProvider
	{
		[field: SerializeField]
		public SignalAsset EndSignal { get; private set; }

		[field: SerializeField]
		public SignalReceiver SignalReceiver { get; private set; }

		[field: SerializeField]
		public CutsceneData[] Cutscenes { get; private set; }
	}
}
