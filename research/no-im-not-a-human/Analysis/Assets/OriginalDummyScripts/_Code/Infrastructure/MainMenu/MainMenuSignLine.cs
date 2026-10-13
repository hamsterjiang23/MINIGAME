using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuSignLine : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CMoveDown_003Ed__13 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public MainMenuSignLine _003C_003E4__this;

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
		private struct _003CMoveUp_003Ed__14 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public MainMenuSignLine _003C_003E4__this;

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
		private MainMenuSignLineElement[] _elements;

		[SerializeField]
		private EMoveDirection _moveDirection;

		[SerializeField]
		private float _moveDelay;

		[SerializeField]
		private float _moveDuration;

		[SerializeField]
		private Sprite[] _spritesArray;

		private bool _isStopped;

		private int _currentFirstElement;

		private int _currentLastElement;

		private CancellationTokenSource _cancellationToken;

		[CompilerGenerated]
		private Action<Sprite> m_Moved;

		public event Action<Sprite> Moved
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

		public void Init()
		{
		}

		[AsyncStateMachine(typeof(_003CMoveDown_003Ed__13))]
		private UniTask MoveDown()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CMoveUp_003Ed__14))]
		private UniTask MoveUp()
		{
			return default;
		}

		private Sprite GetMiddleSprite()
		{
			return null;
		}

		public void Stop()
		{
		}
	}
}
