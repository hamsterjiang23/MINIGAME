using UnityEngine;
using _Code.Utils.EditorWindows;

namespace _Code.Infrastructure.Endings.Data
{
	[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Endings Data", order = 1)]
	[TabsNames(new string[] { "MainData" })]
	public sealed class EndingsSOData : DataClass
	{
		[Header("Killer ending")]
		[TabIndex(0)]
		[SerializeField]
		private int _killsCount;

		[TabIndex(0)]
		[SerializeField]
		private int _bodyEaterCount;

		[Header("Death ending")]
		[TabIndex(0)]
		[SerializeField]
		private int _prophetDontCheckSignsDaysNeeded;

		[Header("FEMA")]
		[TabIndex(0)]
		[SerializeField]
		private int _povistkasUsedCount;

		[TabIndex(0)]
		[SerializeField]
		private int _femaCallsCount;

		public int KillsCount => 0;

		public int BodyEaterCount => 0;

		public int ProphetDontCheckSignsDaysNeeded => 0;

		public int FemaCallsCount => 0;

		public int PovistkasUsedCount => 0;
	}
}
