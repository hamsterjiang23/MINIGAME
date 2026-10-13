using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure._NINAH__Effects;

public sealed class EffectsController : ASavableClass<EffectsSaveData>, IEffectsController
{
	private EffectsSaveData _saveData = new EffectsSaveData();

	private readonly Volume _volume;

	private readonly Camera _camera;

	private readonly GameObject _smoke3d;

	private readonly IDataModelService _dataModelService;

	public EffectsController(IEffectsViewProvider effectsViewProvider, IDataModelService dataModelService)
	{
		_dataModelService = dataModelService;
		_volume = effectsViewProvider.Volume;
		_camera = effectsViewProvider.Camera;
		_smoke3d = effectsViewProvider.RoomSmoke3D;
		effectsViewProvider.NightVolumeProfile.TryGet<LensDistortion>(out var component);
		component.intensity.value = 0f;
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}

	public void ChangeVolumeProfile(VolumeProfile volumeProfile)
	{
		_volume.profile = volumeProfile;
	}

	public void ChangeSkybox(Material skybox)
	{
		RenderSettings.skybox = skybox;
		_camera.clearFlags = CameraClearFlags.Skybox;
	}

	public void EnableFog(float startDistance = 0f, float endDistance = 0f, Color color = default(Color))
	{
		RenderSettings.fog = true;
		if (startDistance != 0f)
		{
			RenderSettings.fogStartDistance = startDistance;
		}
		if (endDistance != 0f)
		{
			RenderSettings.fogEndDistance = endDistance;
		}
		if (color != default(Color))
		{
			RenderSettings.fogColor = color;
		}
	}

	public void DisableFog()
	{
		RenderSettings.fog = false;
	}

	public void EnableSmoke3D()
	{
		_smoke3d.SetActive(value: true);
		_saveData.IsSmokeEnabled = true;
	}

	public void DisableSmoke3D()
	{
		_smoke3d.SetActive(value: false);
		_saveData.IsSmokeEnabled = false;
	}

	public void SetVolumeActive(bool isActive)
	{
		_volume.enabled = isActive;
	}

	protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
	{
		EffectsSaveData effectsSaveData = saver.LoadKey(this);
		if (effectsSaveData == null)
		{
			effectsSaveData = new EffectsSaveData();
		}
		_saveData = effectsSaveData;
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}
}
