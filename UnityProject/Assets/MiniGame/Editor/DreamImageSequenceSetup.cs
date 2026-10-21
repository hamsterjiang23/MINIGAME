using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class DreamImageSequenceSetup
    {
        public const string Destination = "Assets/MiniGame/Art/Dreams/Sequences";
        private const float ImagesPerSecond = 1f;

        [MenuItem("MiniGame/梦境/导入 McQueen 每日图片序列")]
        public static void Import()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请先停止播放再导入梦境序列。");
            string root = Directory.GetParent(Application.dataPath).Parent.FullName;
            var profile = DreamSequenceSetup.EnsureProfile();
            var imported = new Texture2D[3][];
            for (int day = 1; day <= 3; day++)
            {
                string folder = Path.Combine(root, "Art", "Dream", "Dream" + day);
                var files = Directory.GetFiles(folder, "*.png").Select(path => new { Path = path, Index = FrameNumber(path) })
                    .OrderBy(value => value.Index).ToArray();
                if (!files.Select(value => value.Index).SequenceEqual(Enumerable.Range(1, 10)))
                    throw new InvalidOperationException("Dream" + day + " 必须包含编号 1–10 各一张；无后缀图片视为第 1 张。");
                string targetFolder = Destination + "/Day" + day;
                Directory.CreateDirectory(targetFolder);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                imported[day - 1] = new Texture2D[10];
                foreach (var file in files)
                {
                    string target = targetFolder + "/Frame" + file.Index.ToString("00") + ".png";
                    if (!File.Exists(target)) File.Copy(file.Path, target, false);
                    else if (!SameBytes(file.Path, target))
                        throw new InvalidOperationException("已存在不同的梦境图片，未覆盖：" + target);
                    AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(target);
                    importer.textureType = TextureImporterType.Default;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.maxTextureSize = 2048;
                    importer.mipmapEnabled = false;
                    importer.isReadable = false;
                    importer.sRGBTexture = true;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                    imported[day - 1][file.Index - 1] = AssetDatabase.LoadAssetAtPath<Texture2D>(target);
                    if (imported[day - 1][file.Index - 1] == null) throw new InvalidOperationException("梦境图片导入失败：" + target);
                }
            }
            Undo.RecordObject(profile, "绑定每日梦境图片序列");
            float[] fades = { .15f, .2f, .1f };
            float[] motion = { .8f, .7f, .65f };
            float[] post = { .8f, 1f, 1.2f };
            for (int day = 1; day <= 3; day++)
            {
                var settings = profile.GetDay(day);
                if (settings == null) throw new InvalidOperationException("默认配置缺少 Day" + day);
                settings.Frames = imported[day - 1];
                settings.Artwork = settings.Frames[0];
                settings.Duration = settings.Frames.Length / ImagesPerSecond;
                settings.CrossfadeSeconds = fades[day - 1];
                settings.MotionIntensity = motion[day - 1];
                settings.PostIntensity = post[day - 1];
            }
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
            Debug.Log("DREAM_SEQUENCE_IMPORTED days=3 frames=30 order=1..10 imagesPerSecond=1 duration=10; music and ending settings preserved");
        }

        public static int FrameNumber(string path)
        {
            var match = Regex.Match(Path.GetFileNameWithoutExtension(path), @"-(\d+)$");
            return match.Success ? int.Parse(match.Groups[1].Value) : 1;
        }

        private static bool SameBytes(string first, string second)
        {
            if (new FileInfo(first).Length != new FileInfo(second).Length) return false;
            using (var hash = SHA256.Create())
            using (var a = File.OpenRead(first))
            using (var b = File.OpenRead(second))
                return hash.ComputeHash(a).SequenceEqual(hash.ComputeHash(b));
        }
    }
}
