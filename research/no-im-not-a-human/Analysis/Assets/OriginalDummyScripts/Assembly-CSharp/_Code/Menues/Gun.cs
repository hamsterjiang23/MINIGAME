using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.Sound;
using _Code.Utils;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Menues;

public sealed class Gun : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private Sprite[] _loadSprites;

	[SerializeField]
	private Sprite[] _shotSprites;

	[SerializeField]
	private float _delayBetweenFrames = 0.05f;

	[SerializeField]
	private float _targetingTimeScale = 1f;

	[SerializeField]
	private float _targetingForce = 1f;

	private bool _isTargeting;

	private readonly Vector3 _hiddenPosition = new Vector3(425f, -650f, 0f);

	private readonly Vector3 _loadedPosition = new Vector3(425f, -250f, 0f);

	[SerializeField]
	[SearchableEnum]
	private ESound[] _pickupSounds;

	private INotAHumanSoundService _soundService;

	private void Update()
	{
		if (_isTargeting)
		{
			float time = Time.time;
			_image.transform.localPosition = _loadedPosition + new Vector3(Mathf.Sin(time * _targetingTimeScale) * _targetingForce, Mathf.Cos(time * _targetingTimeScale) * _targetingForce);
		}
	}

	public async UniTaskVoid LoadGun()
	{
		_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _pickupSounds.GetRandomElement());
		_soundService.Play(ESoundSource.s1_GameScene_2DMusicSourceExtra, ESound.Gun_music);
		_soundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceExtra, 1f, 0.5f);
		_soundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceDay, 0.1f, 1f);
		_image.gameObject.SetActive(value: true);
		_image.transform.DOLocalMove(_loadedPosition, 0.15f);
		Sprite[] loadSprites = _loadSprites;
		foreach (Sprite sprite in loadSprites)
		{
			_image.sprite = sprite;
			await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenFrames));
		}
		Vibrator.StartVibrate(0.1f, 0f);
		_isTargeting = true;
	}

	public async UniTaskVoid Shot()
	{
		_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Player_gun_shoot_2);
		Vibrator.VibrateForTime(0.33f, 0.6f, 1f).Forget();
		_isTargeting = false;
		Sprite[] shotSprites = _shotSprites;
		foreach (Sprite sprite in shotSprites)
		{
			_image.sprite = sprite;
			await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenFrames));
		}
		HideGun().Forget();
	}

	public async UniTaskVoid FakeShot()
	{
		_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Player_gun_shoot_2);
		Vibrator.VibrateForTime(0.33f, 0.6f, 1f).Forget();
		Sprite[] shotSprites = _shotSprites;
		foreach (Sprite sprite in shotSprites)
		{
			_image.sprite = sprite;
			await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenFrames));
		}
	}

	public async UniTaskVoid HideGun()
	{
		_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _pickupSounds.GetRandomElement());
		_soundService.FadeOut(ESoundSource.s1_GameScene_2DMusicSourceExtra, 1f);
		_soundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceDay, 1f, 1f);
		await (Tween)_image.transform.DOLocalMove(_hiddenPosition, 0.15f);
		_image.gameObject.SetActive(value: false);
		Vibrator.StopVibrate();
	}

	public void InitModules(INotAHumanSoundService soundService)
	{
		_soundService = soundService;
	}
}
