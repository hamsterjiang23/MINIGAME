using System;
using System.Linq;
using MiniGame.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    public static class CombinedInteractionSetup
    {
        [MenuItem("MiniGame/Main Room/Update door camera and interaction focus")]
        public static void InstallInOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before updating scene components.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != MainRoomGameplayBuilder.ScenePath) throw new InvalidOperationException("Open MainRoomCombined first.");
            var player = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ApartmentPlayer>(true)).Single();
            var door = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<HingedDoor>(true)).Single();
            ConfigureDoorCamera(player.ViewCamera, player, door);
            var computer = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true))
                .Single(item => item.Target == InteractionTarget.Computer);
            var monitor = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MeshRenderer>(true))
                .Single(item => item.name.StartsWith("CRT_Monitor_Tripo", StringComparison.Ordinal));
            computer.Configure(computer.Target, computer.Label, monitor, computer.InteractionDistance);
            EditorUtility.SetDirty(computer);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        public static void ConfigureDoorCamera(Camera camera, ApartmentPlayer player, HingedDoor door)
        {
            var root = door.transform.parent;
            var scene = root.gameObject.scene;
            var transforms = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<Transform>(true)).ToArray();
            var centreMarker = transforms.Single(item => item.name == "Unity_DoorCentre");
            var origin = transforms.Single(item => item.name == "Unity_ReferenceOrigin");
            var right = transforms.Single(item => item.name == "Unity_ReferenceRight");
            var outward = (right.position - origin.position).normalized;
            var eye = centreMarker.position; eye.y = camera.transform.position.y;
            var latchSide = Vector3.ProjectOnPlane(centreMarker.position - door.transform.position, Vector3.up);
            latchSide = (latchSide - outward * Vector3.Dot(latchSide, outward)).normalized;
            var pathCentre = eye + latchSide * .35f;
            var observationRotation = Quaternion.LookRotation(outward);
            var inside = Pose(root, "Door camera alignment", pathCentre - outward * .85f, observationRotation);
            var outside = Pose(root, "Door camera outside", pathCentre + outward * 1.15f, observationRotation);
            var controller = camera.GetComponent<DoorViewCamera>();
            if (controller == null) controller = camera.gameObject.AddComponent<DoorViewCamera>();
            controller.Configure(door, player, inside, outside);
            EditorUtility.SetDirty(controller);
        }

        private static Transform Pose(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing;
            var target = new GameObject(name).transform;
            target.SetParent(parent, false);
            target.SetPositionAndRotation(position, rotation);
            return target;
        }
    }
}
