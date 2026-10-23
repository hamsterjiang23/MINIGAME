using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/Terminal Dossiers")]
    public sealed class TerminalDossierCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string patientId, age, occupation, summary, portraitId;
            public Sprite portrait;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();
        public void Configure(Entry[] values) => entries = values ?? Array.Empty<Entry>();
        public Entry Find(string patientId) => Array.Find(entries, entry => entry != null && entry.patientId == patientId);
    }
}
