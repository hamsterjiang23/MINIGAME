using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using _Code.Utils.UI;

namespace _Code.Infrastructure.Settings.Language
{
	public sealed class TextSettingsDropdownInstance : ATextSettingsInstance
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CRefreshText_003Ed__7 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public TextSettingsDropdownInstance _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[SerializeField]
		private ScrollableDropdown _dropdown;

		[SerializeField]
		private LocalizeStringEvent _textLocalizeEvent;

		[SerializeField]
		private RectTransform _textRectTransform;

		[SerializeField]
		private RTLTextMeshPro _text;

		private IReadOnlyList<Locale> _locales;

		private List<TMP_FontAsset> _fonts;

		private void OnEnable()
		{
		}

		[AsyncStateMachine(typeof(_003CRefreshText_003Ed__7))]
		private UniTaskVoid RefreshText()
		{
			return default;
		}

		private void OnDisable()
		{
		}

		protected override void InitInner()
		{
		}

		private void OnLanguageSelected(int index)
		{
		}

		private void OnTextChanged(string text)
		{
		}

		protected override void UpdateVisualsForLoadedData()
		{
		}

		public override void RequestChangeLanguage()
		{
		}
	}
}
