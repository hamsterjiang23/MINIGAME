using UnityEngine;

namespace MiniGame.UnityHost
{
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    [AddComponentMenu("MiniGame/画面/敲门震动")]
    public sealed class KnockScreenShake : MonoBehaviour
    {
        [SerializeField, Range(0, 3), InspectorName("敲门旋转震幅（度）")]
        private float strength = 1.1f;
        [SerializeField, Range(0, .08f), InspectorName("敲门位移震幅（米）")]
        private float translationStrength = .022f;
        private MiniGameAudioDirector director;
        private AudioClip recording;
        private KnockImpactTrack track;
        private Quaternion unshakenRotation;
        private Vector3 unshakenPosition;
        private bool poseApplied;

        public void Bind(MiniGameAudioDirector audio)
        {
            RestorePose();
            director = audio;
            RefreshTrack();
        }

        private void RefreshTrack()
        {
            var clip = director != null && director.Profile != null
                ? director.Profile.GetClip(MiniGameSound.DoorKnock) : null;
            if (recording == clip && track != null) return;
            recording = clip;
            track = KnockImpactTrack.Analyze(clip);
        }

        public Vector3 CurrentRotationOffset => EvaluateCurrent() * strength;
        public Vector3 CurrentPositionOffset => PositionOffset(EvaluateCurrent());

        private Vector3 EvaluateCurrent()
        {
            if (!isActiveAndEnabled || director == null || !director.isActiveAndEnabled || director.Service == null
                || !director.Service.isActiveAndEnabled) return Vector3.zero;
            RefreshTrack();
            var source = director.Service.GetSource(MiniGameAudioChannel.DoorKnock);
            if (source == null || !source.isPlaying || source.clip != recording) return Vector3.zero;
            return track.Evaluate(source.timeSamples);
        }

        private Vector3 PositionOffset(Vector3 response)
            => new Vector3(response.y, -response.x, 0) * translationStrength;

        private void OnPreCull()
        {
            RestorePose();
            var response = EvaluateCurrent();
            if (response == Vector3.zero) return;
            // Apply only while this camera renders; movement, aiming and save captures keep their own pose.
            unshakenRotation = transform.localRotation;
            unshakenPosition = transform.localPosition;
            transform.localPosition = unshakenPosition + unshakenRotation * PositionOffset(response);
            transform.localRotation = unshakenRotation * Quaternion.Euler(response * strength);
            poseApplied = true;
        }

        private void OnPostRender() => RestorePose();
        private void OnDisable() => RestorePose();
        private void RestorePose()
        {
            if (!poseApplied) return;
            transform.localRotation = unshakenRotation;
            transform.localPosition = unshakenPosition;
            poseApplied = false;
        }
    }
}
