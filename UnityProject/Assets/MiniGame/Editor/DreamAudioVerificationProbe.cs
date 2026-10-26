using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace MiniGame.UnityHost.Editor {
 [InitializeOnLoad] public static class DreamAudioVerificationProbe {
  static DreamAudioVerificationProbe() { EditorApplication.delayCall += Verify; }
  static void Verify() {
   string output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Artifacts", "DreamAudioVerification", "editor-verification.json"));
   Directory.CreateDirectory(Path.GetDirectoryName(output));
   GameObject root = null;
   try {
    var p = Resources.Load<MiniGameAudioProfile>("MiniGame/AudioDefault");
    if (p == null || p.EndingMusicGroup == null || p.OceanGroup == null) throw new Exception("Dedicated ending audio routes are missing.");
    root = new GameObject("Dream audio verification") { hideFlags = HideFlags.HideAndDontSave };
    var service = root.AddComponent<MiniGameAudioService>(); service.Configure(p);
    service.GetSource(MiniGameAudioChannel.EndingMusic).mute = true;
    service.GetSource(MiniGameAudioChannel.Ocean).mute = true;
    service.SetLoop(MiniGameAudioChannel.EndingMusic, MiniGameSound.EndingMusic, 0);
    service.SetLoop(MiniGameAudioChannel.Ocean, MiniGameSound.OceanWaves, 0);
    var music = service.GetSource(MiniGameAudioChannel.EndingMusic); var ocean = service.GetSource(MiniGameAudioChannel.Ocean);
    if (music.outputAudioMixerGroup != p.EndingMusicGroup || ocean.outputAudioMixerGroup != p.OceanGroup) throw new Exception("Playback did not use the dedicated routes.");
    var snapshot = p.Mixer.FindSnapshot("Snapshot"); var so = new SerializedObject(snapshot); var it = so.GetIterator(); string dump = "";
    while (it.Next(true)) if (it.propertyType == SerializedPropertyType.Float) dump += it.propertyPath + "=" + it.floatValue.ToString("R",System.Globalization.CultureInfo.InvariantCulture) + ";";
    File.WriteAllText(output, JsonUtility.ToJson(new Result { success = true, project = Application.dataPath, musicRoute = music.outputAudioMixerGroup.name, oceanRoute = ocean.outputAudioMixerGroup.name, musicClip = music.clip.name, oceanClip = ocean.clip.name, snapshotFloats = dump }, true));
   } catch (Exception ex) { File.WriteAllText(output, JsonUtility.ToJson(new Result { success=false, error=ex.ToString() },true)); }
   finally { if (root != null) UnityEngine.Object.DestroyImmediate(root); }
  }
  [Serializable] class Result { public bool success; public string project, musicRoute, oceanRoute, musicClip, oceanClip, snapshotFloats, error; }
 }
}
