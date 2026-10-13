using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Infrastructure.Settings.Language
{
	public abstract class ATextSettingsInstance : ASettingsInstance
	{
		[SerializeField]
		private Toggle _useTypewriter;

		protected readonly TextSettings TextSettings;

		protected InputHandling InputHandler;

		[CompilerGenerated]
		private Action m_LanguageChanged;

		public override ISetting Setting => null;

		public event Action LanguageChanged
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

		protected abstract void InitInner();

		protected override void Init()
		{
		}

		private void OnUseTypewriterChanged(bool isUse)
		{
		}

		protected override void UpdateVisualsForLoadedData()
		{
		}

		protected void CallLanguageChanged()
		{
		}

		public void InitModules(InputHandling inputHandler)
		{
		}

		public abstract void RequestChangeLanguage();
	}
}
