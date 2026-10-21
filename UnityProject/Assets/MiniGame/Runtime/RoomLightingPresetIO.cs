using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiniGame.UnityHost
{
    /// <summary>Portable lighting data; never serializes game saves or Unity object metadata.</summary>
    public static class RoomLightingPresetIO
    {
        public const string Format = "NORM.RoomLightingPreset";
        public const int Version = 1;
        private const int MaximumJsonLength = 2 * 1024 * 1024;
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        private static readonly FieldInfo[] DataFields = typeof(ProfileData).GetFields(BindingFlags.Public | BindingFlags.Instance);

        [Serializable] private sealed class Envelope
        {
            public string format;
            public int version;
            public string profileName;
            public string profileJson;
        }

        // A data-only DTO prevents m_Name, instanceID or other Unity-object fields entering the target.
#pragma warning disable CS0649 // Assigned through JsonUtility and the field-copy helpers.
        [Serializable] private sealed class ProfileData
        {
            public Vector3 lampOffset, lampEulerAngles, rugOffset;
            public Color lampColor, ambientColor, windowColor, rugTint;
            public float lampIntensity, lampRange, outerAngle, innerAngle, shadowStrength, shadowBias, shadowNormalBias, bulbEmission;
            public float reflectionIntensity, windowIntensity, windowRange, monitorIntensity;
            public bool lightingBandsEnabled;
            public int lightingBands;
            public float lightingBandsStrength, lightingDither, floorGain, floorContrast, floorSmoothness;
            public bool rugVisible;
            public Vector2 rugSize;
            public float rugYaw;
            public List<SceneLightSettings> SceneLights;
            public RoomNightLightingSettings night;
        }
#pragma warning restore CS0649

        public static string ExportJson(RoomLightingProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            RoomLightingProfile temporary = null;
            try
            {
                temporary = ScriptableObject.CreateInstance<RoomLightingProfile>();
                var data = JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(ReadData(profile)));
                WriteData(data, temporary); temporary.NotifyChanged();
                return JsonUtility.ToJson(new Envelope { format = Format, version = Version,
                    profileName = profile.name ?? "", profileJson = JsonUtility.ToJson(ReadData(temporary)) }, true);
            }
            finally { Release(temporary); }
        }

        public static bool TryImportJson(string json, RoomLightingProfile target, out string message)
        {
            message = "";
            if (target == null) { message = "没有可导入的房间灯光配置。"; return false; }
            RoomLightingProfile temporary = null;
            try
            {
                RequireObject(json);
                bool legacy = !HasField(json, "format") && HasField(json, "lampIntensity");
                string profileJson = json;
                if (!legacy)
                {
                    RequireFields(json, "format", "version", "profileName", "profileJson");
                    var envelope = JsonUtility.FromJson<Envelope>(json);
                    if (envelope == null || envelope.format != Format) throw new InvalidDataException("不是 NORM 房间灯光配置格式。");
                    if (envelope.version != Version) throw new InvalidDataException("不支持此灯光配置版本。");
                    if (envelope.profileName == null) throw new InvalidDataException("缺少配置名称。");
                    profileJson = envelope.profileJson;
                }
                RequireObject(profileJson);
                foreach (var field in DataFields)
                    if (!legacy || field.Name != "night" && field.Name != "SceneLights" && field.Name != "lampEulerAngles") RequireFields(profileJson, field.Name);
                temporary = ScriptableObject.CreateInstance<RoomLightingProfile>();
                var data = ReadData(temporary);
                JsonUtility.FromJsonOverwrite(profileJson, data);
                if (data.SceneLights == null || data.night == null || data.night.SceneLights == null)
                    throw new InvalidDataException("灯光列表或夜间配置不完整。");
                WriteData(data, temporary); temporary.NotifyChanged();
                var backup = ReadData(target);
                try { WriteData(ReadData(temporary), target); target.NotifyChanged(); }
                catch { WriteData(backup, target); throw; }
                message = legacy ? "已导入旧版白天灯光配置，缺失参数使用默认值。" : "已导入白天与夜间灯光配置。";
                return true;
            }
            catch (Exception exception) { message = "导入失败：" + exception.Message; return false; }
            finally { Release(temporary); }
        }

        public static void SaveFile(RoomLightingProfile profile, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("请选择灯光配置文件路径。", nameof(path));
            string json = ExportJson(profile), destination = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string temporary = destination + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(temporary, json, Utf8);
                if (File.Exists(destination)) File.Replace(temporary, destination, null);
                else File.Move(temporary, destination);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        public static bool TryLoadFile(string path, RoomLightingProfile target, out string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path)) { message = "请选择灯光配置文件。"; return false; }
                if (!File.Exists(path)) { message = "灯光配置文件不存在。"; return false; }
                if (new FileInfo(path).Length > MaximumJsonLength * 4L) { message = "灯光配置文件过大。"; return false; }
                return TryImportJson(File.ReadAllText(path, Utf8), target, out message);
            }
            catch (Exception exception) { message = "读取失败：" + exception.Message; return false; }
        }

        private static ProfileData ReadData(RoomLightingProfile profile)
        {
            var data = new ProfileData();
            foreach (var field in DataFields) field.SetValue(data, typeof(RoomLightingProfile).GetField(field.Name).GetValue(profile));
            return data;
        }
        private static void WriteData(ProfileData data, RoomLightingProfile profile)
        {
            foreach (var field in DataFields) typeof(RoomLightingProfile).GetField(field.Name).SetValue(profile, field.GetValue(data));
        }
        private static bool HasField(string json, string name) => Regex.IsMatch(json, "\"" + name + "\"\\s*:");
        private static void RequireFields(string json, params string[] names)
        {
            foreach (string name in names)
                if (!HasField(json, name)) throw new InvalidDataException("配置缺少字段：" + name);
        }
        private static void RequireObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new InvalidDataException("配置 JSON 为空。");
            if (json.Length > MaximumJsonLength) throw new InvalidDataException("配置 JSON 过大。");
            string trimmed = json.Trim();
            if (!trimmed.StartsWith("{", StringComparison.Ordinal) || !trimmed.EndsWith("}", StringComparison.Ordinal))
                throw new InvalidDataException("配置必须是完整 JSON 对象。");
        }
        private static void Release(RoomLightingProfile profile)
        { if (profile != null) { if (Application.isPlaying) Object.Destroy(profile); else Object.DestroyImmediate(profile); } }
    }
}
