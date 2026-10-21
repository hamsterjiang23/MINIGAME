#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MiniGame.Presentation.Authoring
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CampaignDefinition
    {
        [JsonProperty("patients", Required = Required.Always)] public List<CampaignPatient> Patients { get; set; } = new List<CampaignPatient>();
        [JsonProperty("days", Required = Required.Always)] public List<CampaignDay> Days { get; set; } = new List<CampaignDay>();
        [JsonProperty("releaseThreshold", Required = Required.Always)] public int ReleaseThreshold { get; set; }
        [JsonProperty("variants", Required = Required.Always)] public List<DialogueTextVariant> Variants { get; set; } = new List<DialogueTextVariant>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CampaignPatient
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; } = "";
        [JsonProperty("name", Required = Required.Always)] public string Name { get; set; } = "";
        [JsonProperty("firstDay", Required = Required.Always)] public int FirstDay { get; set; }
        [JsonProperty("initialTrust", Required = Required.Always)] public int InitialTrust { get; set; }
        [JsonProperty("treatmentLimit", Required = Required.Always)] public int TreatmentLimit { get; set; }
        [JsonProperty("treatmentOutcome", Required = Required.Always)] public string TreatmentOutcome { get; set; } = "";
        [JsonProperty("treatmentDelayDays", Required = Required.Always)] public int TreatmentDelayDays { get; set; }
        [JsonProperty("attacksAfterRelease", Required = Required.Always)] public bool AttacksAfterRelease { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CampaignDay
    {
        [JsonProperty("day", Required = Required.Always)] public int Day { get; set; }
        [JsonProperty("quota", Required = Required.Always)] public int Quota { get; set; }
        [JsonProperty("meetings", Required = Required.Always)] public List<CampaignMeeting> Meetings { get; set; } = new List<CampaignMeeting>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CampaignMeeting
    {
        [JsonProperty("patientId", Required = Required.Always)] public string PatientId { get; set; } = "";
        [JsonProperty("startNode", Required = Required.Always)] public string StartNode { get; set; } = "";
        [JsonProperty("endNode", Required = Required.Always)] public string EndNode { get; set; } = "";
        [JsonProperty("yarnSource", Required = Required.Always)] public string YarnSource { get; set; } = "";
        [JsonProperty("specialRoundStartNode", NullValueHandling = NullValueHandling.Ignore)] public string? SpecialRoundStartNode { get; set; }
        [JsonProperty("specialTrustThreshold", NullValueHandling = NullValueHandling.Ignore)] public int? SpecialTrustThreshold { get; set; }
        [JsonProperty("releaseThreshold", NullValueHandling = NullValueHandling.Ignore)] public int? ReleaseThreshold { get; set; }
    }

    /// <summary>Original text remains canonical; only the presentation wording changes when the historical fact is absent.</summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DialogueTextVariant
    {
        [JsonProperty("sourceId", Required = Required.Always)] public string SourceId { get; set; } = "";
        [JsonProperty("predicate", Required = Required.Always)] public string Predicate { get; set; } = "";
        [JsonProperty("fallbackText", Required = Required.Always)] public string FallbackText { get; set; } = "";
    }
}
