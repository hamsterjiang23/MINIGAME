using System.Collections.Generic;
using System.Linq;
using Yarn.Unity;

#nullable enable

namespace MiniGame.UnityHost
{
    public sealed class MiniGameYarnPresenter : DialoguePresenterBase
    {
        private MiniGameYarnBridge? _bridge;
        private readonly HashSet<YarnTask> _pendingPresentations = new HashSet<YarnTask>();
        internal bool HasPendingPresentations => _pendingPresentations.Count != 0;
        internal int PendingPresentationCount => _pendingPresentations.Count;

        public void Initialize(MiniGameYarnBridge bridge) => _bridge = bridge;

        public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            var task = _bridge != null ? _bridge.PresentLineAsync(line, token) : YarnTask.CompletedTask;
            TrackPresentationAsync(task).Forget();
            return task;
        }

        public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken token)
        {
            var task = _bridge != null ? _bridge.PresentOptionsAsync(dialogueOptions, token) : YarnTask<DialogueOption?>.FromResult(null);
            TrackOptionsAsync(task).Forget();
            return task;
        }

        private async YarnTask TrackOptionsAsync(YarnTask<DialogueOption?> task)
        {
            // Track the actual returned task, including its completion, through a non-generic barrier.
            var completed = new YarnTaskCompletionSource();
            _pendingPresentations.Add(completed.Task);
            try { await task; }
            catch { /* DialogueRunner reports presenter failures. This barrier must still drain. */ }
            finally
            {
                _pendingPresentations.Remove(completed.Task);
                completed.TrySetResult();
            }
        }

        private async YarnTask TrackPresentationAsync(YarnTask task)
        {
            _pendingPresentations.Add(task);
            try { await task; }
            catch { /* DialogueRunner reports presenter failures. This barrier must still drain. */ }
            finally { _pendingPresentations.Remove(task); }
        }

        internal async YarnTask DrainPresentationsAsync()
        {
            while (_pendingPresentations.Count != 0)
            {
                var pending = _pendingPresentations.ToArray();
                try { await YarnTask.WhenAll(pending); }
                catch { /* The runner's line/options wrappers log their own failures. */ }
                await YarnTask.Yield();
            }
            // Keep the old dialogue cancellation source alive and cancelled until the runner's
            // outer line/options continuations have disposed their fields and observed cancellation.
            await YarnTask.Yield();
        }

        public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
        public override YarnTask OnDialogueCompleteAsync() => DrainPresentationsAsync();
    }
}
