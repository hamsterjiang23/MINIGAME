using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.UnityHost
{
    public enum CampaignSaveSlot { Manual, Automatic }

    /// <summary>Manual progress has priority; the automatic slot never replaces a manual file.</summary>
    public static class MainMenuContinueStore
    {
        private const string CompletedName = "campaign-completed.json";
        public static string ContinuePath => Path.Combine(Application.persistentDataPath, "campaign-continue.json");

        public static IEnumerable<string> CandidatePaths(string directory, CampaignSaveSlot? slot = null)
        {
            var root = directory ?? Application.persistentDataPath;
            if (slot != CampaignSaveSlot.Automatic)
            {
                yield return Path.Combine(root, "campaign-dream-save.json");
                yield return Path.Combine(root, "campaign-save.json");
            }
            if (slot != CampaignSaveSlot.Manual) yield return Path.Combine(root, "campaign-continue.json");
        }

        public static string ResolveResumePath(string directory = null)
        {
            foreach (var path in CandidatePaths(directory))
            {
                if (File.Exists(path)) return path;
            }
            return null;
        }

        public static string ResolveAvailable(string directory, CampaignSaveSlot? slot,
            Func<string, string> validate, out string warning)
        {
            var failures = new List<string>();
            foreach (var path in CandidatePaths(directory, slot))
            {
                if (!File.Exists(path)) continue;
                var error = validate(path);
                if (string.IsNullOrEmpty(error))
                {
                    warning = failures.Count == 0 ? null : "已跳过不可用存档：" + string.Join("；", failures);
                    return path;
                }
                failures.Add(Path.GetFileName(path) + "：" + error);
            }
            warning = failures.Count == 0 ? "空槽位" : string.Join("；", failures);
            return null;
        }

        public static bool HasCompletedInspection(string directory = null) =>
            File.Exists(Path.Combine(directory ?? Application.persistentDataPath, CompletedName));

        public static void MarkCompleted(string directory = null)
        {
            var root = directory ?? Application.persistentDataPath;
            Directory.CreateDirectory(root);
            string destination = Path.Combine(root, CompletedName);
            string temporary = destination + ".tmp";
            File.WriteAllText(temporary, "{\"completedAtUtc\":\"" + DateTime.UtcNow.ToString("O") + "\"}");
            if (File.Exists(destination)) File.Replace(temporary, destination, null);
            else File.Move(temporary, destination);
        }

        public static void StartNewInspection(string directory = null)
        {
            // Only the completion marker is removed. All manual and previous save files stay intact.
            string path = Path.Combine(directory ?? Application.persistentDataPath, CompletedName);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
