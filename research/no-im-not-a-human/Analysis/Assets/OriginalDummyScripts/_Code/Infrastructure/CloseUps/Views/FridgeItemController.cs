using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.Consumables;

namespace _Code.Infrastructure.CloseUps.Views
{
	public sealed class FridgeItemController : MonoBehaviour
	{
		[SerializeField]
		private FridgeItemView _objectSample;

		[SerializeField]
		private LocalizedString _name;

		[SerializeField]
		private LocalizedString _narrativeDescription;

		[SerializeField]
		private LocalizedString _gameplayDescription;

		[SerializeField]
		private int _maxCount;

		[SerializeField]
		private FridgeItemView[] _baseItems;

		private readonly List<FridgeItemView> _items;

		[CompilerGenerated]
		private Func<EConsumable, bool> m_Used;

		[field: SerializeField]
		public EConsumable ItemType { get; private set; }

		public event Func<EConsumable, bool> Used
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

		public void Init(int count)
		{
		}

		public void PutItem()
		{
		}

		public void UseItem()
		{
		}

		private void RemoveItem()
		{
		}
	}
}
