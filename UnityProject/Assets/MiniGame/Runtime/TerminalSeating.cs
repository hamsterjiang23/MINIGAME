using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Owns the reversible first-person actor pose while the terminal camera leaves the eyes.</summary>
    internal sealed class TerminalSeating
    {
        private readonly Transform actor;
        private readonly Transform camera;
        private readonly CharacterController capsule;
        private Vector3 standingPosition;
        private Quaternion standingRotation;
        private Vector3 standingCenter;
        private float standingHeight;
        private bool controllerWasEnabled;
        private bool captured;
        public bool IsSeated { get; private set; }

        public TerminalSeating(ApartmentPlayer player, Camera viewCamera)
        {
            actor = player.transform;
            camera = viewCamera.transform;
            capsule = player.GetComponent<CharacterController>();
        }

        public void Capture()
        {
            if (captured) return;
            standingPosition = actor.position;
            standingRotation = actor.rotation;
            standingCenter = capsule.center;
            standingHeight = capsule.height;
            controllerWasEnabled = capsule.enabled;
            captured = true;
        }

        public void SetPose(Transform eyePose, float amount)
        {
            Capture();
            var cameraPosition = camera.position;
            var cameraRotation = camera.rotation;
            capsule.enabled = false;
            float p = Mathf.Clamp01(amount);
            var targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(eyePose.forward, Vector3.up), Vector3.up);
            actor.SetPositionAndRotation(Vector3.Lerp(standingPosition, eyePose.position - Vector3.up * 1.15f, p),
                Quaternion.Slerp(standingRotation, targetRotation, p));
            capsule.height = Mathf.Lerp(standingHeight, 1.2f, p);
            capsule.center = Vector3.Lerp(standingCenter, new Vector3(0, .6f, 0), p);
            camera.SetPositionAndRotation(cameraPosition, cameraRotation);
            IsSeated = p >= .999f;
        }

        public void Restore(bool release)
        {
            if (!captured) return;
            var cameraPosition = camera.position;
            var cameraRotation = camera.rotation;
            capsule.enabled = false;
            actor.SetPositionAndRotation(standingPosition, standingRotation);
            capsule.height = standingHeight;
            capsule.center = standingCenter;
            capsule.enabled = controllerWasEnabled;
            camera.SetPositionAndRotation(cameraPosition, cameraRotation);
            IsSeated = false;
            if (release) captured = false;
        }
    }
}
