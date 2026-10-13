using DefaultNamespace.EnumEventBus;
using ECM2;
using UnityEngine;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Sound;
using _Scripts.Services.Sound;

namespace _Code.Player
{
	public sealed class PlayerBodyeaterstepSoundPlayer : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		private bool _isMovingNow;

		private CommonEnumEventus _commonEnumEventus;

		private void Update()
		{
			if (_isMovingNow)
			{
				if (_character.velocity.magnitude < 0.1f)
				{
					Stop();
				}
			}
			else if (_character.velocity.magnitude >= 0.1f)
			{
				Move();
			}
		}

		private void Move()
		{
			_isMovingNow = true;
			_commonEnumEventus.Invoke<ESoundSource, ESound, float, bool>(ECommonEvents.PlaySound, new object[4]
			{
				ESoundSource.s1_GameScene_2DSFXSourceExtra2,
				ESound.Char_body_eater_move,
				0.5f,
				true
			});
			_commonEnumEventus.Invoke<ESoundSource, float>(ECommonEvents.StopSound, new object[3]
			{
				ESoundSource.s1_GameScene_2DSFXSourceExtra,
				0.5f,
				true
			});
		}

		private void Stop()
		{
			_isMovingNow = false;
			_commonEnumEventus.Invoke<ESoundSource, ESound, float, bool>(ECommonEvents.PlaySound, new object[4]
			{
				ESoundSource.s1_GameScene_2DSFXSourceExtra,
				ESound.Char_body_eater_idle,
				0.5f,
				true
			});
			_commonEnumEventus.Invoke<ESoundSource, float>(ECommonEvents.StopSound, new object[3]
			{
				ESoundSource.s1_GameScene_2DSFXSourceExtra2,
				0.5f,
				true
			});
		}

		public void InitModules(CommonEnumEventus commonEnumEventus)
		{
			_commonEnumEventus = commonEnumEventus;
		}
	}
}
