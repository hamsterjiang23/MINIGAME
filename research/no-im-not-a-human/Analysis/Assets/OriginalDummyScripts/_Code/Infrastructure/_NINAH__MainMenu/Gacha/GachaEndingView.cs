using System;
using System.Runtime.CompilerServices;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.Endings.View;

namespace _Code.Infrastructure._NINAH__MainMenu.Gacha
{
	public sealed class GachaEndingView : MonoBehaviour
	{
		[SerializeField]
		private Image _backgroundImage;

		[SerializeField]
		private Sprite[] _backgroundSprites;

		[SerializeField]
		private Button _button;

		[SerializeField]
		private RTLTextMeshPro _description;

		[SerializeField]
		private Image[] _spoilersImages;

		[SerializeField]
		private Image _watchImage;

		[SerializeField]
		private Sprite _watchImageUnlocked;

		[SerializeField]
		private Sprite _watchImageLocked;

		[SerializeField]
		private Material _imageFogMaterial;

		[SerializeField]
		private Material _imageFogUnlockedMaterial;

		private bool _isUnlocked;

		private EndingViewSOData _data;

		private Material _fogMaterialInstance;

		[CompilerGenerated]
		private Action<EndingViewSOData> m_WatchMovieClicked;

		public event Action<EndingViewSOData> WatchMovieClicked
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		public void Init(EndingViewSOData data, bool isUnlocked, int index)
		{
		}

		private void WatchMovie()
		{
		}
	}
}
