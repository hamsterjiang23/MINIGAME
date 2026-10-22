#nullable enable
using System;
using System.Linq;
using Newtonsoft.Json;

namespace MiniGame.Presentation
{
    /// <summary>Fixed, namespaced source snapshots. Progress belongs to GameRuntime / GameSession.</summary>
    public sealed class EndingDialogueCatalog
    {
        public int FormatVersion { get; set; }
        public string Revision { get; set; } = "";
        public string ContentVersion { get; set; } = "";
        public EndingConversation[] Conversations { get; set; } = Array.Empty<EndingConversation>();
        public EndingConversation Get(string id) => Conversations.Single(item => item.Id == id);

        public static EndingDialogueCatalog Parse(string json)
        {
            var catalog = JsonConvert.DeserializeObject<EndingDialogueCatalog>(json)
                ?? throw new ArgumentException("Ending dialogue must be an object.", nameof(json));
            if (catalog.FormatVersion != 1 || string.IsNullOrWhiteSpace(catalog.Revision)
                || catalog.Conversations.Length != 3 || catalog.Conversations.Select(item => item.Id).Distinct().Count() != 3)
                throw new ArgumentException("Ending dialogue snapshot is incomplete.", nameof(json));
            foreach (var conversation in catalog.Conversations)
                if (conversation.Lines.Length == 0 || conversation.Lines.Any(line => string.IsNullOrWhiteSpace(line.Text)
                    || !line.SourceId.StartsWith(conversation.Id + "__", StringComparison.Ordinal)))
                    throw new ArgumentException("Ending dialogue has missing text or an invalid namespace.", nameof(json));
            if (catalog.Get("self-review").Lines.Length != 16 || catalog.Get("self-review").FinalAction != "purify"
                || catalog.Get("priest-ending").Lines.Length != 8 || catalog.Get("supervisor-ending").Lines.Length != 7
                || catalog.Conversations.SelectMany(item => item.Lines).Select(line => line.SourceId).Distinct().Count() != 31)
                throw new ArgumentException("Ending dialogue line counts or actions do not match the authored source.", nameof(json));
            return catalog;
        }
    }

    public sealed class EndingConversation
    {
        public string Id { get; set; } = "";
        public string Node { get; set; } = "";
        public string Speaker { get; set; } = "";
        public string FinalAction { get; set; } = "";
        public EndingDialogueLine[] Lines { get; set; } = Array.Empty<EndingDialogueLine>();
    }

    public sealed class EndingDialogueLine
    {
        public string Id { get; set; } = "";
        public string SourceId { get; set; } = "";
        public int SourceRow { get; set; }
        public string SourceCell { get; set; } = "";
        public string Text { get; set; } = "";
        public string PortraitId { get; set; } = "";
        public bool BlackScreen { get; set; }
    }
}
