using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.TriggerObjects;
using _Code.Player;

namespace _Code.Infrastructure._NINAH__TriggerObjects.Objects;

public sealed class TriggerObjectFollowLight : ATriggerObject
{
	[SerializeField]
	private Light _lightTarget;

	[SerializeField]
	private Light _lightTarget2;

	[SerializeField]
	private float _targetIntensity;

	[SerializeField]
	private Vector3 _bias;

	private bool _isEntered;

	private PlayerController _playerTarget;

	protected override void OnEnterInner(Collider other)
	{
		if (other.gameObject.TryGetComponent<PlayerController>(out var component))
		{
			_isEntered = true;
			_lightTarget.DOIntensity(_targetIntensity, 1f);
			_lightTarget2.DOIntensity(_targetIntensity * 2.5f, 1f);
			_playerTarget = component;
		}
	}

	protected override void OnExitInner(Collider other)
	{
		if (other.gameObject.TryGetComponent<PlayerController>(out var _))
		{
			_lightTarget.DOIntensity(0f, 0.2f);
			_lightTarget2.DOIntensity(0f, 0.2f);
			ResetIsEntered().Forget();
		}
	}

	private async UniTask ResetIsEntered()
	{
		await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224));
		_isEntered = false;
	}

	private void Update()
	{
		if (_isEntered)
		{
			_lightTarget.transform.position = _playerTarget.transform.position + _bias;
			_lightTarget.transform.rotation = _playerTarget.RealCamera.transform.rotation;
		}
	}
}
