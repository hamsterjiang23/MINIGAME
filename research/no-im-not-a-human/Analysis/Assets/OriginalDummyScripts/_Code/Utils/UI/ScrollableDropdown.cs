using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Utils.UI
{
	public sealed class ScrollableDropdown : MonoBehaviour, ISubmitHandler, IEventSystemHandler, IPointerClickHandler
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CRefreshAsync_003Ed__17 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public ScrollableDropdown _003C_003E4__this;

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
		private bool _isLocalizationDD;

		[SerializeField]
		private FontChangerSOData _fontChangerSOData;

		private ScrollRect _scrollRect;

		private UISelectable[] _selectables;

		private InputHandling _inputHandler;

		[field: SerializeField]
		public TMP_Dropdown Dropdown { get; private set; }

		public void InitModules(InputHandling inputHandling)
		{
		}

		private void OnEnable()
		{
		}

		private void OnDeviceChanged(EInputDevice device)
		{
		}

		private void Refresh()
		{
		}

		private void OnItemSelected(BaseEventData eventData)
		{
		}

		public void OnSubmit(BaseEventData eventData)
		{
		}

		public void OnPointerClick(PointerEventData eventData)
		{
		}

		private void RefreshFonts()
		{
		}

		[AsyncStateMachine(typeof(_003CRefreshAsync_003Ed__17))]
		private UniTaskVoid RefreshAsync()
		{
			return default;
		}

		private void OnLocaleChanged(Locale obj)
		{
		}

		private void OnValueChanged(int value)
		{
		}
	}
}
