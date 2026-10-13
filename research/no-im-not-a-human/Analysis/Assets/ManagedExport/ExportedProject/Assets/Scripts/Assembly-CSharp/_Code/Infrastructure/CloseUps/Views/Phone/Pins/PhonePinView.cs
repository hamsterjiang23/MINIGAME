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
			PhoneSubscriber = subscriber;
			base.gameObject.SetActive(value: true);
			_nameText.StringReference = sub;
			_numberText.text = number.Substring(0, 3) + "-" + number.Substring(3, number.Length - 3);
			_pinMeshRenderer.materials[0] = material;
		}
	}
}
