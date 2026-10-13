using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.Sound;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Menues.HUD;

public sealed class ScreamersView : MonoBehaviour
{
	[SerializeField]
	private RawImage _screamer;

	[SerializeField]
	private AnimatedImage _bg;

	[SerializeField]
	private Texture[] _screamersSprites;

	[SerializeField]
	private Material _screamerMaterial;

	[SerializeField]
	[SearchableEnum]
	private ESound[] _sounds;

	private INotAHumanSoundService _soundService;

	public void Init(INotAHumanSoundService soundService)
	{
		_soundService = soundService;
		_screamer.material.SetFloat("_Alpha", 0f);
	}

	public async UniTaskVoid ShowScreamer()
	{
		await UniTask.Delay(TimeSpan.FromSeconds(0.800000011920929));
		_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, _sounds.GetRandomElementWithoutRepeating());
		await SetInitialState();
		await AnimateEntry();
		await AnimateGlitchLoop(0.3f, 5);
		await AnimateExit();
	}

	private async Task SetInitialState()
	{
		_screamer.material = new Material(_screamerMaterial);
		_screamer.material.SetFloat("_FaceOpacity", 0f);
		_screamer.material.SetFloat("_BackgroundOpacity", 0f);
		_screamer.material.SetFloat("_ShapeDistortAmount", 1f);
		_screamer.material.SetFloat("_VignettePower", 1f);
		_screamer.material.SetFloat("_FaceSize", 0.5f);
		_screamer.material.SetVector("_FacePosition", new Vector4(0.5f, 0.5f, 0f, 0f));
		_screamer.material.SetFloat("_FadeInVignettePower", 100f);
		_screamer.material.SetTexture("_MainTex", _screamersSprites.GetRandomElementWithoutRepeating());
		await UniTask.DelayFrame(1);
	}

	private async Task AnimateEntry()
	{
		_screamer.material.DOFloat(1f, "_Alpha", 0.1f).SetEase(Ease.InCubic);
		_bg.StartAnimationOrSetFirstFrame();
		_screamer.material.DOFloat(0.2f, "_BackgroundOpacity", 0.2f).SetEase(Ease.InCubic);
		_screamer.material.DOFloat(5f, "_VignettePower", 1f);
		_screamer.material.DOFloat(-100f, "_FadeInVignettePower", 0.3f);
		await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224));
		_screamer.material.DOFloat(0.5f, "_FaceOpacity", 0.2f).SetEase(Ease.InFlash);
		await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
	}

	private async Task AnimateGlitchLoop(float durationInSeconds, int loopCount)
	{
		float[] array = new float[loopCount];
		float num = 0f;
		for (int i = 0; i < loopCount; i++)
		{
			array[i] = UnityEngine.Random.Range(0.1f, 1f);
			num += array[i];
		}
		float[] delays = new float[loopCount];
		for (int j = 0; j < loopCount; j++)
		{
			delays[j] = array[j] / num * durationInSeconds;
		}
		for (int k = 0; k < loopCount; k++)
		{
			_screamer.material.SetFloat("_ShapeDistortAmount", UnityEngine.Random.Range(-1.5f, 1.5f));
			_screamer.material.SetFloat("_ColorEdge", UnityEngine.Random.Range(0f, 1f));
			_screamer.material.SetFloat("_EdgeSaturation", UnityEngine.Random.Range(-10f, 10f));
			_screamer.material.SetFloat("_ColorDistortAmount", UnityEngine.Random.Range(-1f, 1f));
			_screamer.material.SetFloat("_FaceSize", UnityEngine.Random.Range(0.3f, 0.5f));
			_screamer.material.SetVector("_FacePosition", new Vector4(UnityEngine.Random.Range(0.2f, 0.8f), UnityEngine.Random.Range(0.3f, 0.5f), 0f, 0f));
			await UniTask.Delay(TimeSpan.FromSeconds(delays[k]));
		}
	}

	private async Task AnimateExit()
	{
		_screamer.material.DOFloat(0.15f, "_ShapeDistortAmount", 0.1f);
		_screamer.material.DOFloat(0.15f, "_ColorEdge", 0.1f);
		_screamer.material.DOFloat(-4.5f, "_EdgeSaturation", 0.1f);
		_screamer.material.DOFloat(-0.02f, "_ColorDistortAmount", 0.02f);
		_screamer.material.SetFloat("_FaceSize", 0.8f);
		_screamer.material.DOVector(new Vector4(0.5f, 0.5f, 0f, 0f), "_FacePosition", 0.1f);
		await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
		_bg.Image.DOFade(0f, 0.1f).SetEase(Ease.InCirc);
		_screamer.material.DOFloat(0f, "_Alpha", 0.1f);
	}
}
