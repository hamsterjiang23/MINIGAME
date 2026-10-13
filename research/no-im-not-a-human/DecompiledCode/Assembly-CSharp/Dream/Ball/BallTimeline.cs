using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dream.Ball;

public sealed class BallTimeline : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _timer;

	[SerializeField]
	private Light[] _lights;

	[SerializeField]
	private Image _fade;

	[SerializeField]
	private CoupleAnimation[] _couples;

	[SerializeField]
	private Transform _farWoman;

	[SerializeField]
	private Image _farWomanImage;

	[SerializeField]
	private Image _myWoman;

	[SerializeField]
	private CameraRotate _cameraRotate;

	[SerializeField]
	private Sprite _superSprite;

	private bool _isFadeOff;

	private bool _areLightsOn;

	private bool _areCouplesCreated;

	private bool _areCouplesWentToPositions;

	private bool _isFarWomanCreated;

	private bool _isFarWomanWentToMe;

	private bool _isFlashedToGetWoman;

	private bool _isStartedDance;

	private bool _areLightsOut;

	private bool _isWomanChangedToSuper;

	private float _startTime;

	private void Start()
	{
		_startTime = Time.time;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		_timer.text = Time.time.ToString("00.00");
		if (Time.time - _startTime > 3f && !_isFadeOff)
		{
			FadeOff();
		}
		if (Time.time - _startTime > 6f && !_areLightsOn)
		{
			LightsOn();
		}
		if (Time.time - _startTime > 15.5f && !_areCouplesCreated)
		{
			CreateCouples();
		}
		if (Time.time - _startTime > 22f && !_areCouplesWentToPositions)
		{
			CouplesToPositions();
		}
		if (Time.time - _startTime > 22f && !_isFarWomanCreated)
		{
			CreateFarWoman();
		}
		if (Time.time - _startTime > 25f && !_isFarWomanWentToMe)
		{
			FarWomanToMe();
		}
		if (Time.time - _startTime > 34f && !_isFlashedToGetWoman)
		{
			FlashToGetWoman();
		}
		if (Time.time - _startTime > 34.5f && !_isStartedDance)
		{
			StartDance();
		}
		if (Time.time - _startTime > 60f && !_areLightsOut)
		{
			LightsOut().Forget();
		}
		if (Time.time - _startTime > 69.5f && !_isWomanChangedToSuper)
		{
			ChangeWomanToSuper().Forget();
		}
	}

	private async UniTaskVoid ChangeWomanToSuper()
	{
		_isWomanChangedToSuper = true;
		_myWoman.sprite = _superSprite;
		CoupleAnimation[] couples = _couples;
		for (int i = 0; i < couples.Length; i++)
		{
			couples[i].SetDanceSpeed(5f);
		}
		_cameraRotate.SetDanceSpeed(5f);
		Light[] lights = _lights;
		for (int i = 0; i < lights.Length; i++)
		{
			lights[i].DOIntensity(7f, 0.5f).SetEase(Ease.InSine);
		}
		_fade.DOFade(0f, 0.5f).SetEase(Ease.InSine);
	}

	private async UniTaskVoid LightsOut()
	{
		_areLightsOut = true;
		Light[] lights = _lights;
		for (int i = 0; i < lights.Length; i++)
		{
			lights[i].DOIntensity(0f, 8f).SetEase(Ease.InSine);
		}
		await UniTask.Delay(TimeSpan.FromSeconds(5.0));
		_fade.DOFade(1f, 4.5f).SetEase(Ease.InSine);
	}

	private void StartDance()
	{
		_isStartedDance = true;
		CoupleAnimation[] couples = _couples;
		for (int i = 0; i < couples.Length; i++)
		{
			couples[i].StartDance();
		}
		_cameraRotate.StartDance();
	}

	private void FlashToGetWoman()
	{
		_isFlashedToGetWoman = true;
		Sequence s = DOTween.Sequence();
		s.Append(_fade.DOFade(1f, 0.2f).SetEase(Ease.OutSine));
		s.AppendCallback(() =>
		{
			_farWoman.gameObject.SetActive(value: false);
			_myWoman.gameObject.SetActive(value: true);
		});
		s.Append(_fade.DOFade(0f, 0.2f).SetEase(Ease.InSine));
	}

	private void FarWomanToMe()
	{
		_isFarWomanWentToMe = true;
		_farWoman.DOMoveZ(3.3f, 7f).SetEase(Ease.InSine);
	}

	private void CreateFarWoman()
	{
		_isFarWomanCreated = true;
		_farWomanImage.DOFade(1f, 3f).SetEase(Ease.InSine);
	}

	private void CouplesToPositions()
	{
		_areCouplesWentToPositions = true;
		CoupleAnimation[] couples = _couples;
		for (int i = 0; i < couples.Length; i++)
		{
			couples[i].GoToDancePosition(10f);
		}
	}

	private void CreateCouples()
	{
		_areCouplesCreated = true;
		CoupleAnimation[] couples = _couples;
		for (int i = 0; i < couples.Length; i++)
		{
			couples[i].Create(3f);
		}
	}

	private void LightsOn()
	{
		_areLightsOn = true;
		Light[] lights = _lights;
		for (int i = 0; i < lights.Length; i++)
		{
			lights[i].DOIntensity(7f, 6f).SetEase(Ease.InSine);
		}
	}

	private void FadeOff()
	{
		_isFadeOff = true;
		_fade.DOFade(0f, 6f).SetEase(Ease.InSine);
	}
}
