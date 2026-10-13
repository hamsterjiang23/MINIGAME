using UnityEngine;

namespace _Code.Utils
{
	public sealed class CamOnRail : MonoBehaviour
	{
		[SerializeField]
		private float _speed;

		private void Update()
		{
			base.transform.Translate(Vector3.forward * (_speed * Time.deltaTime));
		}
	}
}
