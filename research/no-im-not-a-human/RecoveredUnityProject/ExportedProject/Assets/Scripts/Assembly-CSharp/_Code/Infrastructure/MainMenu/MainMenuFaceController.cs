using UnityEngine;
using UnityEngine.UI;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuFaceController : MonoBehaviour
	{
		[SerializeField]
		private Image _eyes;

		[SerializeField]
		private Image _teeth;

		public void SetEyes(Sprite eyes)
		{
			_eyes.sprite = eyes;
		}

		public void SetTeeth(Sprite teeth)
		{
			_teeth.sprite = teeth;
		}
	}
}
