using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SABI
{
	public static class MonoBehaviourExtensions
	{
		[CompilerGenerated]
		private sealed class _003CExecute_003Ed__1 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public float delay;

			public Action callback;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CExecute_003Ed__1(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CExecuteAfterFrame_003Ed__3 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public Action callback;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CExecuteAfterFrame_003Ed__3(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CRepeatWhileCoroutine_003Ed__7 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public float interval;

			public Action callback;

			public Func<bool> condition;

			public bool expectedResult;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CRepeatWhileCoroutine_003Ed__7(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CWaitForCondition_003Ed__5 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public Func<bool> condition;

			public bool expectedResult;

			public Action callback;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CWaitForCondition_003Ed__5(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		public static MonoBehaviour DelayedExecution(this MonoBehaviour monoBehaviour, float delay, Action callback)
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CExecute_003Ed__1))]
		private static IEnumerator Execute(float delay, Action callback)
		{
			return null;
		}

		public static MonoBehaviour DelayedExecutionUntilNextFrame(this MonoBehaviour monoBehaviour, Action callback)
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CExecuteAfterFrame_003Ed__3))]
		private static IEnumerator ExecuteAfterFrame(Action callback)
		{
			return null;
		}

		public static MonoBehaviour DelayedExecutionUntil(this MonoBehaviour monoBehaviour, Func<bool> condition, Action callback, bool expectedResult = true)
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CWaitForCondition_003Ed__5))]
		private static IEnumerator WaitForCondition(Func<bool> condition, Action callback, bool expectedResult)
		{
			return null;
		}

		public static MonoBehaviour RepeatExecutionWhile(this MonoBehaviour monoBehaviour, Func<bool> condition, float interval, Action callback, bool expectedResult = true)
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CRepeatWhileCoroutine_003Ed__7))]
		private static IEnumerator RepeatWhileCoroutine(Func<bool> condition, float interval, Action callback, bool expectedResult)
		{
			return null;
		}

		public static T GetOrAddComponent<T>(this MonoBehaviour behaviour) where T : Component
		{
			return null;
		}

		public static void AddComponentIfMissing<T>(this MonoBehaviour behaviour) where T : Component
		{
		}
	}
}
