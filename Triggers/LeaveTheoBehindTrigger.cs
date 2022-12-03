using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/LeaveTheoBehindTrigger")]
    class LeaveTheoBehindTrigger : Trigger {
        private static bool leaveTheoBehind = false;

        public static void Load() {
            IL.Celeste.Level.EnforceBounds += onLevelEnforceBounds;
            On.Celeste.Level.TransitionTo += onLevelTransitionTo;
        }

        public static void Unload() {
            IL.Celeste.Level.EnforceBounds -= onLevelEnforceBounds;
            On.Celeste.Level.TransitionTo -= onLevelTransitionTo;
        }

        private static void onLevelEnforceBounds(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Tracker>("GetEntity"))) {
                // the only usage of GetEntity gets the TheoCrystal entity on the screen.
                Logger.Log("SpringCollab2020/LeaveTheoBehindTrigger", $"Adding hook to leave Theo behind at {cursor.Index} in IL for Level.EnforceBounds");
                cursor.EmitDelegate<Func<TheoCrystal, TheoCrystal>>(theo => {
                    if (leaveTheoBehind) {
                        return null; // pretend there is no Theo, so we can exit the room.
                    }
                    return theo;
                });
            }
        }

        private static void onLevelTransitionTo(On.Celeste.Level.orig_TransitionTo orig, Level self, LevelData next, Vector2 direction) {
            if (leaveTheoBehind) {
                // freeze all Theo Crystals that the player isn't carrying, to prevent them from crashing the game or being weird.
                Player player = self.Tracker.GetEntity<Player>();
                foreach (TheoCrystal crystal in self.Tracker.GetEntities<TheoCrystal>()) {
                    if (player?.Holding?.Entity != crystal) {
                        crystal.RemoveTag(Tags.TransitionUpdate);
                    }
                }
            }

            orig(self, next, direction);
        }


        public LeaveTheoBehindTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            leaveTheoBehind = true;
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            leaveTheoBehind = false;
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            leaveTheoBehind = false;
        }
    }
}
