using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoupleAnimation : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private Transform _dancePosition;

	private Vector3 _startRotation;

	private bool _isDancing;

	private float _danceSpeed = 2f;

	private void Awake()
	{
		_startRotation = transform.eulerAngles + new Vector3(0f, 90f, 0f);
	}

	public void GoToDancePosition(float time)
	{
		transform.DOMove(_dancePosition.position, time).SetEase(Ease.InSine);
		transform.DORotate(new Vector3(0f, transform.eulerAngles.y + 90f, 0f), time).SetEase(Ease.InSine);
	}

	private void Update()
	{
		if (_isDancing)
		{
			transform.position = _dancePosition.position + new Vector3(Mathf.Sin(Time.time * _danceSpeed) * 0.1f, 0f, Mathf.Cos(Time.time * _danceSpeed) * 0.1f);
			transform.eulerAngles = _startRotation + new Vector3(0f, Mathf.Cos(Time.time * _danceSpeed) * 10f, Mathf.Sin(Time.time * _danceSpeed) * 5f);
		}
	}

	public void Create(float time)
	{
		_image.DOFade(1f, time).SetEase(Ease.InSine);
	}

	public void StartDance()
	{
		_isDancing = true;
	}

	public void SetDanceSpeed(float speed)
	{
		_danceSpeed = speed;
	}
}
