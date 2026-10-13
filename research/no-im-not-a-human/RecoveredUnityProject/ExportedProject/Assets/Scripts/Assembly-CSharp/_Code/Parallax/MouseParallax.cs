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
			ParallaxLayer[] layers = _layers;
			foreach (ParallaxLayer parallaxLayer in layers)
			{
				parallaxLayer.Init(parallaxLayer.Transform.position, parallaxLayer.Transform.eulerAngles);
			}
		}

		private void Update()
		{
			if (gameObject.activeSelf)
			{
				Vector3 mousePosition = Input.mousePosition;
				mousePosition = new Vector3(mousePosition.x / (float)Screen.width - 0.5f, mousePosition.y / (float)Screen.height - 0.5f, 0f);
				ParallaxLayer[] layers = _layers;
				foreach (ParallaxLayer parallaxLayer in layers)
				{
					parallaxLayer.Transform.position = parallaxLayer.StartPosition + mousePosition * parallaxLayer.PositionStrength + new Vector3(Mathf.Sin(Time.time * _autoMovingXTimeScale) * _autoMovingXTimeStrength, Mathf.Cos(Time.time * _autoMovingYTimeScale) * _autoMovingYTimeStrength) * parallaxLayer.PositionStrength;
					parallaxLayer.Transform.localEulerAngles = parallaxLayer.StartRotation + new Vector3(mousePosition.y, 0f - mousePosition.x) * parallaxLayer.RotationStrength + new Vector3(Mathf.Sin(Time.time * _autoMovingXTimeScale) * _autoMovingXTimeStrength, Mathf.Cos(Time.time * _autoMovingYTimeScale) * _autoMovingYTimeStrength) * parallaxLayer.RotationStrength;
				}
			}
		}
	}
}
