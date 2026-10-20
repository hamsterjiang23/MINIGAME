using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PMSAmber.Editor
{
    public static class PMSPackageBuilder
    {
        private const string Root="Assets/PMSAmberTerminal";
        [MenuItem("PMS Amber Terminal/Open demo scene")]
        public static void OpenDemo()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(Root+"/Scenes/PMSAmberDemo.unity");
        }
        [MenuItem("PMS Amber Terminal/Create terminal in current scene")]
        public static void CreateTerminal()
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/PMSAmberTerminal.prefab");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);Undo.RegisterCreatedObjectUndo(go,"Create PMS terminal");Selection.activeGameObject=go;
        }
        // Export/validation entry point, invoked in an isolated Unity project, never on the user's working scene.
        public static void BuildAndValidate()
        {
            try
            {
                AssetDatabase.Refresh();ConfigureImports();
                var font=AssetDatabase.LoadAssetAtPath<Font>(Root+"/Fonts/NotoSansMonoCJKsc-Regular.otf");
                var shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/Shaders/PMSAmberCRT.shader");
                var ward=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/Ward_WarmGray.png");
                if(!font||!shader||!ward)throw new Exception("Missing font, shader or monitor texture.");
                if(ShaderUtil.ShaderHasError(shader))throw new Exception("CRT shader has compiler errors.");
                var go=new GameObject("PMS Amber Terminal");var view=go.AddComponent<PMSTerminalView>();
                view.Typeface=font;view.DisplayShader=shader;view.Ward=ward;
                view.GlowLine=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/Phosphor_Line.png");view.GlowDot=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/Phosphor_Dot.png");
                PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/PMSAmberTerminal.prefab");UnityEngine.Object.DestroyImmediate(go);
                var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Art/PMSAmberCRT.mat");
                if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,Root+"/Art/PMSAmberCRT.mat");}else material.shader=shader;
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/PMSAmberTerminal.prefab");var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
                var preview=instance.AddComponent<PMSPreviewDriver>();preview.View=instance.GetComponent<PMSTerminalView>();
                EditorSceneManager.SaveScene(scene,Root+"/Scenes/PMSAmberDemo.unity");AssetDatabase.SaveAssets();
                File.WriteAllText(Path.Combine(Application.dataPath,"../import-validation.json"),"{\"unity\":\""+Application.unityVersion+"\",\"font\":true,\"ward\":true,\"shaderErrors\":false,\"prefab\":true,\"demoScene\":true}");
                Debug.Log("PMS_PACKAGE_IMPORT_PASS");
            }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        private static void ConfigureImports()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Root+"/Art"}))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                bool sprite=Path.GetFileName(importer.assetPath).StartsWith("UI_");
                importer.textureType=sprite?TextureImporterType.Sprite:TextureImporterType.Default;
                if(sprite){importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=new Vector4(1,1,1,1);}
                importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                importer.filterMode=importer.assetPath.Contains("Phosphor")?FilterMode.Bilinear:FilterMode.Point;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;importer.SaveAndReimport();
            }
        }
    }
}
