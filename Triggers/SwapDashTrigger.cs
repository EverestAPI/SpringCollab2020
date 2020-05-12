using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/SwapDashTrigger")]
    class SwapDashTrigger : Trigger {
        private float enterX;

        public SwapDashTrigger(EntityData data, Vector2 offset)
            : base(data, offset) {
        }

        public override void OnEnter(Player player) {
            enterX = player.X;
        }

        public override void OnLeave(Player player) {
            Session session = SceneAs<Level>().Session;
            if (player.MaxDashes == 1 && (!(enterX + 16f >= player.X) || !(enterX - 16f <= player.X))) {
                session.Inventory.Dashes = 2;
            } else if (player.MaxDashes == 2 && (!(enterX + 16f >= player.X) || !(enterX - 16f <= player.X))) {
                session.Inventory.Dashes = 1;
                if (player.Dashes == 2) {
                    player.Dashes = 1;
                }
            }
        }
    }
}
