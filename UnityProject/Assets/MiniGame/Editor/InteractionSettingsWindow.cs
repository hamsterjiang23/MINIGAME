using System;
using System.Linq;
using MiniGame.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    public sealed class InteractionSettingsWindow : EditorWindow
    {
        private Component[] targets = Array.Empty<Component>();
        private Scene mainScene;
        private Vector2 scroll;

        [MenuItem("MiniGame/交互设置")]
        public static void Open() => GetWindow<InteractionSettingsWindow>("交互设置").Show();

        private void OnEnable()
        {
            minSize = new Vector2(420, 260);
            EditorApplication.hierarchyChanged += RefreshTargets;
            Selection.selectionChanged += RefreshTargets;
            Undo.undoRedoPerformed += RefreshTargets;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            RefreshTargets();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= RefreshTargets;
            Selection.selectionChanged -= RefreshTargets;
            Undo.undoRedoPerformed -= RefreshTargets;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode) => RefreshTargets();
        private void OnSceneClosed(Scene scene) => RefreshTargets();
        private void OnSceneSaved(Scene scene) => RefreshTargets();
        private void OnActiveSceneChanged(Scene previous, Scene next) => RefreshTargets();
        private void OnPlayModeChanged(PlayModeStateChange state) => RefreshTargets();

        private void RefreshTargets()
        {
            mainScene = ResolveLoadedScene(MainRoomGameplayBuilder.ScenePath);
            targets = GetTargets(mainScene);
            Repaint();
        }

        public static Scene ResolveLoadedScene(string path)
        {
            if (string.IsNullOrEmpty(path)) return default;
            var scene = SceneManager.GetSceneByPath(path);
            return scene.IsValid() && scene.isLoaded && !EditorSceneManager.IsPreviewScene(scene)
                && string.Equals(scene.path, path, StringComparison.Ordinal) ? scene : default;
        }

        private Scene ResolveMainScene()
        {
            var resolved = ResolveLoadedScene(MainRoomGameplayBuilder.ScenePath);
            if (mainScene != resolved)
            {
                mainScene = resolved;
                targets = GetTargets(resolved);
            }
            return resolved;
        }

        public static Component[] GetTargets(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || EditorSceneManager.IsPreviewScene(scene))
                return Array.Empty<Component>();
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldInteraction>(true)
                    .Cast<Component>().Concat(root.GetComponentsInChildren<ReadableProp>(true)))
                .OrderBy(TypeName, StringComparer.Ordinal).ThenBy(item => item.name, StringComparer.Ordinal).ToArray();
        }

        public static bool CanEditScene(Scene scene) => !EditorApplication.isPlayingOrWillChangePlaymode
            && scene.IsValid() && scene.isLoaded && !EditorSceneManager.IsPreviewScene(scene);

        public static bool ApplyDistance(SerializedObject serialized, Scene scene)
        {
            if (!CanEditScene(scene) || serialized == null || serialized.targetObjects.Length != 1
                || !(serialized.targetObject is Component component) || component.gameObject.scene != scene
                || EditorUtility.IsPersistent(component)
                || !(component is WorldInteraction || component is ReadableProp)) return false;
            if (!serialized.ApplyModifiedProperties()) return false;
            EditorSceneManager.MarkSceneDirty(scene);
            return true;
        }

        private void OnGUI()
        {
            ResolveMainScene();
            EditorGUILayout.LabelField("MainRoomCombined · 交互距离", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("距离从玩家镜头量到瞄准命中的物体表面。门、床、电脑和海报分别配置，包含未激活物体。", MessageType.Info);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("播放中的场景修改退出后会丢失。请退出播放后编辑和保存；此面板不会保存运行时对象。", MessageType.Warning);
                if (GUILayout.Button("退出播放后编辑")) EditorApplication.isPlaying = false;
            }

            if (!mainScene.IsValid() || !mainScene.isLoaded)
            {
                EditorGUILayout.HelpBox("尚未打开 MainRoomCombined 场景。", MessageType.Info);
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    if (GUILayout.Button("打开 Combined"))
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            EditorSceneManager.OpenScene(MainRoomGameplayBuilder.ScenePath, OpenSceneMode.Single);
                    }
                }
                return;
            }

            using (var view = new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = view.scrollPosition;
                if (targets.Length == 0) EditorGUILayout.HelpBox("此场景尚未配置交互物。", MessageType.Info);
                foreach (var component in targets)
                {
                    if (component == null) continue;
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(TypeName(component) + " · " + component.name, EditorStyles.boldLabel);
                            if (GUILayout.Button("定位物体", GUILayout.Width(78)))
                            {
                                Selection.activeGameObject = component.gameObject;
                                EditorGUIUtility.PingObject(component.gameObject);
                            }
                        }
                        using (new EditorGUI.DisabledScope(!CanEditScene(ResolveMainScene())))
                        using (var serialized = new SerializedObject(component))
                        {
                            serialized.Update();
                            EditorGUILayout.PropertyField(serialized.FindProperty("interactionDistance"),
                                new GUIContent("交互距离（米）", "超出此距离将不显示悬停反馈，也不能触发交互。"));
                            ApplyDistance(serialized, ResolveMainScene());
                        }
                    }
                }
            }
            var saveScene = ResolveMainScene();
            if (!saveScene.IsValid() || !saveScene.isLoaded) return;
            EditorGUILayout.LabelField(saveScene.isDirty ? "场景有未保存修改" : "场景已保存", EditorStyles.miniLabel);
            using (new EditorGUI.DisabledScope(!CanEditScene(saveScene) || !saveScene.isDirty))
                if (GUILayout.Button("保存场景"))
                {
                    saveScene = ResolveMainScene();
                    if (CanEditScene(saveScene) && EditorSceneManager.SaveScene(saveScene))
                        ShowNotification(new GUIContent("已保存 MainRoomCombined"));
                }
        }

        private static string TypeName(Component component)
        {
            if (component is ReadableProp) return "海报";
            if (!(component is WorldInteraction interaction)) return "交互物";
            switch (interaction.Target)
            {
                case InteractionTarget.Door: return "门";
                case InteractionTarget.Bed: return "床";
                case InteractionTarget.Computer: return "电脑";
                default: return "交互物";
            }
        }
    }
}
