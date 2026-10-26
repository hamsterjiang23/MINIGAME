using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Shared pullback and turn; ordinary shutdown keeps its initial hold.</summary>
    public static class TerminalReturnMotion
    {
        public const float Seconds = 1.2f;
        public static float Progress(float elapsedFraction, bool delayStart = true)
        {
            float t = Mathf.InverseLerp(delayStart ? .28f : 0, 1, elapsedFraction);
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        public static Vector3 Position(Vector3 source, Quaternion sourceRotation,
            Vector3 destination, Quaternion targetRotation, float progress, bool delayStart = true)
        {
            float t = Progress(progress, delayStart), inverse = 1 - t;
            var a = source - sourceRotation * Vector3.forward * .12f;
            var b = destination - targetRotation * Vector3.forward * .15f + Vector3.up * .025f;
            return inverse * inverse * inverse * source + 3 * inverse * inverse * t * a
                + 3 * inverse * t * t * b + t * t * t * destination;
        }

        public static void Apply(Camera camera, Vector3 source, Quaternion sourceRotation, float sourceFov,
            Vector3 destination, Quaternion targetRotation, float targetFov, float progress, bool delayStart = true)
        {
            float t = Progress(progress, delayStart);
            camera.transform.SetPositionAndRotation(Position(source, sourceRotation, destination, targetRotation, progress, delayStart),
                Quaternion.Slerp(sourceRotation, targetRotation, t));
            camera.fieldOfView = Mathf.Lerp(sourceFov, targetFov, t);
        }
    }
}
