using System;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [Serializable] public sealed class SupervisorDialogueLine
    {
        public string id = "", text = "", next = "", portraitId = "";
        public int day, sourceRow;
    }

    [Serializable] public sealed class SupervisorConversation
    {
        public int day, morningCount;
        public SupervisorDialogueLine[] lines = Array.Empty<SupervisorDialogueLine>();
    }

    [Serializable] public sealed class SupervisorDialogueCheckpoint
    {
        public string revision = "", sourceId = "";
        public int day;
        public PresentationKind cue;
    }

    [Serializable] public sealed class SupervisorDialogueContent
    {
        public const string SourcePrefix = "supervisor__";
        public string revision = "";
        public SupervisorConversation[] conversations = Array.Empty<SupervisorConversation>();

        public static SupervisorDialogueContent Load()
        {
            var source = Resources.Load<TextAsset>("MiniGame/Supervisor/Dialogue");
            if (source == null) return null;
            var data = JsonUtility.FromJson<SupervisorDialogueContent>(source.text);
            if (data == null || string.IsNullOrEmpty(data.revision) || data.conversations.Length != 4
                || data.conversations.SelectMany(day => day.lines).Count() != 46)
                throw new InvalidOperationException("Supervisor dialogue resource is incomplete.");
            return data;
        }

        public bool TryGetCue(RuntimeView view, out string node, out SupervisorDialogueLine[] lines)
        {
            node = ""; lines = Array.Empty<SupervisorDialogueLine>();
            if (view == null || view.TerminalOpen || view.Cue == null) return false;
            int day = view.Game.Day;
            var kind = view.Cue.Kind;
            if (kind == PresentationKind.Ending && view.Game.Ending == EndingType.TakenAway) day = 4;
            else if (kind != PresentationKind.MorningInspection && kind != PresentationKind.Briefing) return false;
            if (day < 1 || day > 4 || day == 4 && kind != PresentationKind.Ending) return false;
            var conversation = conversations.SingleOrDefault(item => item.day == day);
            if (conversation == null) return false;
            lines = kind == PresentationKind.MorningInspection ? conversation.lines.Take(conversation.morningCount).ToArray()
                : kind == PresentationKind.Briefing ? conversation.lines.Skip(conversation.morningCount).ToArray() : conversation.lines;
            node = "Supervisor_D" + day + "_" + kind;
            return lines.Length != 0;
        }

        public static string PortraitResource(string id) => id == "JD_01" ? "MiniGame/SupervisorPortrait"
            : id != null && id.Length == 5 && id.StartsWith("JD_", StringComparison.Ordinal)
                && int.TryParse(id.Substring(3), out int index) && index >= 2 && index <= 6 ? "MiniGame/Supervisor/JD" + index.ToString("00") : null;
    }
}
