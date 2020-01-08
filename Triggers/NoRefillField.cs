using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    // Heavily based off the NoRefillTrigger class from vanilla, except reverting the state on leave.
    [CustomEntity("SpringCollab2020/NoRefillField")]
    [Tracked]
    class NoRefillField : Trigger {
        public NoRefillField(EntityData data, Vector2 offset): base(data, offset) { }

        public static void Load() {
            Everest.Events.Level.OnExit += onLevelExit;
        }

        public static void Unload() {
            Everest.Events.Level.OnExit -= onLevelExit;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            SceneAs<Level>().Session.Inventory.NoRefills = true;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            // re-enable refills if not colliding with another no refill field.
            if (player.Dead || !player.CollideCheck<NoRefillField>()) {
                SceneAs<Level>().Session.Inventory.NoRefills = false;
            }
        }

        private static void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            if (mode == LevelExit.Mode.SaveAndQuit) {
                // if saving in a No Refill Field, be sure to restore the refills.
                Player player = level.Tracker.GetEntity<Player>();
                if (player != null && player.CollideCheck<NoRefillField>()) {
                    level.Session.Inventory.NoRefills = false;
                }
            }
        }
    }
}
