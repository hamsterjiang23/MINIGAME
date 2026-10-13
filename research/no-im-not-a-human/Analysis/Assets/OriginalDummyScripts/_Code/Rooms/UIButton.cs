using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Rooms
{
	public sealed class UIButton : MonoBehaviour
	{
		[SerializeField]
		private Image _image;

		private Action _action;

		private const float AlphaThreshold = 0.1f;

		private bool _isHovered;

		private Material _outlineMaterial;

		public static bool AreButtonsEnabled { get; set; }

		public int IsActiveCount { get; private set; }

		public bool IsActive
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		private void Awake()
		{
		}

		public void Init(int activeCount)
		{
		}

		public void SetAction(Action action)
		{
		}

		public void Click()
		{
		}

		public bool IsMousePosInSpriteArea(Vector3 position, Camera cam)
		{
			return false;
		}

		public void OnHover()
		{
		}

		public void OnUnhover()
		{
		}
	}
}
