using RTLTMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace _Code.Infrastructure.CloseUps.Views.Phone.Pins
{
	public sealed class PhonePinView : MonoBehaviour
	{
		[SerializeField]
		private RTLTextMeshPro _numberText;

		[SerializeField]
		private LocalizeStringEvent _nameText;

		[SerializeField]
		private MeshRenderer _pinMeshRenderer;

		public EPhoneSubscriber PhoneSubscriber { get; private set; }

		public void Create(LocalizedString sub, string number, Material material, EPhoneSubscriber subscriber)
		{
		}
	}
}
