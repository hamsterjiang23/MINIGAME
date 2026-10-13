using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Infrastructure.ControlsViewer
{
	public sealed class ControlView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CDisable_003Ed__18 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ControlView _003C_003E4__this;

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
		private RectTransform _rectTransform;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private Image _keyImage;

		[SerializeField]
		private TMP_Text _keyText;

		[SerializeField]
		private RTLTextMeshPro _descriptionText;

		[SerializeField]
		private ControlsSpritesListSOData _controlsSpritesListSOData;

		[SerializeField]
		private ControlSpriteGaypadListSOData _controlsSpritesGaypadListSOData;

		private bool _isAvailable;

		private bool _isEnabled;

		private bool _isChangingNow;

		public EControl Control { get; private set; }

		public void InitKey(string key, EControl control)
		{
		}

		public void InitGamepadKey(string key, EControl control, EGaypadType gaypadType)
		{
		}

		public void SetDescription(string description)
		{
		}

		public void Enable()
		{
		}

		[AsyncStateMachine(typeof(_003CDisable_003Ed__18))]
		public UniTask Disable()
		{
			return default;
		}

		public void SetAvailability(bool isAvailable)
		{
		}

		private void UpdateVisual()
		{
		}
	}
}
