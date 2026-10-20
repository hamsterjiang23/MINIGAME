using System;
using System.Collections.Generic;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEngine;

namespace MiniGame.UnityHost
{
    /// <summary>Removes Day0 furniture obstruction while retaining the room shell and the bed's ray target.</summary>
    public sealed class BedtimeCollisionGate : MonoBehaviour
    {
        private readonly Dictionary<Collider, ColliderState> retained = new Dictionary<Collider, ColliderState>();
        private GameRuntime runtime;
        private bool subscribed;

        public bool Suppressing { get; private set; }

        public void Bind(GameRuntime game)
        {
            Unsubscribe();
            Restore();
            runtime = game ?? throw new ArgumentNullException(nameof(game));
            if (!isActiveAndEnabled) return;
            Subscribe();
            Apply(game.GetView());
        }

        private void Subscribe()
        {
            if (runtime == null || subscribed) return;
            runtime.Changed += Apply;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (runtime != null && subscribed) runtime.Changed -= Apply;
            subscribed = false;
        }

        private void Apply(RuntimeView view)
        {
            bool suppress = view.Game.Day == 0 && BedtimeGuidance.IsSleepOnly(view)
                && (!view.TerminalOpen || view.LeavingTerminal);
            if (!suppress)
            {
                if (Suppressing && view.Game.Day > 0)
                    foreach (var root in gameObject.scene.GetRootGameObjects())
                        foreach (var player in root.GetComponentsInChildren<ApartmentPlayer>(true)) player.RestoreDayZeroWakePose();
                Restore(); return;
            }
            Suppressing = true;
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                {
                    // Player bodies and unknown structural colliders are never candidates.
                    if (collider is CharacterController || collider.GetComponentInParent<ApartmentPlayer>() != null
                        || collider.GetComponentInParent<CharacterController>() != null) continue;
                    var interaction = collider.GetComponentInParent<WorldInteraction>();
                    if (interaction != null && interaction.Target == InteractionTarget.Bed)
                    {
                        Remember(collider);
                        collider.isTrigger = true;
                    }
                    else if (ShouldDisable(collider, interaction))
                    {
                        Remember(collider);
                        collider.enabled = false;
                        if (interaction != null) interaction.Highlight(false);
                        var readable = collider.GetComponentInParent<ReadableProp>();
                        if (readable != null) readable.Highlight(false);
                    }
                }
        }

        private static bool ShouldDisable(Collider collider, WorldInteraction interaction)
        {
            if (interaction != null)
            {
                // The door leaf is also a room boundary; only a separate trigger may be removed.
                if (interaction.Target == InteractionTarget.Door) return collider.isTrigger;
                return true;
            }
            if (collider.GetComponentInParent<ReadableProp>() != null) return true;
            // These are the authored obstruction objects emitted by both MainRoom gameplay builders.
            // Wall/floor mesh colliders, the corridor and unclassified meshes remain untouched.
            var parent = collider.transform.parent;
            if (parent == null || parent.name != "Room collision") return false;
            string name = collider.name;
            return name == "B_obj_01 collider" || name == "B_HT01 collider"
                || name.StartsWith("B_HT01.", StringComparison.Ordinal) && name.EndsWith(" collider", StringComparison.Ordinal)
                || name == "B_SM_CupboardChunk collider" || name == "B_SM_MovingBoxes collider"
                || name == "B_SM_SingleBox collider";
        }

        private void Remember(Collider collider)
        {
            if (!retained.ContainsKey(collider)) retained.Add(collider, new ColliderState(collider));
        }

        private void Restore()
        {
            foreach (var pair in retained)
            {
                if (pair.Key == null) continue;
                pair.Key.isTrigger = pair.Value.Trigger;
                pair.Key.enabled = pair.Value.Enabled;
            }
            retained.Clear();
            Suppressing = false;
        }

        private void OnEnable()
        {
            if (runtime == null) return;
            Subscribe();
            Apply(runtime.GetView());
        }

        private void OnDisable() { Unsubscribe(); Restore(); }
        private void OnDestroy() { Unsubscribe(); Restore(); }

        private readonly struct ColliderState
        {
            public readonly bool Enabled, Trigger;
            public ColliderState(Collider collider) { Enabled = collider.enabled; Trigger = collider.isTrigger; }
        }
    }
}
