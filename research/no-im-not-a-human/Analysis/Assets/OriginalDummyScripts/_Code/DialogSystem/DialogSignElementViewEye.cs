using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using _Code.Characters;

namespace _Code.DialogSystem
{
	public sealed class DialogSignElementViewEye : DialogSignElementViewFrameAnimatedCover
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CMoveIris_003Ed__9 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public DialogSignElementViewEye _003C_003E4__this;

			private DOTweenAsyncExtensions.TweenAwaiter _003C_003Eu__1;

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
		private Image _white;

		[SerializeField]
		private Image _iris;

		private bool _isEnabled;

		private CharacterEyeData _eyeData;

		private const float IrisMoveXRadius = 150f;

		private const float IrisMoveYRadius = 100f;

		public void Init(CharacterEyeData eyeData)
		{
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		[AsyncStateMachine(typeof(_003CMoveIris_003Ed__9))]
		private UniTask MoveIris()
		{
			return default;
		}
	}
}
