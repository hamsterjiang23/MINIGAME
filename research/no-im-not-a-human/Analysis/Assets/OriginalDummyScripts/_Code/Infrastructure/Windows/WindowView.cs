using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.DialogSystem;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Sound;
using _Code.Rooms;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Windows
{
	public sealed class WindowView : MonoBehaviour, IWindowView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CBodyEaterAchievement_003Ed__48 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

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
		private int _windowIndex;

		[SerializeField]
		private AnimatedImage[] _images;

		[SerializeField]
		private ESoundSource _audioSource;

		[SerializeField]
		private MeshRenderer[] _beams;

		[SerializeField]
		private Material _windowMaterial;

		private IDayNightController _dayNightController;

		private IDialogManager _dialogManager;

		private INotAHumanSoundService _soundService;

		private IDataModelService _dataModelService;

		private WindowDayImageData[] _imagesByDays;

		private WindowsSOData _windowSOData;

		[CompilerGenerated]
		private Action<WindowView> m_Opened;

		[CompilerGenerated]
		private Action<WindowView> m_Closed;

		[CompilerGenerated]
		private Action<WindowView> m_StartedOpening;

		private AActionableObjectView _linkedActionableObject;

		private bool _hasWatchedMonologueToday;

		private int _imagesTonight;

		private float _lightIntensity;

		[field: SerializeField]
		public Camera LinkedCamera { get; private set; }

		[field: SerializeField]
		public float CameraDistance { get; private set; }

		[field: SerializeField]
		public float DialogCameraDistance { get; private set; }

		private bool HasDialogNow => false;

		public bool CanLeave => false;

		public Vector3 LookDirection => default;

		public event Action<WindowView> Opened
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

		public event Action<WindowView> Closed
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

		public event Action<WindowView> StartedOpening
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

		public void Init(IDayNightController dayNightController, IDialogManager dialogManager, IWindowsSODataProvider dataProvider, INotAHumanSoundService soundService, IDataModelService dataModelService)
		{
		}

		public void InitImages(int day)
		{
		}

		public void StartOpen()
		{
		}

		public void FinishOpen()
		{
		}

		public void Close()
		{
		}

		private void ShowDialog()
		{
		}

		[AsyncStateMachine(typeof(_003CBodyEaterAchievement_003Ed__48))]
		private UniTask BodyEaterAchievement()
		{
			return default;
		}

		public void RenewDialogs()
		{
		}

		public void StartOfStartOpening()
		{
		}

		public void LinkActionableObject(AActionableObjectView actionableObject)
		{
		}

		public void ShowView()
		{
		}

		public void EnableSound(ESound sound)
		{
		}

		public void DisableSound()
		{
		}
	}
}
