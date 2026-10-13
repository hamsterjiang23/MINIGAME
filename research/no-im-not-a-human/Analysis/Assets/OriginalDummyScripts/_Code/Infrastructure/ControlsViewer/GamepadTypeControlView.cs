using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Infrastructure.ControlsViewer
{
	public sealed class GamepadTypeControlView : MonoBehaviour
	{
		[SerializeField]
		private SerializedDictionary<EGaypadType, Sprite> _sprites;

		[SerializeField]
		private Image _image;

		public void InitButton(EGaypadType type)
		{
		}
	}
}
