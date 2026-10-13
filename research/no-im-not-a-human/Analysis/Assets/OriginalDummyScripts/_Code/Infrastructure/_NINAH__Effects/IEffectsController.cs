using UnityEngine;
using UnityEngine.Rendering;

namespace _Code.Infrastructure._NINAH__Effects
{
	public interface IEffectsController
	{
		void ChangeVolumeProfile(VolumeProfile volumeProfile);

		void ChangeSkybox(Material skybox);

		void SetVolumeActive(bool isActive);

		void EnableFog(float startDistance = 0f, float endDistance = 0f, Color color = default(Color));

		void DisableFog();

		void EnableSmoke3D();

		void DisableSmoke3D();
	}
}
