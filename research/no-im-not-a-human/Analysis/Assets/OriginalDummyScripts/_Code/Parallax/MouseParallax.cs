using UnityEngine;

namespace _Code.Parallax
{
	public sealed class MouseParallax : MonoBehaviour
	{
		[SerializeField]
		private ParallaxLayer[] _layers;

		[SerializeField]
		private float _autoMovingXTimeScale;

		[SerializeField]
		private float _autoMovingYTimeScale;

		[SerializeField]
		private float _autoMovingXTimeStrength;

		[SerializeField]
		private float _autoMovingYTimeStrength;

		private void OnEnable()
		{
		}

		private void Update()
		{
		}
	}
}
