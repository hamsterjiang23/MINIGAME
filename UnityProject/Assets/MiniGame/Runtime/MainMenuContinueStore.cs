using System;
using System.IO;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Automatic continuation has priority without replacing either manual save.</summary>
    public static class MainMenuContinueStore
    {
        private const string CompletedName = "campaign-completed.json";
        public static string ContinuePath => Path.Combine(Application.persistentDataPath, "campaign-continue.json");

        public static string ResolveResumePath(string directory = null)
        {
            var root = directory ?? Application.persistentDataPath;
            if (HasCompletedInspection(root)) return null;
            foreach (var name in new[] { "campaign-continue.json", "campaign-dream-save.json", "campaign-save.json" })
            {
                var path = Path.Combine(root, name);
                if (File.Exists(path)) return path;
            }
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
