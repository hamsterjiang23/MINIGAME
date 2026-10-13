using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Player;

public interface IPlayerService
{
	IUpdateable Updateable { get; }

	bool IsMouseEnabled { get; set; }

	bool IsInRoom { get; }

	Vector3 LookDirection { get; }

	PlayerSigns Signs { get; }

	bool IsCrouched { set; }

	Vector3 Position { get; }

	void MakeMovable();

	void MakeImmovable();

	UniTask LookAtWithZoom(Transform transformPosition, float fov, float fovChangeSlowerCoefIn);

	UniTask MoveXZ(Vector3 standingPosPosition, float moveToStandingPosSpeed);

	void ResetFov(float fovChangeSlowerCoefOut);

	void TeleportTo(StartPoint newLocationStartPoint);

	void SetRunAvailability(bool isAvailable);
}
