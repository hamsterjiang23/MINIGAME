using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using _Code.Characters;

namespace _Code.DialogSystem;

public sealed class DialogSignElementViewEye : DialogSignElementViewFrameAnimatedCover
{
	[SerializeField]
	private Image _white;

	[SerializeField]
	private Image _iris;

	private bool _isEnabled;

	private CharacterEyeData _eyeData;

	private const float IrisMoveXRadius = 150f;

	private const float IrisMoveYRadius = 100f;

	public void Init(CharacterEyeData eyeData)
	{
		_eyeData = eyeData;
		_white.sprite = eyeData.White;
		_iris.sprite = eyeData.Iris;
		MoveIris().Forget();
	}

	private void OnEnable()
	{
		_isEnabled = true;
	}

	private void OnDisable()
	{
		_isEnabled = false;
	}

	private async UniTask MoveIris()
	{
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		insideUnitCircle *= new Vector2(150f, 100f) * _eyeData.DistanceMultiplier;
		float duration = Random.Range(_eyeData.MinMoveDuration, _eyeData.MaxMoveDuration);
		await (Tween)_iris.transform.DOLocalMove(insideUnitCircle, duration).SetEase(_eyeData.Easing);
		if (_isEnabled)
		{
			MoveIris().Forget();
		}
	}
}
