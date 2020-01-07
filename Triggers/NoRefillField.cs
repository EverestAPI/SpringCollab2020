using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    // Heavily based off the NoRefillTrigger class from vanilla, except reverting the state on leave.
    [CustomEntity("SpringCollab2020/NoRefillField")]
    [Tracked]
    class NoRefillField : Trigger {
        public NoRefillField(EntityData data, Vector2 offset): base(data, offset) { }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            SceneAs<Level>().Session.Inventory.NoRefills = true;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            // re-enable refills if not colliding with another no refill field.
            if (!player.CollideCheck<NoRefillField>()) {
                SceneAs<Level>().Session.Inventory.NoRefills = false;
            }
        }
    }
}
