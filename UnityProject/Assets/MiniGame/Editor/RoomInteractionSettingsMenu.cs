using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    [InitializeOnLoad]
    public static class RoomInteractionSettingsMenu
    {
        public const string AssetPath = "Assets/MiniGame/Resources/MiniGame/RoomInteractionSettings.asset";
        private const string TogglePath = "MiniGame/全局交互/监督员对话后锁门";

        static RoomInteractionSettingsMenu()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.delayCall += RefreshCheckmark;
        }

        [MenuItem(TogglePath)]
        public static void ToggleDoorLock()
        {
            var settings = GetOrCreateSettings();
            Undo.RecordObject(settings, "切换监督员对话后锁门");
            settings.LockDoorAfterSupervisor = !settings.LockDoorAfterSupervisor;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
            RefreshCheckmark();
        }

        [MenuItem(TogglePath, true)]
        private static bool ValidateDoorLock()
        {
            RefreshCheckmark();
            return true;
        }

        [MenuItem("MiniGame/全局交互/打开配置")]
        public static void OpenSettings()
        {
            var settings = GetOrCreateSettings();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        private static RoomInteractionSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<RoomInteractionSettings>(AssetPath);
            if (settings != null) return settings;
            settings = ScriptableObject.CreateInstance<RoomInteractionSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssetIfDirty(settings);
            return settings;
        }

        private static void RefreshCheckmark()
        {
            var settings = AssetDatabase.LoadAssetAtPath<RoomInteractionSettings>(AssetPath);
            Menu.SetChecked(TogglePath, settings == null || settings.LockDoorAfterSupervisor);
        }

        private static void OnUndoRedo()
        {
            var settings = AssetDatabase.LoadAssetAtPath<RoomInteractionSettings>(AssetPath);
            if (settings != null) AssetDatabase.SaveAssetIfDirty(settings);
            RefreshCheckmark();
        }
    }
}
