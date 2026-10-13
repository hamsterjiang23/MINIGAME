using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Helper
{
	[CompilerGenerated]
	private sealed class _003CAnimateStringText_003Ed__6 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string text;

		public TMP_Text animatedText;

		public float secsPerLetter;

		private string[] _003Cwords_003E5__2;

		private int _003Ci_003E5__3;

		private string _003Cword_003E5__4;

		private string _003CpreviousText_003E5__5;

		private int _003Cj_003E5__6;

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
		public _003CAnimateStringText_003Ed__6(int _003C_003E1__state)
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
	private sealed class _003CCallActionAfterFade_003Ed__5 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool fadeIn;

		public float fadeTimeInSec;

		public SpriteRenderer spriteRenderer;

		public Action action;

		private float _003Cstart_003E5__2;

		private float _003Cend_003E5__3;

		private float _003ClerpAmount_003E5__4;

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
		public _003CCallActionAfterFade_003Ed__5(int _003C_003E1__state)
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
	private sealed class _003CFadeCanvasGroup_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool fadeIn;

		public CanvasGroup canvasGroup;

		public float fadeTimeInSec;

		private float _003Cstart_003E5__2;

		private float _003Cend_003E5__3;

		private float _003ClerpAmount_003E5__4;

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
		public _003CFadeCanvasGroup_003Ed__2(int _003C_003E1__state)
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
	private sealed class _003CFadeGraphic_003Ed__3 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool fadeIn;

		public float fadeTimeInSec;

		public Graphic graphic;

		private float _003Cstart_003E5__2;

		private float _003Cend_003E5__3;

		private float _003ClerpAmount_003E5__4;

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
		public _003CFadeGraphic_003Ed__3(int _003C_003E1__state)
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
	private sealed class _003CFadeGraphic_003Ed__4 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool fadeIn;

		public float fadeTimeInSec;

		public SpriteRenderer spriteRenderer;

		private float _003Cstart_003E5__2;

		private float _003Cend_003E5__3;

		private float _003ClerpAmount_003E5__4;

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
		public _003CFadeGraphic_003Ed__4(int _003C_003E1__state)
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
	private sealed class _003CSelectGameObjectAfterFrame_003Ed__7 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public GameObject selectObj;

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
		public _003CSelectGameObjectAfterFrame_003Ed__7(int _003C_003E1__state)
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
	private sealed class _003CWaitAndCallAction_003Ed__9 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float seconds;

		public Action action;

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
		public _003CWaitAndCallAction_003Ed__9(int _003C_003E1__state)
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

	public static string[] Split(this string line, char tempSeparator, string separator, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
	{
		return null;
	}

	public static string[] Split(this string line, char tempSeparator, string[] separators, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CFadeCanvasGroup_003Ed__2))]
	public static IEnumerator FadeCanvasGroup(bool fadeIn, float fadeTimeInSec, CanvasGroup canvasGroup)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CFadeGraphic_003Ed__3))]
	public static IEnumerator FadeGraphic(bool fadeIn, float fadeTimeInSec, Graphic graphic)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CFadeGraphic_003Ed__4))]
	public static IEnumerator FadeGraphic(bool fadeIn, float fadeTimeInSec, SpriteRenderer spriteRenderer)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CCallActionAfterFade_003Ed__5))]
	public static IEnumerator CallActionAfterFade(bool fadeIn, float fadeTimeInSec, SpriteRenderer spriteRenderer, Action action)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CAnimateStringText_003Ed__6))]
	public static IEnumerator AnimateStringText(TMP_Text animatedText, string text, float secsPerLetter)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CSelectGameObjectAfterFrame_003Ed__7))]
	public static IEnumerator SelectGameObjectAfterFrame(GameObject selectObj)
	{
		return null;
	}

	public static GameObject GetSelectedGameObject()
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CWaitAndCallAction_003Ed__9))]
	public static IEnumerator WaitAndCallAction(float seconds, Action action)
	{
		return null;
	}
}
