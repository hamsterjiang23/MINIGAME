using UnityEngine;
using UnityEngine.Rendering;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure._NINAH__Effects
{
	public sealed class EffectsController : ASavableClass<EffectsSaveData>, IEffectsController
	{
		private EffectsSaveData _saveData;

		private readonly Volume _volume;

		private readonly Camera _camera;

		private readonly GameObject _smoke3d;

		private readonly IDataModelService _dataModelService;

		public EffectsController(IEffectsViewProvider effectsViewProvider, IDataModelService dataModelService)
		{
		}

		public void ChangeVolumeProfile(VolumeProfile volumeProfile)
		{
		}

		public void ChangeSkybox(Material skybox)
		{
		}

		public void EnableFog(float startDistance = 0f, float endDistance = 0f, Color color = default(Color))
		{
		}

		public void DisableFog()
		{
		}

		public void EnableSmoke3D()
		{
		}

		public void DisableSmoke3D()
		{
		}

		public void SetVolumeActive(bool isActive)
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
