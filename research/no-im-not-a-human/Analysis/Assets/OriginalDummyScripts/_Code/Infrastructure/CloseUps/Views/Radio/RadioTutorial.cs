using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using _Code.Player;

namespace _Code.Infrastructure.CloseUps.Views.Radio
{
	public sealed class RadioTutorial : MonoBehaviour
	{
		[SerializeField]
		private LocalizeStringEvent _firstText;

		[SerializeField]
		private LocalizeStringEvent _secondText;

		[SerializeField]
		private SerializedDictionary<EInputDevice, LocalizedString> _firstString;

		[SerializeField]
		private SerializedDictionary<EInputDevice, LocalizedString> _secondString;

		private InputHandling _inputHandling;

		public void Init(InputHandling inputHandling)
		{
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private void OnDeviceChanged(EInputDevice obj)
		{
		}

		public void Actualize()
		{
		}
	}
}
