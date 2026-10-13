using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using UnityEngine;
using _Code.Characters.DialogSystem;

namespace _Code.DialogSystem
{
	public sealed class SubtitlesView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAsyncHide_003Ed__10 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SubtitlesView _003C_003E4__this;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CShowDialogForTime_003Ed__11 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public EInfoMessageType messageType;

			public SubtitlesView _003C_003E4__this;

			public TimeSpan time;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CShowDialogForTime_003Ed__12 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public SubtitlesView _003C_003E4__this;

			public string message;

			public TimeSpan time;

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
		private GameObject _gameObject;

		[SerializeField]
		private RTLTextMeshPro _text;

		[SerializeField]
		private MessageByLanguage[] _messages;

		[SerializeField]
		private Transform _showedPos;

		[SerializeField]
		private Transform _hiddenPos;

		private readonly Queue<(string message, TimeSpan time)> _messageQueue;

		private const float SHOW_HIDE_DURATION = 0.5f;

		public void Show()
		{
		}

		public void Hide()
		{
		}

		[AsyncStateMachine(typeof(_003CAsyncHide_003Ed__10))]
		private UniTask AsyncHide()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CShowDialogForTime_003Ed__11))]
		public UniTask ShowDialogForTime(EInfoMessageType messageType, TimeSpan time)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CShowDialogForTime_003Ed__12))]
		public UniTask ShowDialogForTime(string message, TimeSpan time)
		{
			return default;
		}

		public void UpdateText(string text)
		{
		}
	}
}
