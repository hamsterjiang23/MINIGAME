using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using _Code.Infrastructure.Consumables;

namespace _Code.Menues.HUD
{
	public sealed class ItemHintView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CShowAsync_003Ed__8 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ItemHintView _003C_003E4__this;

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
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private LocalizeStringEvent _localizeStringEvent;

		[SerializeField]
		private LocalizedString _receivedString;

		[SerializeField]
		private LocalizedString _givenAwayString;

		private Queue<(EConsumable item, int count, bool isReceiving)> _queue;

		private bool _isShowingNow;

		public void ShowItemGivenAwayHint(EConsumable item, int count)
		{
		}

		public void ShowItemReceivedHint(EConsumable item, int count)
		{
		}

		[AsyncStateMachine(typeof(_003CShowAsync_003Ed__8))]
		private UniTask ShowAsync()
		{
			return default;
		}
	}
}
