using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace _Code.Infrastructure._NINAH__Dream.MushroomDream
{
	public sealed class MushroomDream : MonoBehaviour
	{
		[SerializeField]
		private Transform[] _mushrooms;

		[SerializeField]
		private Material _wallMaterial;

		[SerializeField]
		private VolumeProfile _volumeProfile;

		[SerializeField]
		private CinemachineCamera _camera;

		[SerializeField]
		private CinemachineBasicMultiChannelPerlin _shake;

		private void Start()
		{
			GrowMushrooms().Forget();
			SpinTunnel().Forget();
			ExpandVignette().Forget();
			ChangeFov().Forget();
			LowerShake().Forget();
			CloseScene().Forget();
		}

		private async UniTaskVoid CloseScene()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(20.0));
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			SceneManager.UnloadSceneAsync(6);
		}

		private async UniTask LowerShake()
		{
			_shake.AmplitudeGain = 1.25f;
			await Awaitable.WaitForSecondsAsync(2.5f);
			DOTween.To(() => _shake.AmplitudeGain, (float x) =>
			{
				_shake.AmplitudeGain = x;
			}, 0f, 17.5f).SetEase(Ease.InCubic);
		}

		private async UniTask ChangeFov()
		{
			_camera.Lens.FieldOfView = 90f;
			await Awaitable.WaitForSecondsAsync(2.5f);
			DOTween.To(() => _camera.Lens.FieldOfView, (float x) =>
			{
				_camera.Lens.FieldOfView = x;
			}, 40f, 17.5f).SetEase(Ease.InCubic);
		}

		private async UniTask ExpandVignette()
		{
			_volumeProfile.TryGet<Vignette>(out var vignette);
			vignette.intensity.value = 0f;
			await UniTask.Delay(TimeSpan.FromSeconds(5.0));
			DOTween.To(() => vignette.intensity.value, (float x) =>
			{
				vignette.intensity.value = x;
			}, 1f, 17.5f).SetEase(Ease.InCubic);
		}

		private async UniTaskVoid SpinTunnel()
		{
			_wallMaterial.SetFloat("_Speed", 0f);
			await UniTask.Delay(TimeSpan.FromSeconds(5.0));
			DOTween.To(() => _wallMaterial.GetFloat("_Speed"), (float x) =>
			{
				_wallMaterial.SetFloat("_Speed", x);
			}, 0.01f, 15f).SetEase(Ease.InQuint);
		}

		private async UniTaskVoid GrowMushrooms()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(5.0));
			int i = 0;
			Transform[] mushrooms = _mushrooms;
			for (int j = 0; j < mushrooms.Length; j++)
			{
				mushrooms[j].DOScale(20f, 15f - (float)i * 0.1f).SetEase(Ease.InCubic);
				await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
				i++;
			}
		}
	}
}
