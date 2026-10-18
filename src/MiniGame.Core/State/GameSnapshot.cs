#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace MiniGame.Core
{
    // Mutable transport DTOs, never the live session state. Capture and restore copy them.
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GameSnapshot
    {
        [JsonProperty(Required = Required.Always)] public int SchemaVersion { get; set; }
        [JsonProperty(Required = Required.Always)] public string RulesId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string RulesFingerprint { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public GameStateSnapshot State { get; set; } = new GameStateSnapshot();
        [JsonProperty(Required = Required.Always)] public List<CommandRecord> CommandHistory { get; set; } = new List<CommandRecord>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GameStateSnapshot
    {
        [JsonProperty(Required = Required.Always)] public int Day { get; set; }
        [JsonProperty(Required = Required.Always)] public GamePhase Phase { get; set; }
        [JsonProperty(Required = Required.Always)] public EndingType Ending { get; set; }
        [JsonProperty(Required = Required.Always)] public List<PatientSnapshot> Patients { get; set; } = new List<PatientSnapshot>();
        [JsonProperty(Required = Required.Always)] public List<DaySnapshot> Days { get; set; } = new List<DaySnapshot>();
        [JsonProperty(Required = Required.Always)] public List<string> Queue { get; set; } = new List<string>();
        [JsonProperty(Required = Required.Always)] public int QueueIndex { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public string? CurrentEncounterId { get; set; }
        [JsonProperty(Required = Required.Always)] public List<EncounterSnapshot> Encounters { get; set; } = new List<EncounterSnapshot>();
        [JsonProperty(Required = Required.Always)] public List<ScheduledEventSnapshot> PendingEvents { get; set; } = new List<ScheduledEventSnapshot>();
        [JsonProperty(Required = Required.Always)] public List<string> ExecutedEventIds { get; set; } = new List<string>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class PatientSnapshot
    {
        [JsonProperty(Required = Required.Always)] public string Id { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public int Trust { get; set; }
        [JsonProperty(Required = Required.Always)] public int TreatmentCount { get; set; }
        [JsonProperty(Required = Required.Always)] public PatientPresence Presence { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DaySnapshot
    {
        [JsonProperty(Required = Required.Always)] public int Day { get; set; }
        [JsonProperty(Required = Required.Always)] public int PurificationQuota { get; set; }
        [JsonProperty(Required = Required.Always)] public int Purifications { get; set; }
        [JsonProperty(Required = Required.Always)] public bool Completed { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public bool? QuotaMet { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class EncounterSnapshot
    {
        [JsonProperty(Required = Required.Always)] public string Id { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public int Day { get; set; }
        [JsonProperty(Required = Required.Always)] public string PatientId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public EncounterStage Stage { get; set; }
        [JsonProperty(Required = Required.Always)] public int NormalRoundIndex { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public string? PendingBranchNodeId { get; set; }
        [JsonProperty(Required = Required.Always)] public bool BranchIsSpecial { get; set; }
        [JsonProperty(Required = Required.Always)] public bool SpecialStarted { get; set; }
        [JsonProperty(Required = Required.Always)] public bool SpecialCompleted { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public Disposition? Disposition { get; set; }
        [JsonProperty(Required = Required.Always)] public List<ResponseRecord> Responses { get; set; } = new List<ResponseRecord>();
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)] public bool ExplicitlySelected { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ResponseRecord
    {
        [JsonProperty(Required = Required.Always)] public string NodeId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public string ResponseId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public int TrustDelta { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ScheduledEventSnapshot
    {
        [JsonProperty(Required = Required.Always)] public string Id { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public int DueDay { get; set; }
        [JsonProperty(Required = Required.Always)] public string PatientId { get; set; } = "";
        [JsonProperty(Required = Required.Always)] public ScheduledEventKind Kind { get; set; }
    }
}
