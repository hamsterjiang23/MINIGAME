using System;
using System.Linq;
using MiniGame.Core;
using MiniGame.Presentation;

namespace MiniGame.UnityHost.Editor
{
    public enum CampaignDebugPoint { Opening, Morning, Room, Terminal, Shutdown, Bedtime, Sleep, Dream, AfterDream }

    /// <summary>Builds isolated, replay-validated checkpoints through the campaign's public commands.</summary>
    public static class CampaignDebugCheckpoint
    {
        public static CampaignDebugPoint[] PointsForDay(int day) => day == 0
            ? new[] { CampaignDebugPoint.Opening, CampaignDebugPoint.Room, CampaignDebugPoint.Terminal,
                CampaignDebugPoint.Shutdown, CampaignDebugPoint.Bedtime, CampaignDebugPoint.Sleep }
            : new[] { CampaignDebugPoint.Morning, CampaignDebugPoint.Room, CampaignDebugPoint.Terminal,
                CampaignDebugPoint.Shutdown, CampaignDebugPoint.Bedtime, CampaignDebugPoint.Sleep,
                CampaignDebugPoint.Dream, CampaignDebugPoint.AfterDream };

        public static string Create(GameRules rules, IGameContent content, int day, CampaignDebugPoint point,
            EndingDialogueCatalog endings = null)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (!rules.Days.Any(item => item.Day == day) || !PointsForDay(day).Contains(point))
                throw new ArgumentException("所选日期或阶段不属于当前正式内容配置。");
            if (!rules.DreamFlowEnabled && (point == CampaignDebugPoint.Dream || point == CampaignDebugPoint.AfterDream))
                throw new InvalidOperationException("当前工程没有启用每日梦境流程。");
            var runtime = new GameRuntime(rules, content, useTerminalDirectory: true);
            if (endings != null) runtime.ConfigureEndings(endings);
            if (point == CampaignDebugPoint.Opening)
            {
                Require(runtime.BeginDayZeroOpening(), runtime.GetView());
                return runtime.Save();
            }
            for (int step = 0; step < 6000; step++)
            {
                var view = runtime.GetView();
                if (Matches(view, day, point)) return runtime.Save();
                if (view.Game.Day > day || view.Game.Phase == GamePhase.Ending || view.Game.Phase == GamePhase.SelfReview)
                    throw new InvalidOperationException($"前置流程在第 {view.Game.Day} 天 / {view.Game.Phase} 结束，未到达第 {day} 天 / {point}。");
                Require(Advance(runtime, rules, view), view);
            }
            throw new InvalidOperationException("调试进度生成超过命令上限，请检查正式流程配置。");
        }

        public static bool Matches(RuntimeView view, int day, CampaignDebugPoint point)
        {
            if (point == CampaignDebugPoint.AfterDream && view.Game.Day > day)
                return view.Game.Phase == GamePhase.MorningInspection || view.Game.Phase == GamePhase.Ending;
            if (view.Game.Day != day) return false;
            switch (point)
            {
                case CampaignDebugPoint.Opening:
                    return view.Cue?.Kind == PresentationKind.EnterTerminal && view.TerminalBootRequired;
                case CampaignDebugPoint.Morning:
                    return view.Game.Phase == GamePhase.MorningInspection && view.Cue == null;
                case CampaignDebugPoint.Room:
                    return view.Game.Phase == GamePhase.Apartment && !view.TerminalOpen && view.Cue == null;
                case CampaignDebugPoint.Terminal:
                    return view.TerminalOpen && view.TerminalPage == TerminalPage.Directory && view.Cue == null;
                case CampaignDebugPoint.Shutdown:
                    return view.TerminalShuttingDown && view.LeavingTerminal && view.Cue?.Kind == PresentationKind.ExitTerminal;
                case CampaignDebugPoint.Bedtime:
                    return view.Game.Phase == GamePhase.Evening && !view.TerminalOpen && view.Cue == null;
                case CampaignDebugPoint.Sleep:
                    return view.Game.Phase == GamePhase.Evening && view.Cue?.Kind == PresentationKind.Sleep;
                case CampaignDebugPoint.Dream:
                    return view.Game.Phase == GamePhase.Dream && view.Cue?.Kind == PresentationKind.Dream;
                case CampaignDebugPoint.AfterDream:
                    return (view.Game.Phase == GamePhase.AfterDream || view.Game.Phase == GamePhase.SelfReview) && view.Cue == null;
                default: return false;
            }
        }

        private static RuntimeResult Advance(GameRuntime runtime, GameRules rules, RuntimeView view)
        {
            if (view.Cue != null) return runtime.CompleteCue(view.Cue.Token);
            if (view.TerminalPage == TerminalPage.Archive) return runtime.LeaveTerminal();
            if (view.TerminalPage == TerminalPage.Directory)
            {
                var patient = view.Patients.FirstOrDefault(item => item.CanReceiveToday);
                if (patient == null) throw new InvalidOperationException("病人目录中没有可继续的会谈。");
                return runtime.EnterSelectedPatient(patient.Id);
            }
            if (view.Input == InputContext.Dialogue)
            {
                if (view.Game.EncounterStage == EncounterStage.AwaitingDisposition)
                {
                    var choices = view.Game.AvailableDispositions;
                    // Meet the nightly quota so debug checkpoints can reach later days.
                    var disposition = choices.Contains(Disposition.Purify) ? Disposition.Purify
                        : choices.Contains(Disposition.Treat) ? Disposition.Treat : choices.First();
                    return runtime.SubmitDisposition(view.Game.CurrentEncounterId, disposition);
                }
                var response = view.Game.AvailableResponseIds.OrderBy(id =>
                    rules.RebellionOptionDeltas.TryGetValue(id, out int delta) ? delta : 0).First();
                return runtime.SelectResponse(view.Game.CurrentEncounterId, view.Game.CurrentNodeId, response);
            }
            if (view.Game.Phase == GamePhase.Evening || view.Game.Phase == GamePhase.AfterDream)
                return runtime.TryInteract(InteractionTarget.Bed);
            if (view.Game.Phase == GamePhase.MorningInspection) return runtime.TryInteract(InteractionTarget.Door);
            if (view.Game.Phase == GamePhase.Apartment || view.Game.Phase == GamePhase.Work)
                return runtime.TryInteract(InteractionTarget.Computer);
            throw new InvalidOperationException("没有可用的前置步骤：" + view.Game.Phase + " / " + view.Input);
        }

        private static void Require(RuntimeResult result, RuntimeView before)
        {
            if (!result.Accepted)
                throw new InvalidOperationException($"调试前置命令被拒绝：{before.Game.Day}/{before.Game.Phase} — {result.Reason}");
        }
    }
}
