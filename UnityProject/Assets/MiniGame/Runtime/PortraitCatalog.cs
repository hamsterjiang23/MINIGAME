using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/Portrait Catalog")]
    public sealed class PortraitCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string id;
            public Sprite sprite;
        }
        [SerializeField] private Entry[] entries = Array.Empty<Entry>();
        private Dictionary<string, Sprite> lookup;

        public void Configure(Entry[] values) { entries = values ?? Array.Empty<Entry>(); lookup = null; }
        private void OnValidate() => lookup = null;
        public Sprite Find(string id)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<string, Sprite>(StringComparer.Ordinal);
                foreach (var entry in entries)
                    if (entry != null && !string.IsNullOrEmpty(entry.id) && entry.sprite != null) lookup.Add(entry.id, entry.sprite);
            }
            return id != null && lookup.TryGetValue(id, out var sprite) ? sprite : null;
        }

        // A missing expression may fall back only to the same patient's neutral portrait.
        public Sprite Resolve(string id, string patientId) => Find(id) ?? Find(BasePortrait(patientId));

        public static string BasePortrait(string patientId)
        {
            switch (patientId)
            {
                case "child": return "ML01";
                case "former-ruler": return "VV01";
                case "battlefield-doctor": return "ED01";
                case "diplomat": return "AD01";
                case "priest": return "JN01";
                case "tutorial": return "D0_01";
                default: return "";
            }
        }

        public static string Unit(string patientId) => "B-" + (12 + PatientIndex(patientId)).ToString("00");
        public static string Registration(string patientId) => "R-0317-" + (42 + PatientIndex(patientId)).ToString("000");
        private static int PatientIndex(string patientId)
        {
            switch (patientId)
            {
                case "former-ruler": return 1;
                case "battlefield-doctor": return 2;
                case "diplomat": return 3;
                case "priest": return 4;
                case "tutorial": return 5;
                default: return 0;
            }
        }
    }
}
