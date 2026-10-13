using System;
using System.Collections.Generic;
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

		private readonly List<FridgeItemView> _items = new List<FridgeItemView>();

		[field: SerializeField]
		public EConsumable ItemType { get; private set; }

		public event Func<EConsumable, bool> Used;

		public void Init(int count)
		{
			if (_items.Count > count)
			{
				while (_items.Count > count)
				{
					RemoveItem();
				}
			}
			else
			{
				while (_items.Count < count && _items.Count < _maxCount)
				{
					PutItem();
				}
			}
			FridgeItemView[] baseItems = _baseItems;
			foreach (FridgeItemView fridgeItemView in baseItems)
			{
				fridgeItemView.Used += UseItem;
				fridgeItemView.InitGotTexts(() => (_name.GetLocalizedString(), _narrativeDescription.GetLocalizedString(), _gameplayDescription.GetLocalizedString()), ItemType);
				_items.Add(fridgeItemView);
			}
		}

		public void PutItem()
		{
			if (_items.Count < _maxCount)
			{
				FridgeItemView fridgeItemView = UnityEngine.Object.Instantiate(_objectSample, base.transform);
				fridgeItemView.Used += UseItem;
				fridgeItemView.InitGotTexts(() => (_name.GetLocalizedString(), _narrativeDescription.GetLocalizedString(), _gameplayDescription.GetLocalizedString()), ItemType);
				_items.Add(fridgeItemView);
			}
		}

		public void UseItem()
		{
			Used?.Invoke(ItemType);
		}

		private void RemoveItem()
		{
			List<FridgeItemView> items = _items;
			UnityEngine.Object.Destroy(items[items.Count - 1].gameObject);
			List<FridgeItemView> items2 = _items;
			List<FridgeItemView> items3 = _items;
			items2.Remove(items3[items3.Count - 1]);
		}
	}
}
