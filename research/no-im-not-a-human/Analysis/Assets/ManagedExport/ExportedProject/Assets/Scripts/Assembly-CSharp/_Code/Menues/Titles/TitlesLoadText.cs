using System.IO;
using TMPro;
using UnityEngine;

namespace _Code.Menues.Titles
{
	public sealed class TitlesLoadText : MonoBehaviour
	{
		[SerializeField]
		private string _titlesPath;

		[SerializeField]
		private TMP_Text _text;

		private void Start()
		{
			_text.text = File.ReadAllText(_titlesPath);
		}
	}
}
