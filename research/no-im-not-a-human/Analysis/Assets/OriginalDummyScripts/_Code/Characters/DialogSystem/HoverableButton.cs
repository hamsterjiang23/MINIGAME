using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Characters.DialogSystem
{
	public sealed class HoverableButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CSetBaseDataAfterPause_003Ed__13 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public HoverableButton _003C_003E4__this;

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
		private RTLTextMeshPro _text;

		[SerializeField]
		private AnimatedImage _targetImage;

		[SerializeField]
		private ESoundSource _soundSource;

		[SerializeField]
		private HoverableButtonStyle[] _styles;

		[SerializeField]
		private EDialogButtonStyle _startStyle;

		private HoverableButtonStyle _selectedStyle;

		private INotAHumanSoundService _soundService;

		public void Init(INotAHumanSoundService soundService)
		{
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		public void OnPointerExit(PointerEventData eventData)
		{
		}

		public void OnSelect(BaseEventData eventData)
		{
		}

		public void OnDeselect(BaseEventData eventData)
		{
		}

		public void OnPointerClick(PointerEventData eventData)
		{
		}

		[AsyncStateMachine(typeof(_003CSetBaseDataAfterPause_003Ed__13))]
		private UniTaskVoid SetBaseDataAfterPause()
		{
			return default;
		}

		private void SetBaseData()
		{
		}

		private void SetHoverData()
		{
		}

		private void OnEnable()
		{
		}

		public void SelectStyle(EDialogButtonStyle style)
		{
		}
	}
}
