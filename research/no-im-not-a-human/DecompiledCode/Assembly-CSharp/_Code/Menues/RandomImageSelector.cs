using UnityEngine;
using UnityEngine.UI;

namespace _Code.Menues;

public sealed class RandomImageSelector : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private Sprite[] _sprites;

	public void Regenerate()
	{
		_image.sprite = _sprites.GetRandomElement();
	}
}
