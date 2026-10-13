#nullable enable

using System;
using Newtonsoft.Json;

namespace MiniGame.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CommandRecord
    {
        [JsonProperty(Required = Required.Always)] public CommandKind Kind { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public InteractionTarget? Target { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public string? EncounterId { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public string? NodeId { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public string? ResponseId { get; set; }
        [JsonProperty(Required = Required.AllowNull)] public Disposition? Disposition { get; set; }

        internal static CommandRecord From(GameCommand command)
        {
            switch (command)
            {
                case CompleteMorningInspection _: return new CommandRecord { Kind = CommandKind.CompleteMorningInspection };
                case CompleteBriefing _: return new CommandRecord { Kind = CommandKind.CompleteBriefing };
                case CompleteSleep _: return new CommandRecord { Kind = CommandKind.CompleteSleep };
                case CompleteDream _: return new CommandRecord { Kind = CommandKind.CompleteDream };
                case Interact c: return new CommandRecord { Kind = CommandKind.Interact, Target = c.Target };
                case SelectResponse c: return new CommandRecord
                    { Kind = CommandKind.SelectResponse, EncounterId = c.EncounterId, NodeId = c.NodeId, ResponseId = c.ResponseId };
                case CompleteResponseBranch c: return new CommandRecord
                    { Kind = CommandKind.CompleteResponseBranch, EncounterId = c.EncounterId, NodeId = c.BranchNodeId };
                case SubmitDisposition c: return new CommandRecord
                    { Kind = CommandKind.SubmitDisposition, EncounterId = c.EncounterId, Disposition = c.Disposition };
                case CompleteReaction c: return new CommandRecord
                    { Kind = CommandKind.CompleteReaction, EncounterId = c.EncounterId };
                default: throw new ArgumentException("Unsupported command.", nameof(command));
            }
        }

        internal GameCommand ToCommand()
        {
            GameCommand command;
            switch (Kind)
            {
                case CommandKind.CompleteMorningInspection: command = new CompleteMorningInspection(); break;
                case CommandKind.CompleteBriefing: command = new CompleteBriefing(); break;
                case CommandKind.CompleteSleep: command = new CompleteSleep(); break;
                case CommandKind.CompleteDream: command = new CompleteDream(); break;
                case CommandKind.Interact when Target.HasValue: command = new Interact(Target.Value); break;
                case CommandKind.SelectResponse when EncounterId != null && NodeId != null && ResponseId != null:
                    command = new SelectResponse(EncounterId, NodeId, ResponseId); break;
                case CommandKind.CompleteResponseBranch when EncounterId != null && NodeId != null:
                    command = new CompleteResponseBranch(EncounterId, NodeId); break;
                case CommandKind.SubmitDisposition when EncounterId != null && Disposition.HasValue:
                    command = new SubmitDisposition(EncounterId, Disposition.Value); break;
                case CommandKind.CompleteReaction when EncounterId != null:
                    command = new CompleteReaction(EncounterId); break;
                default: throw new SnapshotValidationException("Invalid command record.");
            }
            if (SnapshotCodec.Canonical(From(command)) != SnapshotCodec.Canonical(this))
                throw new SnapshotValidationException("Command contains unexpected arguments.");
            return command;
        }
    }
}
