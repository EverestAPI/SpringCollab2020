using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    // Heavily based off the NoRefillTrigger class from vanilla, except reversing the state again on leave.
    [CustomEntity("SpringCollab2020/NoRefillField")]
    class NoRefillField : Trigger {
        public bool NoGroundRefillInside;

        public NoRefillField(EntityData data, Vector2 offset): base(data, offset) {
            NoGroundRefillInside = data.Bool("noGroundRefillInside");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            SceneAs<Level>().Session.Inventory.NoRefills = NoGroundRefillInside;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            SceneAs<Level>().Session.Inventory.NoRefills = !NoGroundRefillInside;
        }
    }
}
