using DG.Tweening;
using DefaultNamespace.EnumEventBus;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Sound;
using _Scripts.Services.Sound;

namespace _Code.Infrastructure.TriggerObjects.Objects;

public sealed class TriggerObjectOpenDoor : ATriggerObject
{
	[SerializeField]
	private Transform _doorTransform;

	[SerializeField]
	private Vector3 _rotateAngle;

	[SerializeField]
	private float _duration;

	[SerializeField]
	[SearchableEnum]
	private ESound[] _sounds;

	private CommonEnumEventus _commonEventBus;

	private bool _isTriggered;

	protected override void OnEnterInner(Collider other)
	{
		if (!_isTriggered)
		{
			_isTriggered = true;
			_doorTransform.DORotate(_rotateAngle, _duration);
			_commonEventBus.Invoke<ESoundSource, ESound>(ECommonEvents.PlaySoundOneShot, new object[2]
			{
				ESoundSource.s1_GameScene_2DSFXSource,
				_sounds.GetRandomElement()
			});
		}
	}

	public void Init(CommonEnumEventus eventus)
	{
		_commonEventBus = eventus;
	}
}
