using System;
using UnityEngine;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Infrastructure.Settings.Language
{
	public abstract class ATextSettingsInstance : ASettingsInstance
	{
		[SerializeField]
		private Toggle _useTypewriter;

		protected readonly TextSettings TextSettings = new TextSettings();

		protected InputHandling InputHandler;

		public override ISetting Setting => TextSettings;

		public event Action LanguageChanged;

		protected abstract void InitInner();

		protected override void Init()
		{
			InitInner();
			_useTypewriter.onValueChanged.RemoveAllListeners();
			_useTypewriter.onValueChanged.AddListener(OnUseTypewriterChanged);
		}

		private void OnUseTypewriterChanged(bool isUse)
		{
			TextSettings.IsUseTypewriter = isUse;
		}

		protected override void UpdateVisualsForLoadedData()
		{
			_useTypewriter.isOn = ((TextSettingsData)TextSettings.SettingsData)?.IsUseTypewriter ?? false;
		}

		protected void CallLanguageChanged()
		{
			LanguageChanged?.Invoke();
		}

		public void InitModules(InputHandling inputHandler)
		{
			InputHandler = inputHandler;
		}

		public abstract void RequestChangeLanguage();
	}
}
