using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Player;
using _Code.Utils.Extensions;

namespace _Code.Infrastructure.ControlsViewer
{
	public sealed class ControlsListView : MonoBehaviour
	{
		[SerializeField]
		private ControlView[] _controls;

		private int _maxUsedControl;

		private InputHandling _inputHandling;

		public int ActiveCount => _controls.Count((ControlView x) => x.gameObject.activeSelf);

		public void InitModules(IInputHandlerProvider inputHandlerProvider)
		{
			_inputHandling = inputHandlerProvider.InputHandler;
		}

		public async UniTask ClearControls()
		{
			List<UniTask> list = new List<UniTask>();
			_maxUsedControl = 0;
			ControlView[] controls = _controls;
			foreach (ControlView controlView in controls)
			{
				list.Add(controlView.Disable());
			}
			await UniTask.WhenAll(list);
		}

		public void AddControl(EControl control)
		{
			string key = _inputHandling.GetKey(control);
			switch (_inputHandling.CurrentDevice)
			{
			case EInputDevice.MouseAndKeyboard:
				_controls[_maxUsedControl].InitKey(key, control);
				break;
			case EInputDevice.Gaypad:
				_controls[_maxUsedControl].InitGamepadKey(key, control, _inputHandling.GetGaypadType());
				break;
			}
			_controls[_maxUsedControl].SetDescription($"control_key_{control}".GetLocalization("UI"));
			_controls[_maxUsedControl].Enable();
			_maxUsedControl++;
		}

		public void SetControlAvailability(EControl control, bool isAvailable)
		{
			_controls.FirstOrDefault((ControlView x) => x.Control == control)?.SetAvailability(isAvailable);
		}
	}
}
